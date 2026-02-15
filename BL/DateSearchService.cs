using DB.Models;
using System.Globalization;

namespace BL
{
    public class DateSearchService : IDateSearchService
    {
        public IQueryable<Patient> ApplyBirthDateFilter(IQueryable<Patient> query, string[]? birthdateParams)
        {
            if (birthdateParams == null || !birthdateParams.Any())
                return query;

            foreach (var param in birthdateParams)
            {
                if (string.IsNullOrWhiteSpace(param)) continue;

                if (!TryParseDateParameter(param.Trim(), out var prefix, out var testStart, out var testEnd, out var precision))
                    throw new ArgumentException($"Invalid birthdate parameter format: {param}");

                switch (prefix)
                {
                    case "eq":
                        query = query.Where(p => p.BirthDate >= testStart && p.BirthDate < testEnd);
                        break;
                    case "ge":
                        query = query.Where(p => p.BirthDate >= testStart);
                        break;
                    case "le":
                        query = query.Where(p => p.BirthDate < testEnd);
                        break;
                    case "lt":
                        query = query.Where(p => p.BirthDate < testStart);
                        break;
                    case "gt":
                        query = query.Where(p => p.BirthDate >= testEnd);
                        break;
                    // For unsupported prefixes (ne, sa, eb, ap) we do nothing in SQL – they must be handled in memory.
                    default:
                        break;
                }
            }
            return query;
        }

        public IEnumerable<Patient> FilterByBirthDateInMemory(IEnumerable<Patient> patients, string[]? birthdateParams)
        {
            if (birthdateParams == null || !birthdateParams.Any())
                return patients;

            foreach (var param in birthdateParams)
            {
                if (string.IsNullOrWhiteSpace(param)) continue;

                if (!TryParseDateParameter(param.Trim(), out var prefix, out var testStart, out var testEnd, out var precision))
                    throw new ArgumentException($"Invalid birthdate parameter format: {param}");

                patients = patients.Where(p =>
                {
                    var resStart = p.BirthDate;
                    DateTime resEnd;

                    // Heuristic: if time component is zero, treat as date precision (whole day)
                    if (resStart.TimeOfDay == TimeSpan.Zero)
                        resEnd = resStart.AddDays(1);
                    else
                        resEnd = resStart.AddTicks(1); 

                    switch (prefix)
                    {
                        case "eq": return IntervalsIntersect(resStart, resEnd, testStart, testEnd);
                        case "ne": return !IntervalsIntersect(resStart, resEnd, testStart, testEnd);
                        case "lt": return resStart < testStart;
                        case "gt": return resEnd > testEnd;
                        case "le": return resStart <= testEnd;
                        case "ge": return resEnd >= testStart;
                        case "sa": return resStart >= testEnd;   // starts after (inclusive of testEnd)
                        case "eb": return resEnd <= testStart;   // ends before (inclusive of testStart)
                        case "ap":
                            var (apStart, apEnd) = ExpandApproximateInterval(testStart, testEnd, precision);
                            return IntervalsIntersect(resStart, resEnd, apStart, apEnd);
                        default: return IntervalsIntersect(resStart, resEnd, testStart, testEnd);
                    }
                });
            }
            return patients;
        }

