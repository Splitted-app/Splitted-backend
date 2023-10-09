using AutoMapper;
using Models.CsvModels;
using Models.DTOs.Incoming.Transaction;
using Models.DTOs.Outgoing.Transaction;
using Models.Entities;

namespace Splitted_backend.MapperProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<TransactionCsv, Transaction>();
            CreateMap<TransactionPostDTO, Transaction>();
            CreateMap<Transaction, TransactionCreatedDTO>();
        }
    }
}
