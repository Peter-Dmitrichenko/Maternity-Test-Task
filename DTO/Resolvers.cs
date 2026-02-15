using AutoMapper;
using DB;
using DB.Lookups;
using DB.Models;
using DTO.Models;
using Microsoft.EntityFrameworkCore;

namespace DTO
{
    public class GenderIdToCodeResolver : IValueResolver<Patient, PatientDTO, string?>
    {
        private readonly AppDbContext _db;
        public GenderIdToCodeResolver(AppDbContext db) => _db = db;

        public string Resolve(Patient source, PatientDTO destination, string? destMember, ResolutionContext context)
        {
            var g = _db.Genders.Find(source.GenderId);
            return g?.Code ?? DefaultValues.Default_Gender;
        }
    }

    public class ActiveIdToCodeResolver : IValueResolver<Patient, PatientDTO, bool?>
    {
        private readonly AppDbContext _db;
        public ActiveIdToCodeResolver(AppDbContext db) => _db = db;

        public bool? Resolve(Patient source, PatientDTO destination, bool? destMember, ResolutionContext context)
        {
            if (source == null) return false;

            var a = _db.Actives.AsNoTracking().FirstOrDefault(x => x.Id == source.ActiveId);
            if (a == null || string.IsNullOrWhiteSpace(a.Code))
                return false;

            if (bool.TryParse(a.Code, out var result))
                return result;

            return false;
        }
    }

    public class GenderCodeToIdResolver : IValueResolver<PatientDTO, Patient, int>
    {
        private readonly AppDbContext _db;

        public GenderCodeToIdResolver(AppDbContext db)
        {
            _db = db;
        }

        public int Resolve(PatientDTO source, Patient destination, int destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Gender)) return _db.Genders.First(g => g.Code == DefaultValues.Default_Gender).Id;
            var code = source.Gender.Trim().ToLowerInvariant();


            var g = _db.Genders.FirstOrDefault(x => x.Code.ToLower() == code);

            if (g != null) return g.Id;

            return _db.Genders.First(g => g.Code == DefaultValues.Default_Gender).Id;
        }
    }

    public class ActiveCodeToIdResolver : IValueResolver<PatientDTO, Patient, int>
    {
        private readonly AppDbContext _db;
        public ActiveCodeToIdResolver(AppDbContext db) => _db = db;

        public int Resolve(PatientDTO source, Patient destination, int destMember, ResolutionContext context)
        {
            var code = (source.Active ?? DefaultValues.Default_Active);
            var a = _db.Actives.FirstOrDefault(x => x.Code.ToLower() == code.ToString());
            if (a != null) return a.Id;

            return _db.Actives.First(x => x.Code.ToLower() == DefaultValues.Default_Active.ToString()).Id;
        }
    }
}
