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
    public class PekaoMapper : ClassMap<TransactionCsv>
    {
        private string[] possibleTransferNames = new string[]
        {
            "przekazanie",
            "przelew",
        };


        public PekaoMapper()
        {
            Map(transaction => transaction.Currency).Name("Waluta");
            Map(transaction => transaction.Date).Name("Data waluty");
            Map(transaction => transaction.Description).Name("Nadawca / Odbiorca");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row));
            Map(transaction => transaction.Category).Name("Kategoria");
        }


        private decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>("Kwota operacji")!.Replace(".", ","));

        private TransactionTypeEnum MapTransactionType(IReaderRow row)
        {
            string title = row.GetField<string>("Typ operacji")!.ToLower();

            if (title.Contains("płatność blik")) return TransactionTypeEnum.Blik;
            else if (title.Contains("kartą")) return TransactionTypeEnum.Card;
            else if (possibleTransferNames.Any(ptn => title.Contains(ptn))) return TransactionTypeEnum.Transfer;
            else return TransactionTypeEnum.Other;
        }


    }
}
