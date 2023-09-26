using AutoMapper;
using Models.CsvModels;
using Models.Entities;

namespace Splitted_backend.MapperProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<TransactionCsv, Transaction>();
        }
    }
}
