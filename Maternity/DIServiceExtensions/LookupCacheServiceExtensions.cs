namespace DB.Cache
{
    public static class LookupCacheServiceExtensions
    {
        public static IServiceCollection AddLookupCache(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddScoped<ILookupCache, LookupCache>();

            return services;
        }
    }
}
