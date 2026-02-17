using DB.Models;


namespace DB.Cache
{
    public interface ILookupCache
    {
        IEnumerable<GenderLookup> Genders { get; }
        IEnumerable<ActiveLookup> Actives { get; }
        void Preload(); 
        void ResetGenders();
        void ResetActives();
    }
}