        private static bool IntervalsIntersect(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
            => aStart < bEnd && bStart < aEnd;

        private static (DateTime, DateTime) ExpandApproximateInterval(DateTime start, DateTime end, DatePrecision precision)
        {
            return precision switch
            {
                DatePrecision.Year => (start.AddYears(-1), end.AddYears(1)),
                DatePrecision.Month => (start.AddMonths(-1), end.AddMonths(1)),
                DatePrecision.Day => (start.AddDays(-1), end.AddDays(1)),
                _ => (start.AddHours(-1), end.AddHours(1)),
            };
        }

        private static bool TryParseDateParameter(string raw, out string prefix, out DateTime start, out DateTime end, out DatePrecision precision)
        {
            prefix = "eq";
            start = default;
            end = default;
            precision = DatePrecision.Day;
            if (string.IsNullOrEmpty(raw)) return false;

            var knownPrefixes = new[] { "eq", "ne", "lt", "gt", "le", "ge", "sa", "eb", "ap" };
            var lower = raw.ToLowerInvariant();
            var detected = knownPrefixes.FirstOrDefault(p => lower.StartsWith(p));
            var valuePart = string.IsNullOrEmpty(detected) ? raw : raw[detected.Length..];
            if (!string.IsNullOrWhiteSpace(detected)) prefix = detected;
            valuePart = valuePart.Trim();
            if (string.IsNullOrEmpty(valuePart)) return false;

            // If timezone present parse as DateTimeOffset -> UTC
            if (valuePart.EndsWith("Z", StringComparison.OrdinalIgnoreCase) || valuePart.Contains("+") || valuePart.LastIndexOf('-', 1) > 0)
            {
                if (!DateTimeOffset.TryParse(valuePart, null, DateTimeStyles.RoundtripKind, out var dto)) return false;
                start = dto.UtcDateTime;
                precision = DeterminePrecision(valuePart);
                end = ComputeIntervalEnd(start, precision);
                return true;
            }

            // Year
            if (DateTime.TryParseExact(valuePart, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtYear))
            {
                precision = DatePrecision.Year;
                start = new DateTime(dtYear.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);
                end = start.AddYears(1);
                return true;
            }

            // Year-month
            if (DateTime.TryParseExact(valuePart, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtMonth))
            {
                precision = DatePrecision.Month;
                start = new DateTime(dtMonth.Year, dtMonth.Month, 1, 0, 0, 0, DateTimeKind.Local);
                end = start.AddMonths(1);
                return true;
            }

            // Date
            if (DateTime.TryParseExact(valuePart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtDay))
            {
                precision = DatePrecision.Day;
                start = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, 0, 0, 0, DateTimeKind.Local);
                end = start.AddDays(1);
                return true;
            }

            // DateTime formats
            var dtFormats = new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFF" };
            foreach (var fmt in dtFormats)
            {
                if (DateTime.TryParseExact(valuePart, fmt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                {
                    precision = fmt.Contains("FFF") ? DatePrecision.Millisecond :
                                fmt.Contains("ss") ? DatePrecision.Second :
                                DatePrecision.Minute;
                    start = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                    end = ComputeIntervalEnd(start, precision);
                    return true;
                }
            }

            // Fallback
            if (DateTime.TryParse(valuePart, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtAny))
            {
                precision = DeterminePrecision(valuePart);
                start = DateTime.SpecifyKind(dtAny, DateTimeKind.Local);
                end = ComputeIntervalEnd(start, precision);
                return true;
            }

            return false;
        }

        private static DateTime ComputeIntervalEnd(DateTime start, DatePrecision precision)
        {
            return precision switch
            {
                DatePrecision.Year => start.AddYears(1),
                DatePrecision.Month => start.AddMonths(1),
                DatePrecision.Day => start.AddDays(1),
                DatePrecision.Minute => start.AddMinutes(1),
                DatePrecision.Second => start.AddSeconds(1),
                DatePrecision.Millisecond => start.AddMilliseconds(1),
                _ => start.AddDays(1),
            };
        }

        private static DatePrecision DeterminePrecision(string s)
        {
            if (string.IsNullOrEmpty(s)) return DatePrecision.Day;
            if (s.Length == 4 && int.TryParse(s, out _)) return DatePrecision.Year;
            if (s.Length >= 7 && s[4] == '-' && s[7] == '-') return s.Contains("T") ? DatePrecision.Day : DatePrecision.Month;
            if (s.Contains("T"))
            {
                if (s.Contains(".")) return DatePrecision.Millisecond;
                if (s.Count(c => c == ':') >= 2) return DatePrecision.Second;
                return DatePrecision.Minute;
            }
            return DatePrecision.Day;
        }



        private enum DatePrecision { Year, Month, Day, Minute, Second, Millisecond }
    }
}