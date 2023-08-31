using CsvHelper.Configuration;
using CsvHelper;
using Models.CsvModels;

namespace CsvConversion.Mappers
{
    internal class IngMapper : BaseMapper
    {
        private string[] possibleAmountNames = new string[]
        {
            "Kwota transakcji (waluta rachunku)",
            "Kwota blokady/zwolnienie blokady",
            "Kwota płatności w walucie"
        };


        private IngMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Name("Data transakcji");
            Map(transaction => transaction.Description).Name("Dane kontrahenta");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>("Tytuł")!.ToLower()));
        }


        private string MapCurrency(IReaderRow row)
        {
            for (int i = 0; i < 3; i++)
            {
                string currency = row.GetField<string>("Waluta", i)!;
                if (!string.IsNullOrEmpty(currency)) return currency;
            }
            return "";
        }

        protected override decimal MapAmount(IReaderRow row)
        {
            decimal amount;
            bool ifConverted;

            foreach (var possibleAmountName in possibleAmountNames)
            {
                ifConverted = decimal.TryParse(row.GetField<string>(possibleAmountName)!.Replace(".", ","), out amount);
                if (ifConverted) return amount;
            }

            return 0;
        }
    }
}
