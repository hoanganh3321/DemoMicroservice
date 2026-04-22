using Authen.Domain.Entities;
using Authen.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.DatabaseConfig
{

    public class AuthenDBContext(DbContextOptions<AuthenDBContext> options) : IdentityDbContext<Authen.Infrastructure.Identity.User, IdentityRole, string>(options)
    {
        //User
        public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public virtual DbSet<StaffProfile> StaffProfiles { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AuthenDBContext).Assembly);
            builder.Ignore<Authen.Domain.Entities.User>();
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            builder.Entity<Authen.Infrastructure.Identity.User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                //Identity
                entity.Property(u => u.UserName)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.NormalizedUserName)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.NormalizedEmail)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.PhoneNumber)
                      .HasMaxLength(16);

                entity.Property(u => u.PasswordHash)
                      .HasMaxLength(256);

                entity.Property(u => u.SecurityStamp)
                      .HasMaxLength(100);

                entity.Property(u => u.ConcurrencyStamp)
                      .HasMaxLength(100);

                //extend
                entity.Property(u => u.RefreshToken)
                      .HasMaxLength(512);

                entity.Property(u => u.RefreshTokenExpiryTime);

                entity.Property(u => u.UserStatus)
                      .HasConversion<byte>()
                      .IsRequired();

                entity.Property(u => u.FullName)
                      .HasMaxLength(128);

                entity.Property(u => u.Avatar)
                      .HasMaxLength(256);

                entity.Property(u => u.Address)
                      .HasMaxLength(256)
                      .IsRequired(false);

                entity.Property(u => u.Gender)
                      .HasColumnType("bit");

                // Audit fields
                entity.Property(u => u.CreatedAt)
                      .IsRequired();

                entity.Property(u => u.LastModifiedAt);

                entity.Property(u => u.CreatedBy)
                      .HasMaxLength(100);

                entity.Property(u => u.ModifiedBy)
                      .HasMaxLength(100);

                // Soft Delete
                entity.Property(u => u.IsDeleted)
                      .IsRequired()
                      .HasDefaultValue(false);

                entity.Property(u => u.DeletedAt);

                // Soft Delete
                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(r => r.Name).HasMaxLength(256);
                entity.Property(r => r.NormalizedName).HasMaxLength(256);
                entity.Property(r => r.ConcurrencyStamp).HasMaxLength(100);
            });

            builder.Entity<CustomerProfile>(entity =>
            {
                entity.HasKey(cp => cp.Id);
                entity.Ignore(cp => cp.User);

                entity.Property(cp => cp.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(cp => cp.UserId)
                    .IsRequired()
                    .HasMaxLength(450); 

                entity.HasOne<Authen.Infrastructure.Identity.User>()
                    .WithOne(u => u.CustomerProfile)
                    .HasForeignKey<CustomerProfile>(cp => cp.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(cp => cp.UserId).IsUnique(); 
            });

            builder.Entity<StaffProfile>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Ignore(s => s.User);

                entity.Property(s => s.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(s => s.UserId)
                    .IsRequired()
                    .HasMaxLength(450); 

                entity.Property(s => s.EmployeeCode)
                    .HasMaxLength(128)
                    .IsRequired();

                entity.Property(s => s.Notes)
                    .HasMaxLength(512); 

                entity.HasOne<Authen.Infrastructure.Identity.User>()
                    .WithOne(u => u.StaffProfile)
                    .HasForeignKey<StaffProfile>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(s => s.UserId).IsUnique(); 
            });
        }
    }
}
