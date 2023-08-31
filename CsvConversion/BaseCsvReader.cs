using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Models.CsvModels;


namespace CsvConversion
{
    public abstract class BaseCsvReader
    {
        protected string path;


        public BaseCsvReader(string path)
        {
            this.path = path;   
        }


        protected void SetConverterOptions<T>(CsvReader csvReader, string[] formats)
        {
            var options = new TypeConverterOptions { Formats = formats };
            csvReader.Context.TypeConverterOptionsCache.AddOptions<T>(options);
        }

        protected abstract CsvConfiguration SetConfiguration();

        protected abstract void SkipToHeaderRecord(CsvReader csvReader);

        public abstract List<TransactionCsv?> GetTransactions();
    }
}
