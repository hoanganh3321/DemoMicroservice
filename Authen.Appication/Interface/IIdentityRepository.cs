using Authen.Application.Models.User;

namespace Authen.Application.Interface
{
    public interface IIdentityRepository
    {
        Task<IdentityUserCreatedResult> CreateUserAsync(
            CreateUserModel createUserModel,
            CancellationToken cancellationToken = default);
    }

    public record IdentityUserCreatedResult(string UserId, string Email);
}
