using AutoMapper;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Outgoing.Budget;
using Models.Entities;
using Models.Enums;

namespace Splitted_backend.MapperProfiles
{
    public class BudgetProfile : Profile
    {
        public BudgetProfile()
        {
            CreateMap<BudgetPostDTO, Budget>()
                .ForMember(dest => dest.BudgetType, opt => opt.MapFrom(src => BudgetTypeEnum.Personal))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.Today));

            CreateMap<BudgetPutDTO, Budget>()
                .ForMember(dest => dest.Bank, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Bank))))
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Name))))
                .ForMember(dest => dest.Currency, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Currency))))
                .ForMember(dest => dest.BudgetBalance, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.BudgetBalance))));
            CreateMap<Budget, BudgetCreatedDTO>();
            CreateMap<Budget, UserBudgetGetDTO>();
            CreateMap<Budget, BudgetGetDTO>();
        }
    }

}
