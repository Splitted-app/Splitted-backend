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
                .AfterMap<GetPayBackTransaction>();
        }

    }
}
