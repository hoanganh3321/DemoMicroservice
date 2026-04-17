using Authen.Application.Interface;
using Authen.Application.Models.User;
using Authen.Infrastructure.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Authen.Infrastructure.Implement
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public IdentityRepository(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IdentityUserCreatedResult> CreateUserAsync(
            CreateUserModel createUserModel,
            CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<User>(createUserModel);

            var result = await _userManager.CreateAsync(user, createUserModel.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Create user failed: {errors}");
            }

            return new IdentityUserCreatedResult(user.Id, user.Email ?? createUserModel.Email);
        }
    }
}
