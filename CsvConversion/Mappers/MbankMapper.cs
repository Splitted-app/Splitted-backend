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
    internal class MbankMapper : BaseMapper
    {
        private MbankMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Name("Data operacji");
            Map(transaction => transaction.Description).Name("Opis operacji");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row.GetField<string>("Opis operacji")!.ToLower()));
            Map(transaction => transaction.Category).Name("Kategoria");
        }


        private string MapCurrency(IReaderRow row)
        {
            string amountCurrency = row.GetField<string>("Kwota")!;
            return amountCurrency.Split()[1];
        }

        protected override decimal MapAmount(IReaderRow row)
        {
            string amountCurrency = row.GetField<string>("Kwota")!;
            string amount = amountCurrency.Split()[0];
            return decimal.Parse(amount.Replace(".", ","));
        }
    }
}
