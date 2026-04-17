using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Domain.Enum
{
    public enum UserStatus : byte
    {
        Block,
        Inactive,
        Active,
        decline
    }
}
