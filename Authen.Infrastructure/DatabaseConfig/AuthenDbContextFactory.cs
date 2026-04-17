using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.DatabaseConfig
{
    public class AuthenDbContextFactory : IDesignTimeDbContextFactory<AuthenDBContext>
    {
        public AuthenDBContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AuthenService.WebApi");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(basePath))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<AuthenDBContext>();

            builder.UseSqlServer(
                configuration.GetConnectionString("DB"),
                sqlOptions => sqlOptions.MigrationsAssembly("Authen.Infrastructure"));

            return new AuthenDBContext(builder.Options);
        }
    }
}
