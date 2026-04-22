using Authen.Application.Models.User;
using AuthenService.WebApi.RequestDTO;
using AutoMapper;

namespace AuthenService.WebApi.Mapping
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<RequestCreateUserDTO, CreateUserModel>();
            CreateMap<RequestLoginDTO, LoginUserModel>();
        }
    }
}
