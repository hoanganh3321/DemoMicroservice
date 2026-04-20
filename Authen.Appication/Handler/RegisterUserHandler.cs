using Authen.Application.Command;
using Authen.Application.Common;
using Authen.Application.EventBus;
using Authen.Application.Interface;
using Authen.Application.PublicEvents;
using MediatR;

namespace Authen.Application.Handler
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ServiceResult<string>>
    {
        private readonly IIdentityRepository _identityRepository;
        private readonly IEventBus _eventBus;

        public RegisterUserHandler(IIdentityRepository identityRepository, IEventBus eventBus)
        {
            _identityRepository = identityRepository;
            _eventBus = eventBus;
        }

        public async Task<ServiceResult<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var createdUser = await _identityRepository.CreateUserAsync(
                request.CreateUserModel, cancellationToken);

            if (createdUser is null)
                return ServiceResult<string>.Fail("Đăng ký thất bại.", 400);

            await _eventBus.PublishAsync(
                new UserCreatedEvent(createdUser.UserId, createdUser.Email));

            return ServiceResult<string>.SuccessResult(
                createdUser.UserId,
                "Đăng ký tài khoản thành công.",
                statusCode: 201);
        }
    }
}
