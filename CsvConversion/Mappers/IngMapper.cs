using CsvHelper.Configuration;
using CsvHelper;
using Models.CsvModels;

namespace CsvConversion.Mappers
{
    public class IngMapper : ClassMap<TransactionCsv>
    {
        private string[] possibleAmountNames = new string[]
        {
            "Kwota transakcji (waluta rachunku)",
            "Kwota blokady/zwolnienie blokady",
            "Kwota płatności w walucie"
        };

        private string[] possibleTransferNames = new string[]
        {
            "przekazanie",
            "przelew",
        };


        private IngMapper()
        {
            Map(transaction => transaction.Currency).Name("Waluta");
            Map(transaction => transaction.Date).Name("Data transakcji");
            Map(transaction => transaction.Description).Name("Dane kontrahenta");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row));
        }


        private decimal MapAmount(IReaderRow row)
        {
            decimal amount;
            bool ifConverted;

            foreach (var possibleAmountName in possibleAmountNames)
            {
                ifConverted = Decimal.TryParse((row.GetField<string>(possibleAmountName)), out amount);
                if (ifConverted) return amount;
            }

            return 0;
        }

        private TransactionTypeEnum MapTransactionType(IReaderRow row)
        {
            string title = row.GetField<string>("Tytuł")!.ToLower();

            if (title.Contains("blik")) return TransactionTypeEnum.Blik;
            else if (title.Contains("kartą") || title.Contains("karty")) return TransactionTypeEnum.Card;
            else if (possibleTransferNames.Any(ptn => title.Contains(ptn))) return TransactionTypeEnum.Transfer;
            else return TransactionTypeEnum.Other;
        }
    }
}
