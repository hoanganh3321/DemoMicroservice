using Authen.Application.Command;
using Authen.Application.Common;
using Authen.Application.Interface;
using Authen.Application.Models.User;
using MediatR;

namespace Authen.Application.Handler
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ServiceResult<LoginResponseModel>>
    {
        private readonly IIdentityRepository _identityRepository;

        public RefreshTokenHandler(IIdentityRepository identityRepository)
        {
            _identityRepository = identityRepository;
        }

        public async Task<ServiceResult<LoginResponseModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshed = await _identityRepository.RefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (!refreshed.IsSuccess || string.IsNullOrWhiteSpace(refreshed.AccessToken) || string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                return ServiceResult<LoginResponseModel>.Fail(refreshed.ErrorMessage ?? "Refresh token không hợp lệ.", 401);

            return ServiceResult<LoginResponseModel>.SuccessResult(
                new LoginResponseModel
                {
                    AccessToken = refreshed.AccessToken,
                    RefreshToken = refreshed.RefreshToken,
                    ExpiresInMinutes = refreshed.ExpiresInMinutes,
                    Roles = refreshed.Roles
                },
                "Làm mới token thành công.",
                200);
        }
    }
}
