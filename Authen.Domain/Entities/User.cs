using Authen.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Domain.Entities
{
    public class User
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpriryTime { get; set; }
        public UserStatus UserStatus { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string Address { get; set; } = null!;
        public bool? Gender { get; set; }
        public DateTime CreateAt { get; set; }

        public virtual StaffProfile? StaffProfile { get; set; }
        public virtual CustomerProfile? CustomerProfile { get; set; }
    }
}
