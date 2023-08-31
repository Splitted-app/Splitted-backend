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
            Map(transaction => transaction.Description).Name("Nadawca / Odbiorca");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>("Typ operacji")!.ToLower()));
            Map(transaction => transaction.Category).Name("Kategoria");
        }


        protected override decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>("Kwota operacji")!.Replace(".", ","));
    }
}
