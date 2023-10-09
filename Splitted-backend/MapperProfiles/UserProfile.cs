using AutoMapper;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.User;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.MapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegisterDTO, User>();

            CreateMap<User, UserCreatedDTO>();

            CreateMap<UserPutDTO, User>()
                .ForMember(dest => dest.AvatarImage, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.AvatarImage))));

            CreateMap<User, UserGetDTO>();
        }
    }
}
