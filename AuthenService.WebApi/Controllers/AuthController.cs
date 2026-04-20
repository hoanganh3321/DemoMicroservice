using Authen.Application.Command;
using Authen.Application.Common;
using Authen.Application.Handler;
using Authen.Application.Models.User;
using AuthenService.WebApi.Controllers.Base;
using AuthenService.WebApi.RequestDTO;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    }
}
