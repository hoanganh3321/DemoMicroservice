using Authen.Application.Command;
using Authen.Application.EventBus;
using Authen.Application.Interface;
using Authen.Application.PublicEvents;

namespace Authen.Application.Handler
{
    public class RegisterUserHandler
    {
        private readonly IIdentityRepository _identityService;
        private readonly IEventBus _eventBus;

        public RegisterUserHandler(IIdentityRepository identityService, IEventBus eventBus)
        {
            _identityService = identityService;
            _eventBus = eventBus;
        }

        public async Task Handle(RegisterUserCommand command)
        {
            var createdUser = await _identityService.CreateUserAsync(
                command.CreateUserModel);

            await _eventBus.PublishAsync(
                new UserCreatedEvent(createdUser.UserId, createdUser.Email)
            );
        }
    }
}
