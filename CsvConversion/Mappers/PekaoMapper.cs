using CsvConversion.Extensions;
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
    internal class PekaoMapper : BaseMapper
    {
        public PekaoMapper()
        {
            Map(transaction => transaction.Currency).Name("Waluta");
            Map(transaction => transaction.Date).Name("Data waluty");
            Map(transaction => transaction.Description).Convert(args => MapDescription(args.Row));
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>("Typ operacji")!.ToLower()));
            Map(transaction => transaction.Category).Name("Kategoria");
        }


        protected override decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>("Kwota operacji")!.Replace(".", ","));

        protected override string MapDescription(IReaderRow row)
        {
            string operationType = row.GetField<string>("Typ operacji")!;
            string title = row.GetField<string>("Tytułem")!;
            string recipient = row.GetField<string>("Nadawca / Odbiorca")!;
            string recipientAddress = row.GetField<string>("Adres nadawcy / odbiorcy")!;

            StringBuilder stringBuilder = new StringBuilder(recipient);
            TransactionTypeEnum transactionType = MapTransactionType(operationType.ToLower());

            stringBuilder.Append(" ").Append(recipientAddress);

            if (transactionType.Equals(TransactionTypeEnum.Card)) return stringBuilder.ToString().Beutify();

            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                if (title.ToLower().Contains("przelew"))
                {
                    stringBuilder.Append("\n");

                    string[] titleSplitted = title.Split();
                    string[] titleSplittedModified = Array.ConvertAll(titleSplitted, s => s.ToLower());

                    int[] transferIndexes = Enumerable.Range(0, titleSplitted.Count())
                                    .Where(i => titleSplittedModified[i].Equals("przelew"))
                                    .ToArray();

                    int elementsToSkip = (transferIndexes.Count() > 1) ? titleSplitted.Count() - transferIndexes[1] : 0;
                    titleSplitted = titleSplitted.SkipLast(elementsToSkip)
                        .ToArray();

                    return stringBuilder.Append(string.Join(" ", titleSplitted))
                        .ToString()
                        .Beutify();

                }
                else return stringBuilder.ToString()
                        .Beutify();
            }

            else return stringBuilder.ToString()
                    .Beutify();

        }
    }
}
