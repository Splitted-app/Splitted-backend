using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion
{
    internal static class EncodingConverter
    {
        private static readonly char[] polishCharacters = new char[] { 'ą', 'ę', 'ż', 'ź', 'ó', 'ł', 'ń', 'ś', 'ć' };
        private static readonly byte[] utf8BomBytes = new byte[] { 0xEF, 0xBB, 0xBF };


        private static Encoding DetermineEncoding(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] buffer = new byte[3];


            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 3)
                {
                    if (Enumerable.SequenceEqual(buffer, utf8BomBytes)) return Encoding.UTF8;
                }
            }

            using (StreamReader reader = new StreamReader(path))
            {
                string fileContent = reader.ReadToEnd().ToLower();
                return fileContent.IndexOfAny(polishCharacters) != -1 ? Encoding.UTF8 : Encoding.GetEncoding(1250);
            }
        }


        internal static void Encode(string path, Encoding originalEncoding, Encoding finalEncoding)
        {
            Encoding determinedEncoding = DetermineEncoding(path);
            if (determinedEncoding.Equals(finalEncoding)) return;

            string originalContent;

            using (StreamReader reader = new StreamReader(path, originalEncoding))
            {
                originalContent = reader.ReadToEnd();
            }

            byte[] originalBytes = originalEncoding.GetBytes(originalContent);
            byte[] finalBytes = Encoding.Convert(originalEncoding, finalEncoding, originalBytes);
            string finalContent = finalEncoding.GetString(finalBytes);

            using (StreamWriter writer = new StreamWriter(path, false, finalEncoding))
            {
                writer.Write(finalContent);
            }
        }
    }
}
