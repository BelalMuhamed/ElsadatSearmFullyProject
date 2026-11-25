using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Applies the pending EF Core migrations, then runs the seed (D20: every environment, at startup).
        /// </summary>
        public static async Task MigrateAndSeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await context.Database.MigrateAsync();
            await DbInitializer.SeedAsync(context, configuration);
        }
    }
}