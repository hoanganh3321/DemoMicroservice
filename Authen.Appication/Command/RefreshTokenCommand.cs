using Authen.Application.Common;
using Authen.Application.Models.User;
using MediatR;

namespace Authen.Application.Command
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<ServiceResult<LoginResponseModel>>;
}
