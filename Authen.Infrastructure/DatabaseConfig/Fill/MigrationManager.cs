using Authen.Infrastructure.Constant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.DatabaseConfig.Fill
{
    public static class MigrationManager
    {
        public static async Task<WebApplication> MigrateDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<AuthenDBContext>())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    await context.Database.MigrateAsync();

                    await DataSeeder.SeedAsync(context, roleManager);
                }
            }
            return app;
        }
    }
}
