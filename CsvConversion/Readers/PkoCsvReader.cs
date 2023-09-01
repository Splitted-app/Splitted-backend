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
    public class PkoCsvReader : BaseCsvReader
    {
        public PkoCsvReader(string path) : base(path)
        {
        }


        protected override CsvConfiguration SetConfiguration()
        {
            return new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ",",
                BadDataFound = null,
            };
        }

        protected override void SkipToHeaderRecord(CsvReader csvReader)
        {
            csvReader.Read();
            csvReader.ReadHeader();
        }

        protected override bool DetermineEndOfTransactions(CsvReader csvReader) => false;

        public override List<TransactionCsv?> GetTransactions() => base.GetSpecificTransactions<PkoMapper>(new[] { "dd-MM-yyyy", "yyyy-MM-dd" });
     
    }
}
