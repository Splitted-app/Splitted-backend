using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion.Mappers
{
    internal class PkoMapper : BaseMapper
    {
        private PkoMapper()
        {
            Map(transaction => transaction.Currency).Name("Waluta");
            Map(transaction => transaction.Date).Name("Data waluty");
            Map(transaction => transaction.Description).Convert(args => MapDescription(args.Row));
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>("Typ transakcji")!.ToLower()));
        }

        protected override decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>("Kwota")!.Replace(".", ","));

        protected override string MapDescription(IReaderRow row)
        {
            string description = row.GetField<string>("Opis transakcji")!;
            string operationType = row.GetField<string>("Typ transakcji")!;
            TransactionTypeEnum transactionType = MapTransactionType(operationType.ToLower());

            if (transactionType.Equals(TransactionTypeEnum.Card)) return description;
            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                if (description.ToLower().Contains("blik")) return description;
                else
                {
                    //stringBuilder.Append("\n");
                    //string[] splittedTitle = title.Split();
                    //int elementsToSkip = (splittedTitle.Count() > 3 && splittedTitle[3].Contains("+")) ? 4 : 3;
                    //splittedTitle = splittedTitle.Skip(elementsToSkip).ToArray();
                    //return splittedTitle.Aggregate(stringBuilder, (prev, current) => prev.Append(current)).ToString();
                    return description;

                }
            }
            else return description;

        }
    }
}
