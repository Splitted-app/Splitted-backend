using CsvConversion.Mappers;
using CsvHelper.Configuration;
using CsvHelper;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion
{
    public class MbankCsvReader : BankCsvReader
    {
        public MbankCsvReader(string path) : base(path)
        {
        }


        protected override CsvConfiguration SetConfiguration()
        {
            return new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ";",
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header.Replace("#", string.Empty),
            };
        }

        protected override void SkipToHeaderRecord(CsvReader csvReader)
        {
            while (true)
            {
                csvReader.Read();
                string? field = csvReader.GetField<string>(0);
                if (field is not null && field.Contains("Data")) break;
            }
            csvReader.ReadHeader();
        }

        public override List<TransactionCsv?> GetTransactions()
        {
            List<TransactionCsv?> transactions = new List<TransactionCsv?>();
            var config = SetConfiguration();
            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<MbankMapper>();
                SetConverterOptions<DateTime>(csvReader, new[] { "dd.MM.yyyy", "yyyy.MM.dd" });
                SkipToHeaderRecord(csvReader);
                while (csvReader.Read())
                {
                    var field = csvReader.GetField<string>(0);
                    if (field!.Equals("")) break;
                    var transaction = csvReader.GetRecord<TransactionCsv?>();
                    transactions.Add(transaction);
                }
            }
            return transactions;
        }
    }
}
