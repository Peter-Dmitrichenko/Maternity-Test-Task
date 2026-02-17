using DB.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DB.Cache
{
    public class LookupCache : ILookupCache
    {
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private const string GendersKey = "lookup_genders";
        private const string ActivesKey = "lookup_actives";

        private readonly TimeSpan span = TimeSpan.FromHours(1);
        public LookupCache(IMemoryCache cache, IServiceScopeFactory scopeFactory)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        public IEnumerable<GenderLookup> Genders
        {
            get
            {
                return _cache.GetOrCreate(GendersKey, entry =>
                {
                    // Optional: set expiration (e.g., 24 hours) as a safety net
                    entry.AbsoluteExpirationRelativeToNow = span;

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    // Synchronous load – ToList() blocks, but this runs only once
                    return dbContext.Genders.ToList();
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

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    return dbContext.Actives.ToList();
                });
            }
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