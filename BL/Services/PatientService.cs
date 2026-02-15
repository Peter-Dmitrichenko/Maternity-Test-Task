using DB;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DTO;
using DTO.Models;
using DB.Models;
using BL.Interfaces;

namespace BL.Services
{
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _db;
        private readonly IDateSearchService _dateSearch;
        private readonly IMapper _mapper;

        public PatientService(AppDbContext db,
            IDateSearchService dateSearch,
            IMapper automapper)
        {
            _db = db;
            _dateSearch = dateSearch;
			_mapper = automapper;
        }

        public async Task<IEnumerable<PatientDTO>> GetAllAsync(string[]? birthdate = null)
        {
            var query = _db.Patients.Include(p => p.Name).AsNoTracking();

            var filteredQuery = _dateSearch.ApplyBirthDateFilter(query, birthdate);

            var list = await filteredQuery.ToListAsync();
			return _mapper.Map<IEnumerable<PatientDTO>>(_dateSearch.FilterByBirthDateInMemory(list, birthdate));
        }

        public async Task<PatientDTO?> GetByIdAsync(Guid id)
        {
            var record = await _db.Patients
                .Include(p => p.Name)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return _mapper.Map<PatientDTO>(record);
        }

        public async Task<PatientDTO> CreateAsync(PatientDTO input)
        {
            input.Id = Guid.NewGuid();
            if (input.Name != null && input.Name.Id == Guid.Empty) input.Name.Id = Guid.NewGuid();
            if (input.Name != null) input.Name.PatientId = input.Id;


            var patient = _mapper.Map<Patient>(input);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
			
            return _mapper.Map<PatientDTO>(patient); 
        }

        public async Task<bool> UpdateAsync(Guid id, PatientDTO input)
        {
            if (input == null || id != input.Id) return false;

            var existing = await _db.Patients
                .Include(p => p.Name)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existing == null) return false;

			var patient = _mapper.Map<Patient>(input);

			existing.GenderId = patient.GenderId;
            existing.BirthDate = patient.BirthDate;
            existing.ActiveId = patient.ActiveId;

            if (input.Name == null)
            {
                if (existing.Name != null)
                    _db.Names.Remove(existing.Name);
            }
            else
            {
                if (existing.Name == null)
                {
                    input.Name.Id = input.Name.Id == Guid.Empty ? Guid.NewGuid() : input.Name.Id;
                    input.Name.PatientId = existing.Id;
                    existing.Name = patient.Name;
                    _db.Names.Add(existing.Name);
                }
                else
                {
                    existing.Name.Use = input.Name.Use;
                    existing.Name.Family = input.Name.Family;
                    existing.Name.Given = input.Name.Given ?? new List<string>();
                }
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var person = await _db.Patients
                .Include(p => p.Name)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null) return false;

            _db.Patients.Remove(person);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
