using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;
using System.Text;

namespace CsvConversion.Mappers
{
    internal class SantanderMapper : BaseMapper
    {
        internal static string currency = "";


        public SantanderMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Index(1);
            Map(transaction => transaction.Description).Index(2);
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>(2)!.ToLower()));
        }


        private string MapCurrency(IReaderRow row) => currency;

        protected override decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>(5)!.Replace(".", ","));

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
                    int elementsToSkip = (splittedTitle.Count() > 3 && splittedTitle[3].Contains("+")) ? 4 : 3;
                    splittedTitle = splittedTitle.Skip(elementsToSkip).ToArray();
                    return splittedTitle.Aggregate(stringBuilder, (prev, current) => prev.Append(current)).ToString();

                }
            }
            else return stringBuilder.Append("\n").Append(title).ToString();

        }
    }
}
