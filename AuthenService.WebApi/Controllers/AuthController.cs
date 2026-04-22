using Authen.Application.Command;
using Authen.Application.Common;
using Authen.Application.Models.User;
using AuthenService.WebApi.Controllers.Base;
using AuthenService.WebApi.RequestDTO;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenService.WebApi.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AuthController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng mới
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(
            [FromBody] RequestCreateUserDTO request,
            CancellationToken cancellationToken)
        {
            if (request.Password != request.ConfirmPassword)
                return HandleServiceResult(
                    ServiceResult<string>.Fail("Mật khẩu xác nhận không khớp.", 400));

            var createUserModel = _mapper.Map<CreateUserModel>(request);
            var command = new RegisterUserCommand(createUserModel);

            // MediaR
            var result = await _mediator.Send(command, cancellationToken);

            return HandleServiceResult(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] RequestLoginDTO request,
            CancellationToken cancellationToken)
        {
            var loginModel = _mapper.Map<LoginUserModel>(request);
            var command = new LoginUserCommand(loginModel);
            var result = await _mediator.Send(command, cancellationToken);
            return HandleServiceResult(result);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(
            [FromBody] RequestRefreshTokenDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
            return HandleServiceResult(result);
        }

        [Authorize]
        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return HandleServiceResult(ServiceResult<string>.Fail("Token không hợp lệ.", 401));

            var result = await _mediator.Send(new RevokeTokenCommand(userId), cancellationToken);
            return HandleServiceResult(result);
        }
    }
}
