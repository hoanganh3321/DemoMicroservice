using Authen.Application.Command;
using Authen.Application.Common;
using Authen.Application.Interface;
using MediatR;

namespace Authen.Application.Handler
{
    public class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, ServiceResult<string>>
    {
        private readonly IIdentityRepository _identityRepository;

        public RevokeTokenHandler(IIdentityRepository identityRepository)
        {
            _identityRepository = identityRepository;
        }

        public async Task<ServiceResult<string>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            var isRevoked = await _identityRepository.RevokeRefreshTokenAsync(request.UserId, cancellationToken);
            if (!isRevoked)
                return ServiceResult<string>.Fail("Không thể thu hồi refresh token.", 400);

            return ServiceResult<string>.SuccessResult("ok", "Thu hồi refresh token thành công.", 200);
        }
    }
}
