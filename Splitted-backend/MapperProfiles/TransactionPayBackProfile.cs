using AutoMapper;
using Models.CsvModels;
using Models.DTOs.Outgoing.TransactionPayBack;
using Models.Entities;
using Splitted_backend.MapperProfiles.MappingActions;

namespace Splitted_backend.MapperProfiles
{
    public class TransactionPayBackProfile : Profile
    {
        public TransactionPayBackProfile()
        {
            CreateMap<TransactionPayBack, TransactionPayBackGetDTO>()
                .ForMember(dest => dest.AvatarImage, opt => opt.MapFrom(src => src.OwingUser == null ? null 
                    : src.OwingUser.AvatarImage))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.OwingUser == null ? null
                    : src.OwingUser.UserName))
                .AfterMap<GetPayBackTransaction>();
        }

    }
}
