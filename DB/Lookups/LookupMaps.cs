using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Lookups
{
    public static class LookupSeedIds
    {
        // Gender
        public const int Gender_Male = 1;
        public const int Gender_Female = 2;
        public const int Gender_Other = 3;
        public const int Gender_Unknown = 4;

        // Active
        public const int Active_True = 1;
        public const int Active_False = 2;
    }

    public static class DefaultValues
    {
        public static readonly string Default_Gender = "unknown";
        public static readonly bool Default_Active = false;
    }

    public static class LookupMaps
    {
        public static readonly IReadOnlyDictionary<string, int> GenderCodeToId =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["male"] = LookupSeedIds.Gender_Male,
                ["female"] = LookupSeedIds.Gender_Female,
                ["other"] = LookupSeedIds.Gender_Other,
                ["unknown"] = LookupSeedIds.Gender_Unknown
            };

        public static readonly IReadOnlyDictionary<int, string> GenderIdToCode =
            GenderCodeToId.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static readonly IReadOnlyDictionary<string, int> ActiveCodeToId =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["true"] = LookupSeedIds.Active_True,
                ["false"] = LookupSeedIds.Active_False
            };

        public static readonly IReadOnlyDictionary<int, string> ActiveIdToCode =
            ActiveCodeToId.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static int GenderCodeToIdOrDefault(string code) =>
            string.IsNullOrWhiteSpace(code) ? LookupSeedIds.Gender_Unknown
            : (GenderCodeToId.TryGetValue(code, out var id) ? id : LookupSeedIds.Gender_Unknown);

        public static string GenderIdToCodeOrDefault(int id) =>
            GenderIdToCode.TryGetValue(id, out var code) ? code : "unknown";

        public static int ActiveCodeToIdOrDefault(string code) =>
            string.IsNullOrWhiteSpace(code) ? LookupSeedIds.Active_True
            : (ActiveCodeToId.TryGetValue(code, out var id) ? id : LookupSeedIds.Active_False);

        public static string ActiveIdToCodeOrDefault(int id) =>
            ActiveIdToCode.TryGetValue(id, out var code) ? code : "false";
    }

}
