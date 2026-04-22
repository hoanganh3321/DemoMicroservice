using Authen.Application.Common;
using MediatR;

namespace Authen.Application.Command
{
    public record RevokeTokenCommand(string UserId) : IRequest<ServiceResult<string>>;
}
