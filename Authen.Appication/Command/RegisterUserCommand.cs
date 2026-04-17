using Authen.Application.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Application.Command
{
        public record RegisterUserCommand(CreateUserModel CreateUserModel);
}
