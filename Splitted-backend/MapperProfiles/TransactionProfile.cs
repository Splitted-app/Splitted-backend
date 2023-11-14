using AutoMapper;
using Models.CsvModels;
using Models.DTOs.Incoming.Transaction;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.Transaction;
using Models.Entities;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.MapperProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<TransactionCsv, Transaction>();

            CreateMap<TransactionPostDTO, Transaction>();

            CreateMap<Transaction, TransactionCreatedDTO>();

            CreateMap<Transaction, TransactionGetDTO>()
                .ForMember(dest => dest.AvatarImage, opt => opt.MapFrom(src => src.User.AvatarImage))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<Transaction, TransactionDuplicatedGetDTO>()
                .ForMember(dest => dest.AvatarImage, opt => opt.MapFrom(src => src.User.AvatarImage))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<TransactionPutDTO, Transaction>()
                .ForMember(dest => dest.Amount, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Amount))))
                .ForMember(dest => dest.Currency, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Currency))))
                .ForMember(dest => dest.Date, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Date))))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Description))))
                .ForMember(dest => dest.TransactionType, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.TransactionType))))
                .ForMember(dest => dest.UserCategory, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.UserCategory))));

            CreateMap<Transaction, TransactionAITrainDTO>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("yyyy-MM-dd")));
        }
    }
}
