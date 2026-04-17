using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.Constant
{
    public static class UserRoles
    {
        public const string CUSTOMER = "CUSTOMER";
        public const string STAFF = "STAFF";
        public const string ADMIN = "ADMIN";

        public static readonly string[] ALL =
        [
            CUSTOMER,
            STAFF,
            ADMIN, 
       ];
    }
}
