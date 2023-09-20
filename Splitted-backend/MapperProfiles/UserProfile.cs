using AutoMapper;
using Models.DTOs.Incoming;
using Models.DTOs.Outgoing;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.MapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegisterDTO, User>();
            CreateMap<User, UserCreatedDTO>();
        }
    }
}
