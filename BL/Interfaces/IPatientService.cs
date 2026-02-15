using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.Models;

namespace BL.Interfaces
    {
        public interface IPatientService
        {
            Task<IEnumerable<PatientDTO>> GetAllAsync(string[]? birthdate = null);
            Task<PatientDTO?> GetByIdAsync(Guid id);
            Task<PatientDTO> CreateAsync(PatientDTO input);
            Task<bool> UpdateAsync(Guid id, PatientDTO input);
            Task<bool> DeleteAsync(Guid id);
        }
    }
