using AutoMapper;
using DB.Cache;
using DB.Lookups;
using DB.Models;
using DTO.Models;

namespace DTO
{
    public class GenderIdToCodeResolver : IValueResolver<Patient, PatientDTO, string?>
    {
        private readonly ILookupCache _cache;
        public GenderIdToCodeResolver(ILookupCache cache) => _cache = cache;

        public string Resolve(Patient source, PatientDTO destination, string? destMember, ResolutionContext context)
        {
            var g = _cache.Genders.FirstOrDefault(e=>e.Id == source.GenderId);
            return g?.Code ?? DefaultValues.Default_Gender;
        }
    }

    public class ActiveIdToCodeResolver : IValueResolver<Patient, PatientDTO, bool?>
    {
        private readonly ILookupCache _cache;
        public ActiveIdToCodeResolver(ILookupCache cache) => _cache = cache;

        public bool? Resolve(Patient source, PatientDTO destination, bool? destMember, ResolutionContext context)
        {
            if (source == null) return false;

            var a = _cache.Actives.FirstOrDefault(x => x.Id == source.ActiveId);
            if (a == null || string.IsNullOrWhiteSpace(a.Code))
                return false;

            if (bool.TryParse(a.Code, out var result))
                return result;

            return false;
        }
    }

    public class GenderCodeToIdResolver : IValueResolver<PatientDTO, Patient, int>
    {
        private readonly ILookupCache _cache;
        public GenderCodeToIdResolver(ILookupCache cache) => _cache = cache;

        public int Resolve(PatientDTO source, Patient destination, int destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Gender)) 
                return _cache.Genders.First(g => g.Code == DefaultValues.Default_Gender).Id;

            var code = source.Gender.Trim().ToLowerInvariant();


            var g = _cache.Genders.FirstOrDefault(x => x.Code.ToLower() == code);

            if (g != null) return g.Id;

            return _cache.Genders.First(g => g.Code == DefaultValues.Default_Gender).Id;
        }
    }

    public class ActiveCodeToIdResolver : IValueResolver<PatientDTO, Patient, int>
    {
        private readonly ILookupCache _cache;
        public ActiveCodeToIdResolver(ILookupCache cache) => _cache = cache;

        public int Resolve(PatientDTO source, Patient destination, int destMember, ResolutionContext context)
        {
            var code = (source.Active ?? DefaultValues.Default_Active);
            var a = _cache.Actives.FirstOrDefault(x => x.Code.ToLower() == code.ToString());
            if (a != null) return a.Id;

            return _cache.Actives.First(x => x.Code.ToLower() == DefaultValues.Default_Active.ToString()).Id;
        }
    }
}
