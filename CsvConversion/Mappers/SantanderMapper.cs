using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;

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
    }
}
