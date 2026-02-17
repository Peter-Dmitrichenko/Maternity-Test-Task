using DB.Models;


namespace DB.Cache
{
    public interface ILookupCache
    {
        IEnumerable<GenderLookup> Genders { get; }
        IEnumerable<ActiveLookup> Actives { get; }
        void ResetGenders();
        void ResetActives();
    }
}
