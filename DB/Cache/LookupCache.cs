using DB.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DB.Cache
{
    public class LookupCache : ILookupCache
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;
        private const string GendersKey = "lookup_genders";
        private const string ActivesKey = "lookup_actives";

        private readonly TimeSpan span = TimeSpan.FromHours(1);
        public LookupCache(IMemoryCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public IEnumerable<GenderLookup> Genders
        {
            get
            {
                return _cache.GetOrCreate(GendersKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = span;

                    return _context.Genders.ToList();
                });
            }
        }

        public IEnumerable<ActiveLookup> Actives
        {
            get
            {
                return _cache.GetOrCreate(ActivesKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = span;

                    return _context.Actives.ToList();
                });
            }
        }

        public void Preload()
        {
            _ = Genders.ToList();
            _ = Actives.ToList();
        }

        public void ResetGenders()
        {
            _cache.Remove(GendersKey);
        }

        public void ResetActives()
        {
            _cache.Remove(ActivesKey);
        }
    }
}