using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Models.CsvModels;
using System.Text;

namespace CsvConversion.Readers
{
    public abstract class BaseCsvReader
    {
        protected string path;


        public BaseCsvReader(string path)
        {
            this.path = path;
        }

        protected void ConvertToUtf8(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            EncodingConverter.Encode(path, Encoding.GetEncoding(1250), Encoding.UTF8);
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
