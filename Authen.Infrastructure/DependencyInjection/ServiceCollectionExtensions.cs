
using Authen.Application.EventBus;
using Authen.Application.Interface;
using Authen.Infrastructure.Constant;
using Authen.Infrastructure.DatabaseConfig;
using Authen.Infrastructure.EventBus;
using Authen.Infrastructure.Identity;
using Authen.Infrastructure.Implement;
using Authen.Infrastructure.Mappings;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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
            services.AddJwtConfig(configuration);

            // 3. Tự động register tất cả Repository bằng Scrutor
            services.AddRepositories();           
            
            // 4. AutoMapper
            services.AddAutoMapper(
                typeof(Authen.Application.Common.AssemblyReference),
                typeof(UserMappingProfile));

            // 5. MediatR 
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(
                    typeof(Authen.Application.Common.AssemblyReference).Assembly);
            });

            // MassTransit + RabbitMQ
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var rabbitUsername = configuration["RabbitMQ:Username"]
                        ?? throw new InvalidOperationException("Thiếu cấu hình RabbitMQ:Username.");
                    var rabbitPassword = configuration["RabbitMQ:Password"]
                        ?? throw new InvalidOperationException("Thiếu cấu hình RabbitMQ:Password.");

                    cfg.Host(configuration["RabbitMQ:Host"], h =>
                    {
                        h.Username(rabbitUsername);
                        h.Password(rabbitPassword);
                    });
                });
            });
            services.AddScoped<IEventBus, MassTransitEventBus>();

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

        private static IServiceCollection AddJwtConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtConfig>(configuration.GetSection("Jwt"));
            var jwt = configuration.GetSection("Jwt").Get<JwtConfig>()
                ?? throw new InvalidOperationException("Thiếu cấu hình Jwt.");

            if (string.IsNullOrWhiteSpace(jwt.SecretKey))
                throw new InvalidOperationException("Thiếu Jwt:SecretKey.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();
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

