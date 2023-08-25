using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion
{
    public abstract class BankCsvReader
    {
        protected string path;


        public BankCsvReader(string path)
        {
            this.path = path;   
        }


        protected void SetConverterOptions<T>(CsvReader csvReader, string[] formats)
        {
            var options = new TypeConverterOptions { Formats = formats };
            csvReader.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
        }

        protected abstract CsvConfiguration SetConfiguration();

        protected abstract void SkipToHeaderRecord(CsvReader csvReader);

        public abstract List<TransactionCsv?> GetTransactions();
    }
}
