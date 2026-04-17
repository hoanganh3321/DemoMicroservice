using Authen.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.Identity
{
    public class User : IdentityUser<string>   
    {

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public UserStatus UserStatus { get; set; } = UserStatus.Active;

        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool? Gender { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Constructor
        public User()
        {
            UserStatus = UserStatus.Active;

            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }
    }
}
