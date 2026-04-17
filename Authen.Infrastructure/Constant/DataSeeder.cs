using Authen.Infrastructure.DatabaseConfig;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.Constant
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AuthenDBContext context, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles 
            foreach (var role in UserRoles.ALL)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
