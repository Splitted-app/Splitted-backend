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
            CreateMap<UserRegisterDTO, User>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))));

            CreateMap<User, UserCreatedDTO>();

            CreateMap<UserPutDTO, User>()
                .ForMember(dest => dest.AvatarImage, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.AvatarImage))))
                .ForMember(dest => dest.UserName, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.UserName)) && src.UserName is not null));

            CreateMap<User, UserGetDTO>();
        }
    }
}
