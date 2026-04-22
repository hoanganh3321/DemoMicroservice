using Authen.Application.Interface;
using Authen.Application.Models.User;
using Authen.Domain.Enum;
using Authen.Infrastructure.Constant;
using Authen.Infrastructure.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authen.Infrastructure.Implement
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly IOptions<JwtConfig> _jwtConfig;
        private readonly IUserRepository _userRepository;
        public IdentityRepository(UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, IOptions<JwtConfig> jwtConfig, IUserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _jwtConfig = jwtConfig;
            _userRepository = userRepository;
        }

        public async Task<IdentityUserCreatedResult> CreateUserAsync(CreateUserModel createUserModel,
        CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<User>(createUserModel);
            var createResult = await _userManager.CreateAsync(user, createUserModel.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Create user failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.CUSTOMER);
            if (!roleResult.Succeeded)
            {

                await _userManager.DeleteAsync(user);

                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Assign role failed: {errors}");
            }

            return new IdentityUserCreatedResult(user.Id, user.Email ?? createUserModel.Email);
        }

        public async Task<IdentityUserLoginResult> LoginUserAsync(string email, string password, 
            CancellationToken cancellationToken = default)
        {
            var account = await FindAccountAsync(email);

            var validationError = ValidateAccount(account);
            if (validationError != null)
                return IdentityUserLoginResult.Failure(validationError);

            var passwordCheck = await _signInManager
                .CheckPasswordSignInAsync(account!, password, lockoutOnFailure: false);

            if (!passwordCheck.Succeeded)
                return IdentityUserLoginResult.Failure("Email hoặc mật khẩu không chính xác");

            var roles = await GetUserRoles(account!);
            if (roles.Count == 0)
                throw new InvalidOperationException($"User {account!.Id} không có role nào");

            account = await LoadAccountProfileAsync(account!, roles);

            var claims = CreateClaimForAccessToken(account, roles);
            var accessToken = GenerateToken(claims);
            var refreshToken = GenerateRefreshToken();

            account!.RefreshToken = HashRefreshToken(refreshToken);
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10);
            await _userManager.UpdateAsync(account);

            return IdentityUserLoginResult.Success(
                accessToken,
                refreshToken,
                _jwtConfig.Value.ExpireInMinutes,
                roles.ToList());
        }

        public async Task<IdentityUserLoginResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var hashedRefreshToken = HashRefreshToken(refreshToken);
            var account = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken, cancellationToken);
            if (account == null || account.RefreshTokenExpiryTime == null || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return IdentityUserLoginResult.Failure("Refresh token không hợp lệ hoặc đã hết hạn.");

            var roles = await GetUserRoles(account);
            if (roles.Count == 0)
                return IdentityUserLoginResult.Failure("Người dùng không có quyền truy cập.");

            account = await LoadAccountProfileAsync(account, roles);
            var claims = CreateClaimForAccessToken(account, roles);
            var newAccessToken = GenerateToken(claims);
            var newRefreshToken = GenerateRefreshToken();

            account.RefreshToken = HashRefreshToken(newRefreshToken);
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10);
            await _userManager.UpdateAsync(account);

            return IdentityUserLoginResult.Success(
                newAccessToken,
                newRefreshToken,
                _jwtConfig.Value.ExpireInMinutes,
                roles.ToList());
        }

        public async Task<bool> RevokeRefreshTokenAsync(string userId, CancellationToken cancellationToken = default)
        {
            var account = await _userManager.FindByIdAsync(userId);
            if (account == null)
                return false;

            account.RefreshToken = null;
            account.RefreshTokenExpiryTime = null;
            var updateResult = await _userManager.UpdateAsync(account);
            return updateResult.Succeeded;
        }

        private List<Claim> CreateClaimForAccessToken(User user, IList<string> roles)
        {
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException("UserName hoặc Email không hợp lệ để tạo access token.");

            var authClaims = new List<Claim>()
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email), // de dung cho .net
                //new(JwtRegisteredClaimNames.Email, user.Email), // de dung cho frontend
                new(JwtRegisteredClaimNames.Sub, user.Id), // dung sub phai cau hinh them gi do de dung cho frontend
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            };

            if (user.CustomerProfile != null)
            {
                authClaims.Add(new Claim("customer_id", user.CustomerProfile.Id.ToString()));
            }

            if (user.StaffProfile != null)
            {
                authClaims.Add(new Claim("staff_id", user.StaffProfile.Id.ToString()));
            }

            foreach (var role in roles)
            {
                authClaims.Add(new(ClaimTypes.Role, role));
            }

            return authClaims;
        }

        private string GenerateToken(IEnumerable<Claim> authClaims)
        {
            if (string.IsNullOrWhiteSpace(_jwtConfig.Value.SecretKey))
                throw new InvalidOperationException("JWT SecretKey chưa được cấu hình.");

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Value.Issuer,
                audience: _jwtConfig.Value.Audience,
                expires: DateTime.Now.AddMinutes(_jwtConfig.Value.ExpireInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);

            // rng la 1 object
            // rng.GetBytes(randomNumber);
            // tao ra 1 byte roi insert vao randomNumber cho day mang
            // 1 byte la 8 bit day nhi phan 0 1
            // Convert.ToBase64String(randomNumber)
            // sau do chuyen cac byte thanh chuoi base64
            // vd "z5FNRt5T8BgMErkYkY8k/hv63M0+0UXrJxN4VtQO5iPjoxYtC4JccIhC5g=="
        }

        private async Task<User> FindAccountAsync(string email)
        {
            User? user = null;
            if (email.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(email);
            }
            else
            {
                user = await _userManager.FindByNameAsync(email);
            }
            if (user == null)
                throw new InvalidOperationException("User not found.");
            return user;
        }

        private static string? ValidateAccount(User? account) => account switch
        {
            null => "Tài khoản không tồn tại",
            { EmailConfirmed: false } => "Tài khoản chưa được xác nhận",
            { UserStatus: UserStatus.Block } => "Tài khoản của bạn đã bị khóa",
            _ => null
        };

        private async Task<User> LoadAccountProfileAsync(User account, IList<string> roles)
        {
            // Các role cần load profile tương ứng
            var profileIncludes = new Dictionary<bool, Func<IQueryable<User>, IQueryable<User>>>
            {
                [roles.Contains(UserRoles.CUSTOMER)] =
                    q => q.Include(u => u.CustomerProfile),

                [roles.Any(r => r is UserRoles.STAFF )] =
                    q => q.Include(u => u.StaffProfile),
            };

            var includeFunc = profileIncludes.GetValueOrDefault(true);
            if (includeFunc == null) return account;

            return await includeFunc(_userManager.Users)
                       .FirstOrDefaultAsync(u => u.Id == account.Id) ?? account;
        }

        private async Task<IList<string>> GetUserRoles(Authen.Infrastructure.Identity.User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToHexString(bytes);
        }
    }
}
