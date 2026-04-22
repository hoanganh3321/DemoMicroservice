namespace Authen.Application.Models.User
{
    public class LoginResponseModel
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required int ExpiresInMinutes { get; set; }
        public IReadOnlyCollection<string> Roles { get; set; } = [];
    }
}
