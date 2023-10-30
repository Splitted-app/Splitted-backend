using CsvConversion.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using Models.Enums;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvConversion.Mappers
{
    internal class SantanderMapper : BaseMapper
    {
        internal static string currency = "";


        public SantanderMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Index(1);
            Map(transaction => transaction.Description).Convert(args => MapDescription(args.Row));
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>(2)!.ToLower()));
        }


        private string MapCurrency(IReaderRow row) => currency;

        protected override decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>(5)!.Replace(".", ","));

        protected override string MapDescription(IReaderRow row)
        {
            string description = row.GetField<string>(2)!;
            string address = row.GetField<string>(3)!;  
            string[] descriptionSplitted = description.Split();
            string[] descriptionSplittedModified = Array.ConvertAll(descriptionSplitted, s => s.ToLower());

            StringBuilder stringBuilder = new StringBuilder(description);
            TransactionTypeEnum transactionType = MapTransactionType(description.ToLower());

            if (transactionType.Equals(TransactionTypeEnum.Card))
            {
                int currencyIndex = Array.IndexOf(descriptionSplittedModified, currency.ToLower());
                int elementsToSkip = currencyIndex + 1;

                descriptionSplitted = descriptionSplitted.Skip(elementsToSkip)
                    .ToArray();

                return string.Join(" ", descriptionSplitted)
                    .Beutify();
            }

            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                if (description.ToLower().Contains("blik"))
                {
                    int blikIndex = Array.IndexOf(descriptionSplittedModified, "blik");
                    int refIndex = Enumerable.Range(0, descriptionSplitted.Count())
                        .FirstOrDefault(i => Regex.IsMatch(descriptionSplittedModified[i], @"ref:\d+"));

                    int elementsToSkip = blikIndex + 1;
                    int elementsToSkipLast = descriptionSplitted.Count() - refIndex;

                    descriptionSplitted = descriptionSplitted.Skip(elementsToSkip)
                        .SkipLast(elementsToSkipLast)
                        .ToArray();

                    return string.Join(" ", descriptionSplitted) + $"\n {address}"
                        .Beutify();
                }

                else return stringBuilder.Append("\n")
                        .Append(address)
                        .ToString()
                        .Beutify();
            }

            else return stringBuilder.Append("\n")
                    .Append(address)
                    .ToString()
                    .Beutify();
        }
    }
}
