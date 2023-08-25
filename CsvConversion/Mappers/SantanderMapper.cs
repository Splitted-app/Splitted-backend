using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;

namespace CsvConversion.Mappers
{
    public class SantanderMapper : ClassMap<TransactionCsv>
    {
        private string[] possibleTransferNames = new string[]
        {
            "przekazanie",
            "przelew",
        };

        internal static string currency = "";


        public SantanderMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Index(0);
            Map(transaction => transaction.Description).Index(2);
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row));
        }


        private string MapCurrency(IReaderRow row) => currency;

        private decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>(5)!);

        private TransactionTypeEnum MapTransactionType(IReaderRow row)
        {
            string title = row.GetField<string>(2)!.ToLower();

            if (title.Contains("blik")) return TransactionTypeEnum.Blik;
            else if (title.Contains("kartą")) return TransactionTypeEnum.Card;
            else if (possibleTransferNames.Any(ptn => title.Contains(ptn))) return TransactionTypeEnum.Transfer;
            else return TransactionTypeEnum.Other;
        }
    }
}
