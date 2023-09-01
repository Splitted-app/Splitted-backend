using CsvConversion.Mappers;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Models.CsvModels;
using System.Linq.Expressions;
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

        protected List<TransactionCsv?> GetSpecificTransactions<T>(string[] formats) where T : ClassMap
        {
            List<TransactionCsv?> transactions = new List<TransactionCsv?>();
            CsvConfiguration config = SetConfiguration();
            ConvertToUtf8(path);

            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<T>();
                SetConverterOptions<DateTime>(csvReader, formats);
                SkipToHeaderRecord(csvReader);

                while (csvReader.Read() && !DetermineEndOfTransactions(csvReader))
                {
                    transactions.Add(csvReader.GetRecord<TransactionCsv?>());
                }
            }
            return transactions;
        }

        protected abstract bool DetermineEndOfTransactions(CsvReader csvReader);

        protected abstract CsvConfiguration SetConfiguration();

        protected abstract void SkipToHeaderRecord(CsvReader csvReader);

        public abstract List<TransactionCsv?> GetTransactions();
    }
}
