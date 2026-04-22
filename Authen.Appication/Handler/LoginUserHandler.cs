using Authen.Application.Command;
using Authen.Application.Common;
using Authen.Application.EventBus;
using Authen.Application.Interface;
using Authen.Application.Models.User;
using Authen.Application.PublicEvents;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Application.Handler
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, ServiceResult<LoginResponseModel>>
    {
        private readonly IIdentityRepository _identityRepository;
        private readonly IEventBus _eventBus;

        public LoginUserHandler(IIdentityRepository identityRepository, IEventBus eventBus)
        {
            _identityRepository = identityRepository;
            _eventBus = eventBus;
        }

        public async Task<ServiceResult<LoginResponseModel>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var loginUser = await _identityRepository.LoginUserAsync(
               request.LoginUserModel.Email, request.LoginUserModel.Password, cancellationToken);

            if (!loginUser.IsSuccess || string.IsNullOrWhiteSpace(loginUser.AccessToken) || string.IsNullOrWhiteSpace(loginUser.RefreshToken))
                return ServiceResult<LoginResponseModel>.Fail(loginUser.ErrorMessage ?? "Đăng nhập thất bại.", 400);

            await _eventBus.PublishAsync(
                new UserLoginEvent(true));

            return ServiceResult<LoginResponseModel>.SuccessResult(
                new LoginResponseModel
                {
                    AccessToken = loginUser.AccessToken,
                    RefreshToken = loginUser.RefreshToken,
                    ExpiresInMinutes = loginUser.ExpiresInMinutes,
                    Roles = loginUser.Roles
                },
                "Đăng nhập thành công.",
                statusCode: 200);
        }
    }
}
