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
    internal class PkoMapper : ClassMap<TransactionCsv>
    {
        private string[] possibleTransferNames = new string[]
        {
            "przekazanie",
            "przelew",
            "zlecenie",
        };


        private PkoMapper()
        {
            Map(transaction => transaction.Currency).Name("Waluta");
            Map(transaction => transaction.Date).Name("Data waluty");
            Map(transaction => transaction.Description).Name("Opis transakcji");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row));
        }

        private decimal MapAmount(IReaderRow row) => decimal.Parse(row.GetField<string>("Kwota")!.Replace(".", ","));

        private TransactionTypeEnum MapTransactionType(IReaderRow row)
        {
            string title = row.GetField<string>("Typ transakcji")!.ToLower();

            if (title.Contains("blik")) return TransactionTypeEnum.Blik;
            else if (title.Contains("kartą")) return TransactionTypeEnum.Card;
            else if (possibleTransferNames.Any(ptn => title.Contains(ptn))) return TransactionTypeEnum.Transfer;
            else return TransactionTypeEnum.Other;
        }
    }
}
