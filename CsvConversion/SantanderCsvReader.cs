using CsvConversion.Mappers;
using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion
{
    public class SantanderCsvReader : BaseCsvReader
    {
        public SantanderCsvReader(string path) : base(path)
        {
        }


        private void SetCurrency(CsvReader? csvReader) => SantanderMapper.currency = csvReader?.GetField<string>(4)!;

        protected override CsvConfiguration SetConfiguration()
        {
            return new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ",",
                BadDataFound = null,
                HasHeaderRecord = false,
            };
        }

        protected override void SkipToHeaderRecord(CsvReader csvReader) { }
 
        public override List<TransactionCsv?> GetTransactions()
        {
            List<TransactionCsv?> transactions = new List<TransactionCsv?>();
            var config = SetConfiguration();
            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<SantanderMapper>();
                SetConverterOptions<DateTime>(csvReader, new[] { "dd-MM-yyyy", "yyyy-MM-dd" });
                csvReader.Read();
                SetCurrency(csvReader);
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
