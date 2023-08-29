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
    internal class MbankMapper : ClassMap<TransactionCsv>
    {
        private string[] possibleTransferNames = new string[]
        {
            "przekazanie",
            "przelew",
        };


        private MbankMapper()
        {
            Map(transaction => transaction.Currency).Convert(args => MapCurrency(args.Row));
            Map(transaction => transaction.Date).Name("Data operacji");
            Map(transaction => transaction.Description).Name("Opis operacji");
            Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            Map(transaction => transaction.TransactionType).Convert(args => MapTransactionType(args.Row));
            Map(transaction => transaction.Category).Name("Kategoria");
        }


        private string MapCurrency(IReaderRow row)
        {
            string amountCurrency = row.GetField<string>("Kwota")!;
            return amountCurrency.Split()[1];
        }

        private decimal MapAmount(IReaderRow row)
        {
            string amountCurrency = row.GetField<string>("Kwota")!;
            string amount = amountCurrency.Split()[0];
            return decimal.Parse(amount.Replace(".", ","));
        }

        private TransactionTypeEnum MapTransactionType(IReaderRow row)
        {
            string title = row.GetField<string>("Opis operacji")!.ToLower();

            if (title.Contains("blik")) return TransactionTypeEnum.Blik;
            else if (title.Contains("karty")) return TransactionTypeEnum.Card;
            else if (possibleTransferNames.Any(ptn => title.Contains(ptn))) return TransactionTypeEnum.Transfer;
            else return TransactionTypeEnum.Other;
        }
    }
}
