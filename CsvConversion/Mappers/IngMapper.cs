using CsvHelper.Configuration;
using CsvHelper;
using Models.CsvModels;
using System.Text;
using System.Runtime.Versioning;
using System.Data;

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
            Map(transaction => transaction.Description).Convert(args => MapDescription(args.Row));
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

        protected override string MapDescription(IReaderRow row)
        {
            string title = row.GetField<string>("Tytuł")!;
            string contractorData = row.GetField<string>("Dane kontrahenta")!;

            StringBuilder stringBuilder = new StringBuilder(contractorData);
            TransactionTypeEnum transactionType = MapTransactionType(title.ToLower());

            if (transactionType.Equals(TransactionTypeEnum.Card)) return stringBuilder.ToString();
            else if (transactionType.Equals(TransactionTypeEnum.Blik))
            {
                if (title.ToLower().Contains("blik")) return stringBuilder.ToString();
                else
                {
                    stringBuilder.Append("\n");
                    string[] splittedTitle = title.Split();

                    int elementsToSkip = (splittedTitle.Count() > 4 && splittedTitle[4].Contains("+")) ? 5 : 0;
                    splittedTitle = splittedTitle.Skip(elementsToSkip).ToArray();

                    return splittedTitle.Aggregate(stringBuilder, (prev, current) => prev.Append(" ").Append(current)).ToString();

                }
            }
            else return stringBuilder.Append("\n")
                    .Append(title)
                    .ToString();
           
        }
    }
}
