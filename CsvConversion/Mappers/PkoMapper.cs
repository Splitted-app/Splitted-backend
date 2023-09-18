using CsvConversion.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private string MapPaymentDescription(string[] descriptionSplitted)
        {
            int addressIndex = Enumerable.Range(0, descriptionSplitted.Count())
                    .FirstOrDefault(i => descriptionSplitted[i].Contains("Adres"), -1);
            int cityIndex = Enumerable.Range(0, descriptionSplitted.Count())
                .FirstOrDefault(i => descriptionSplitted[i].Contains("Miasto"), -1);
            int countryIndex = Enumerable.Range(0, descriptionSplitted.Count())
                .FirstOrDefault(i => descriptionSplitted[i].Contains("Kraj"), -1);

            int[] finalDescriptionIndexes = new int[] { addressIndex, cityIndex, countryIndex };
            IEnumerable<string> finalDescriptionSplitted = finalDescriptionIndexes
                .Select(i => Regex.Replace(descriptionSplitted[i], @".*:", string.Empty));

            return string.Join(string.Empty, finalDescriptionSplitted)
                .Beutify();
        }

        private string MapTransferDescription(string[] descriptionSplitted)
        {
            List<string> finalDescriptionSplitted = new List<string>();

            int recipientNameIndex = Enumerable.Range(0, descriptionSplitted.Count())
                    .FirstOrDefault(i => descriptionSplitted[i].Contains("Nazwa odbiorcy"));
            int recipientAddressIndex = Enumerable.Range(0, descriptionSplitted.Count())
                    .FirstOrDefault(i => descriptionSplitted[i].Contains("Adres odbiorcy"));
            int titleIndex = Enumerable.Range(0, descriptionSplitted.Count())
                    .FirstOrDefault(i => descriptionSplitted[i].Contains("Tytuł"));

            finalDescriptionSplitted.Add(descriptionSplitted[recipientNameIndex + 1]);
            finalDescriptionSplitted.Add(descriptionSplitted[recipientAddressIndex + 1] + "\n");
            finalDescriptionSplitted.AddRange(descriptionSplitted.Take(new Range(titleIndex + 1, descriptionSplitted.Count())));

            return string.Join(" ", finalDescriptionSplitted)
                .Beutify();
        }

        protected override string MapDescription(IReaderRow row)
        {
            string description = row.GetField<string>("Opis transakcji")!;
            string[] descriptionSplitted = description.Split("|");

            string operationType = row.GetField<string>("Typ transakcji")!;
            TransactionTypeEnum transactionType = MapTransactionType(operationType.ToLower());

            if (transactionType.Equals(TransactionTypeEnum.Card))
                return MapPaymentDescription(descriptionSplitted);

            else if (transactionType.Equals(TransactionTypeEnum.Transfer))
                return MapTransferDescription(descriptionSplitted);

            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                if (operationType.ToLower().Contains("przelew")) return MapTransferDescription(descriptionSplitted);
                else return MapPaymentDescription(descriptionSplitted);
            }

            else
            {
                try
                {
                    return MapPaymentDescription(descriptionSplitted);
                }
                catch
                {
                    return Regex.Replace(description.Replace("|", string.Empty), @".*:", string.Empty)
                        .Beutify();   
                }
            }
        }
    }
}
