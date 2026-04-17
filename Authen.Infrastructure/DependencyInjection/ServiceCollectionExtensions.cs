using Authen.Infrastructure.Constant;
using Authen.Infrastructure.DatabaseConfig;
using Authen.Infrastructure.Identity;
using Authen.Infrastructure.Mappings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Authen.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Đăng ký DbContext
            services.AddDatabaseContext(configuration);

            // 2. Đăng ký Identity 
            services.AddIdentityConfig();

            // 3. Tự động register tất cả Repository bằng Scrutor
            services.AddRepositories();

            // 4. AutoMapper
            services.AddAutoMapper(
                typeof(Authen.Application.Common.AssemblyReference).Assembly,
                typeof(UserMappingProfile).Assembly);

            // 5. MediatR 
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(
                    typeof(Authen.Application.Common.AssemblyReference).Assembly);
            });


            return services;
        }

        private static IServiceCollection AddDatabaseContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AuthenDBContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DB"),
                    sqlOptions => sqlOptions.MigrationsAssembly("Authen.Infrastructure") // Migration
                );
            });

            return services;
        }


        private static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

            })
                  .AddEntityFrameworkStores<AuthenDBContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        /// <summary>
        /// Tự động đăng ký tất cả Repository theo Convention
        /// </summary>
        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(AuthenDBContext).Assembly)

                .AddClasses(classes => classes
                    .Where(type =>
                        type.Name.EndsWith("Repository") &&       // Tên class kết thúc bằng "Repository"
                        !type.IsAbstract &&
                        !type.IsGenericTypeDefinition))
                .AsImplementedInterfaces()                        // Register theo Interface
                .WithScopedLifetime());                           // Lifetime = Scoped

            return services;
        }
    }
}

