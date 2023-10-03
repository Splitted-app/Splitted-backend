using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion.Extensions
{
    public static class CsvReaderExtension
    {
        public static T? TryGetRecord<T>(this CsvReader csvReader,  out bool ifConverted)
        {
            try
            {
                T? convertedRecord = csvReader.GetRecord<T>();
                ifConverted = true;
                return convertedRecord;
            }
            catch (Exception)
            {
                ifConverted = false;
                return default;
            }
        }
    }
}
