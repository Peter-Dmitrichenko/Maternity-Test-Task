using System.Text;
using System.Text.Json;

namespace Populate
{

    class Program
    {
        static readonly string baseUrl = "http://localhost:8080";
        static readonly string endpoint = "/api/Patients";
        static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        static readonly string[] familyNames = new[]
        {
            "Smith","Johnson","Williams","Brown","Jones","Garcia","Miller","Davis","Rodriguez","Martinez"
        };

        static readonly string[] givenNames = new[]
        {
            "James","Mary","John","Patricia","Robert","Jennifer","Michael","Linda","William","Elizabeth",
            "David","Barbara","Richard","Susan","Joseph","Jessica","Thomas","Sarah","Charles","Karen"
        };

        static readonly string[] genders = new[] { "male", "female", "other" };

        static async Task Main(string[] args)
        {
            using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

            var createdCount = 0;
            var failed = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                var patient = GenerateRandomPatient();
                var json = JsonSerializer.Serialize(patient, jsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var resp = await http.PostAsync(endpoint, content);
                    if (resp.IsSuccessStatusCode)
                    {
                        createdCount++;
                        Console.WriteLine($"[{i + 1}] Created patient {patient.Id} - Status: {resp.StatusCode}");
                    }
                    else
                    {
                        var body = await resp.Content.ReadAsStringAsync();
                        failed.Add($"#{i + 1} Id={patient.Id} Status={(int)resp.StatusCode} {resp.ReasonPhrase} Body={Truncate(body, 300)}");
                        Console.WriteLine($"[{i + 1}] Failed to create patient {patient.Id} - Status: {resp.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    failed.Add($"#{i + 1} Id={patient.Id} Exception={ex}");
                    Console.WriteLine($"[{i + 1}] Exception creating patient {patient.Id}: {ex}");
                }

                // Optional small delay to avoid overwhelming the server
                await Task.Delay(50);
            }

            Console.WriteLine();
            Console.WriteLine($"Finished. Created: {createdCount}, Failed: {failed.Count}");
            if (failed.Count > 0)
            {
                Console.WriteLine("Failures:");
                foreach (var f in failed) Console.WriteLine(f);
            }
        }

        static Patient GenerateRandomPatient()
        {
            var rnd = RandomProvider.GetThreadRandom();
            var family = familyNames[rnd.Next(familyNames.Length)];
            var given1 = givenNames[rnd.Next(givenNames.Length)];
            string? given2 = rnd.NextDouble() < 0.3 ? givenNames[rnd.Next(givenNames.Length)] : null;

            var name = new Name
            {
                Use = "official",
                Family = family,
                Given = given2 == null ? new List<string> { given1 } : new List<string> { given1, given2 }
            };

            var birthDate = RandomBirthDate(rnd, 1920, 2020);

            return new Patient
            {
                Name = name,
                Gender = genders[rnd.Next(genders.Length)],
                BirthDate = birthDate,
                Active = (rnd.NextDouble() < 0.9).ToString()
            };
        }

        static DateTime RandomBirthDate(Random rnd, int startYear, int endYear)
        {
            var year = rnd.Next(startYear, endYear + 1);
            var month = rnd.Next(1, 13);
            var day = rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : string.Concat(s.AsSpan(0, max), "...");
        }
    }

    static class RandomProvider
    {
        private static readonly ThreadLocal<Random> threadLocal =
            new(() => {
                return new Random(Guid.NewGuid().GetHashCode());
            });

        public static Random GetThreadRandom() => threadLocal.Value!;
    }
}
