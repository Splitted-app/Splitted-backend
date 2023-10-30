using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CsvConversion.Mappers
{
    internal abstract class BaseMapper : ClassMap<TransactionCsv>
    {
        private string[] possibleTransferNames = new string[]
        {
             "przekazanie",
             "przelew",
             "zlecenie",
        };


        protected TransactionTypeEnum MapTransactionType(string field)
        {
            if (field.Contains("blik") || (field.Contains("przelew na") && field.Contains("telefon")) || field.Contains("mobil")) return TransactionTypeEnum.Blik;
            else if (field.Contains("kartą") || field.Contains("karty")) return TransactionTypeEnum.Card;
            else if (possibleTransferNames.Any(ptn => field.Contains(ptn))) return TransactionTypeEnum.Transfer;
            else return TransactionTypeEnum.Other;
        }

        protected abstract decimal MapAmount(IReaderRow row);

        protected abstract string MapDescription(IReaderRow row);
    }
}
