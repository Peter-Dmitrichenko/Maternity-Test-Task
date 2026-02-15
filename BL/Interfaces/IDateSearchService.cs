using DB.Models;

namespace BL.Interfaces
{

    public interface IDateSearchService
    {
        IQueryable<Patient> ApplyBirthDateFilter(IQueryable<Patient> query, string[]? birthdateParams);
        IEnumerable<Patient> FilterByBirthDateInMemory(IEnumerable<Patient> patients, string[]? birthdateParam);
    }

}
