using Authen.Application.Models.User;
using Authen.Domain.Entities;

namespace Authen.Application.Interface
{
    public interface IIdentityRepository
    {
        Task<IdentityUserCreatedResult> CreateUserAsync(
            CreateUserModel createUserModel,
            CancellationToken cancellationToken = default);

        Task<IdentityUserLoginResult> LoginUserAsync(
            string email, string password,
            CancellationToken cancellationToken = default);

        Task<IdentityUserLoginResult> RefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);

        Task<bool> RevokeRefreshTokenAsync(
            string userId,
            CancellationToken cancellationToken = default);
    }
    public record IdentityUserCreatedResult(string UserId, string Email);
    public record IdentityUserLoginResult
    {
        // Success
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }
        public int ExpiresInMinutes { get; init; }
        public IReadOnlyCollection<string> Roles { get; init; } = [];

        // Failure
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }

        public static IdentityUserLoginResult Success(
            string accessToken,
            string refreshToken,
            int expiresInMinutes,
            IReadOnlyCollection<string> roles) =>
            new()
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresInMinutes = expiresInMinutes,
                Roles = roles
            };

        public static IdentityUserLoginResult Failure(string message) =>
            new() { IsSuccess = false, ErrorMessage = message };
    }
}
