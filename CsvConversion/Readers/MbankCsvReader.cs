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

namespace CsvConversion.Readers
{
    public class MbankCsvReader : BaseCsvReader
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

        protected override bool DetermineEndOfTransactions(CsvReader csvReader)
        {
            var field = csvReader.GetField<string>(0);
            if (field!.Equals("")) return true;
            else return false;
        }

        public override List<TransactionCsv?> GetTransactions() => base.GetSpecificTransactions<MbankMapper>(new[] { "dd.MM.yyyy", "yyyy.MM.dd" });
    }
}
