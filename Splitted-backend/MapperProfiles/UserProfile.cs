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
            CreateMap<UserPutDTO, User>()
                .ForMember(dest => dest.BankBalance, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.BankBalance))))
                .ForMember(dest => dest.Bank, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Bank))))
                .ForMember(dest => dest.AvatarImage, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.AvatarImage))));
        }
    }
}
