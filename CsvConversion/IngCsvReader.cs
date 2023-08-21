using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.CsvModels;

namespace CsvConversion
{
    public class IngCsvReader : BankCsvReader
    {
        private class IngMapping : ClassMap<TransactionCsv>
        {
            private string[] possibleAmountNames = new string[]
            {
                "Kwota transakcji (waluta rachunku)",
                "Kwota blokady/zwolnienie blokady",
                "Kwota płatności w walucie"
            };

            private IngMapping()
            {
                Map(transaction => transaction.Currency).Name("Waluta");
                Map(transaction => transaction.Date).Name("Data transakcji");
                Map(transaction => transaction.Description).Name("Dane kontrahenta");
                Map(transaction => transaction.Amount).Convert(args => MapAmount(args.Row));
            }

            private decimal MapAmount(IReaderRow row)
            {
                decimal amount;
                bool ifConverted;

                foreach (var possibleAmountName in possibleAmountNames)
                {
                    ifConverted = Decimal.TryParse((row.GetField<string>(possibleAmountName)), out amount);
                    if (ifConverted) return amount;
                }

                return 0;
            }
        }

        public IngCsvReader(string path) : base(path)
        { 
        }


        protected override CsvConfiguration SetConfiguration()
        {
            return new CsvConfiguration(cultureInfo: System.Globalization.CultureInfo.CurrentCulture)
            {
                MissingFieldFound = null,
                DetectDelimiter = true,
                BadDataFound = null,
            };
        }

        protected override void SkipToHeaderRecord(CsvReader csvReader)
        {
            for (int i = 0; i < 14; i++)
            {
                csvReader.Read();
            }
            csvReader.ReadHeader();
        }

        public override List<TransactionCsv?> GetTransactions()
        {
            List<TransactionCsv?> transactions = new List<TransactionCsv?>();   
            var config = SetConfiguration();
            using (var reader = new StreamReader(path))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<IngMapping>();
                SkipToHeaderRecord(csvReader);
                while (csvReader.Read())
                {
                    var transaction = csvReader.GetRecord<TransactionCsv?>();
                    transactions.Add(transaction);
                }
            }
            return transactions;
        }

       
    }
}
