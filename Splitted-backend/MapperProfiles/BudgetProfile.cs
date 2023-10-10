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
            CreateMap<Budget, BudgetCreatedDTO>();
        }
    }

}
