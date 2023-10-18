using AutoMapper;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Outgoing.Budget;
using Models.Entities;

namespace Splitted_backend.MapperProfiles
{
    public class BudgetProfile : Profile
    {
        public BudgetProfile()
        {
            CreateMap<BudgetPostDTO, Budget>();
            CreateMap<BudgetPutDTO, Budget>()
                .ForMember(dest => dest.Bank, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Bank))))
                .ForMember(dest => dest.BudgetType, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.BudgetType))))
                .ForMember(dest => dest.Currency, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Currency))))
                .ForMember(dest => dest.BudgetBalance, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.BudgetBalance))));
            CreateMap<Budget, BudgetCreatedDTO>();
            CreateMap<Budget, BudgetGetDTO>();
        }
    }

}
