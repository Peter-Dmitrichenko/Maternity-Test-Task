using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace DB
{

    public static class DbInitializer
    {
        public static async Task EnsureLookupsInitializedAsync(AppDbContext ctx)
        {
            await ctx.Database.EnsureCreatedAsync();

            if (!await ctx.Genders.AnyAsync())
            {
                ctx.Genders.AddRange(
                    new GenderLookup { Code = "male", Display = "Male" },
                    new GenderLookup { Code = "female", Display = "Female" },
                    new GenderLookup { Code = "other", Display = "Other" },
                    new GenderLookup { Code = "unknown", Display = "Unknown" }
                );
            }

            if (!await ctx.Actives.AnyAsync())
            {
                ctx.Actives.AddRange(
                    new ActiveLookup { Code = "true", Display = "true" },
                    new ActiveLookup { Code = "false", Display = "false" }
                );
            }

            await ctx.SaveChangesAsync();
        }
    }

}
