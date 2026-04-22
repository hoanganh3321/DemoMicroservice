namespace Authen.Application.Interface
{
    public interface ITokenRepository
    {
        string GenerateAccessToken(
            string userId,
            string userName,
            string email,
            IList<string> roles,
            int? customerProfileId = null,
            int? staffProfileId = null);
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
        int GetAccessTokenExpiryMinutes();
    }
}
