using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsvConversion.Mappers
{
    internal class MbankMapper : BaseMapper
    {
        private MbankMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Name("Data operacji");
            Map(transaction => transaction.Description).Convert(args => MapDescription(args.Row));
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>("Opis operacji")!.ToLower()));
            Map(transaction => transaction.Category).Name("Kategoria");
        }


        private string MapCurrency(IReaderRow row)
        {
            string amountCurrency = row.GetField<string>("Kwota")!;
            return amountCurrency.Split()[1];
        }

        protected override decimal MapAmount(IReaderRow row)
        {
            string amountCurrency = row.GetField<string>("Kwota")!;
            string amount = amountCurrency.Split()[0];
            return decimal.Parse(amount.Replace(".", ","));
        }

        protected override string MapDescription(IReaderRow row)
        {
            string description = row.GetField<string>("Opis operacji")!;
            description = string.Join(" ", description.Split(' ')
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            string[] descriptionSplitted = description.Split();
            string[] descriptionSplittedModified = Array.ConvertAll(descriptionSplitted, s => s.ToLower());

            TransactionTypeEnum transactionType = MapTransactionType(description.ToLower());

            if (transactionType.Equals(TransactionTypeEnum.Card))
            {
                int elementsToSkip = descriptionSplitted.Count() - Array.IndexOf(descriptionSplittedModified, "zakup");
                descriptionSplitted = descriptionSplitted.SkipLast(elementsToSkip)
                    .ToArray();

                return string.Join(" ", descriptionSplitted);
            }
            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                int[] blikIndexes = Enumerable.Range(0, descriptionSplitted.Count())
                                    .Where(i => descriptionSplittedModified[i].Equals("blik"))
                                    .ToArray();
                int elementsToSkipLast = descriptionSplitted.Count() - blikIndexes[1];
                int elementsToSkip = blikIndexes[0] + 2;

                descriptionSplitted = descriptionSplitted.SkipLast(elementsToSkipLast)
                    .Skip(elementsToSkip)
                    .ToArray();

                return string.Join(" ", descriptionSplitted);
            }
            else return description;

        }
    }
}
