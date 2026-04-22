using Authen.Application.Interface;
using Authen.Application.Models.User;
using Authen.Domain.Enum;
using Authen.Infrastructure.Constant;
using Authen.Infrastructure.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Authen.Infrastructure.Implement
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenRepository _tokenService;
        public IdentityRepository(UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, ITokenRepository tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
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

            var accessToken = _tokenService.GenerateAccessToken(
                account.Id,
                account.UserName ?? string.Empty,
                account.Email ?? string.Empty,
                roles,
                account.CustomerProfile?.Id,
                account.StaffProfile?.Id);
            var refreshToken = _tokenService.GenerateRefreshToken();

            account!.RefreshToken = _tokenService.HashRefreshToken(refreshToken);
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10);
            await _userManager.UpdateAsync(account);

            return IdentityUserLoginResult.Success(
                accessToken,
                refreshToken,
                _tokenService.GetAccessTokenExpiryMinutes(),
                roles.ToList());
        }

        public async Task<IdentityUserLoginResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken);
            var account = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken, cancellationToken);
            if (account == null || account.RefreshTokenExpiryTime == null || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return IdentityUserLoginResult.Failure("Refresh token không hợp lệ hoặc đã hết hạn.");

            var roles = await GetUserRoles(account);
            if (roles.Count == 0)
                return IdentityUserLoginResult.Failure("Người dùng không có quyền truy cập.");

            account = await LoadAccountProfileAsync(account, roles);
            var newAccessToken = _tokenService.GenerateAccessToken(
                account.Id,
                account.UserName ?? string.Empty,
                account.Email ?? string.Empty,
                roles,
                account.CustomerProfile?.Id,
                account.StaffProfile?.Id);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            account.RefreshToken = _tokenService.HashRefreshToken(newRefreshToken);
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10);
            await _userManager.UpdateAsync(account);

            return IdentityUserLoginResult.Success(
                newAccessToken,
                newRefreshToken,
                _tokenService.GetAccessTokenExpiryMinutes(),
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

    }
}
