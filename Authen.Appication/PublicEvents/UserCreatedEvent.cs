using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Application.PublicEvents
{
        public record UserCreatedEvent(string UserId, string Email);
        public record UserLoginEvent( bool IsSuccess);
}
