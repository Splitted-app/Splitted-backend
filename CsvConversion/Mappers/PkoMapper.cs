using CsvConversion.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private string MapPaymentDescription(IReaderRow row, int transactionDescriptionIndex)
        {
            string[] paymentDescriptionSplitted = row.GetField<string>(transactionDescriptionIndex + 1)!.Split(":");

            int addressIndex = Enumerable.Range(0, paymentDescriptionSplitted.Count())
                    .FirstOrDefault(i => paymentDescriptionSplitted[i].Contains("Adres"), -1);
            int cityIndex = Enumerable.Range(0, paymentDescriptionSplitted.Count())
                .FirstOrDefault(i => paymentDescriptionSplitted[i].Contains("Miasto"), -1);
            int countryIndex = Enumerable.Range(0, paymentDescriptionSplitted.Count())
                .FirstOrDefault(i => paymentDescriptionSplitted[i].Contains("Kraj"), -1);

                int[] finalDescriptionIndexes = new int[] { addressIndex + 1, cityIndex + 1, countryIndex + 1 };
            IEnumerable<string> finalDescriptionSplitted = finalDescriptionIndexes
                .Select(i => i != 0 ? paymentDescriptionSplitted[i] : "");

            return string.Join(string.Empty, finalDescriptionSplitted)
                .Beutify();
        }

        private string MapTransferDescription(IReaderRow row, int transactionDescriptionIndex)
        {
            string[] transferNameDescriptionSplitted = row.GetField<string>(transactionDescriptionIndex + 1)!.Split(":");
            string[] transferAddressDescriptionSplitted = row.GetField<string>(transactionDescriptionIndex + 2)!.Split(":");
            string? transferTitleDescription = row.GetField(transactionDescriptionIndex + 3);

            List<string> finalDescriptionSplitted = new List<string>();

            int transferNameIndex = Enumerable.Range(0, transferNameDescriptionSplitted.Count())
                    .FirstOrDefault(i => transferNameDescriptionSplitted[i].Contains("Nazwa odbiorcy") || 
                    transferNameDescriptionSplitted[i].Contains("Nazwa nadawcy"), -1);
            int transferAddressIndex = Enumerable.Range(0, transferAddressDescriptionSplitted.Count())
                    .FirstOrDefault(i => transferAddressDescriptionSplitted[i].Contains("Adres odbiorcy") ||
                    transferAddressDescriptionSplitted[i].Contains("Adres nadawcy") || 
                    transferAddressDescriptionSplitted[i].Contains("Tytuł"), -1);

            finalDescriptionSplitted.Add(transferNameDescriptionSplitted[transferNameIndex + 1]);
            finalDescriptionSplitted.AddRange(transferAddressDescriptionSplitted.Take(new Range(transferAddressIndex + 1, 
                transferAddressDescriptionSplitted.Count())));

            if (transferTitleDescription is not null)
            {
                string[] transferTitleDescriptionSplitted = transferTitleDescription.Split(":");
                int transferTitleIndex = Enumerable.Range(0, transferTitleDescriptionSplitted.Count())
                    .FirstOrDefault(i => transferTitleDescriptionSplitted[i].Contains("Tytuł"), -1);

                finalDescriptionSplitted.Add("\n" + transferTitleDescriptionSplitted[transferTitleIndex + 1]);
            }

            return string.Join(string.Empty, finalDescriptionSplitted)
                .Beutify();
        }

        private string MapOtherDescription(IReaderRow row, int transferDescriptionIndex)
        {
            string[] transferDescriptionSplitted = row.GetField<string>(transferDescriptionIndex)!.Split(":");

            int titleIndex = Enumerable.Range(0, transferDescriptionSplitted.Count())
                    .FirstOrDefault(i => transferDescriptionSplitted[i].Contains("Tytuł"), -1);

            return transferDescriptionSplitted[titleIndex + 1]
                .Beutify();
        }

        protected override string MapDescription(IReaderRow row)
        {
            string operationType = row.GetField<string>("Typ transakcji")!;
            TransactionTypeEnum transactionType = MapTransactionType(operationType.ToLower());

            int transactionDescriptionIndex = Array.IndexOf(row.HeaderRecord!, "Opis transakcji");

            if (transactionType.Equals(TransactionTypeEnum.Card))
                return MapPaymentDescription(row, transactionDescriptionIndex);

            else if (transactionType.Equals(TransactionTypeEnum.Transfer))
                return MapTransferDescription(row, transactionDescriptionIndex);

            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                if (operationType.ToLower().Contains("mobile")) return MapTransferDescription(row, transactionDescriptionIndex);
                else return MapPaymentDescription(row, transactionDescriptionIndex + 1);
            }

            else
                return MapOtherDescription(row, transactionDescriptionIndex);
        }
    }
}
