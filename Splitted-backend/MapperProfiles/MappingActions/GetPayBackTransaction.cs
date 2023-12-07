using AutoMapper;
using Models.DTOs.Outgoing.Transaction;
using Models.DTOs.Outgoing.TransactionPayBack;
using Models.Entities;
using Splitted_backend.Interfaces;

namespace Splitted_backend.MapperProfiles.MappingActions
{
    public class GetPayBackTransaction : IMappingAction<TransactionPayBack, TransactionPayBackGetDTO>
    {
        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }


        public GetPayBackTransaction(IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
        }

        public void Process(TransactionPayBack source, TransactionPayBackGetDTO destination, ResolutionContext context)
        {
            if (source.PayBackTransactionId is not null)
            {
                Transaction? transaction = repositoryWrapper.Transactions
                    .GetEntityOrDefaultByCondition(t => t.Id.Equals(source.PayBackTransactionId))!;

                destination.PayBackTransaction = mapper.Map<TransactionDuplicatedGetDTO>(transaction);
            }
        }
    }
}
