using System;
using System.IO;
using System.Linq;
using C4Compile.Csv;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private class Item
        {
            public string abc { get; set; }
            public string cde;

            public override string ToString()
            {
                return $"abc={abc} cde={cde}";
            }
        }

        private string input =
@"abc,""cde""

""efg"",ghi";

        [TestMethod]
        public void TestRead()
        {
            using (var reader = new CsvReader(new StringReader(input)))
            {
                while (!reader.EndOfInput)
                {
                    var item = reader.Read();
                    Console.WriteLine($"{item} {reader.EndOfLine} {reader.EndOfInput}");
                }
            }
        }

        [TestMethod]
        public void TestReadLine()
        {
            using (var reader = new CsvReader(new StringReader(input)))
            {
                while (!reader.EndOfInput)
                {
                    var items = reader.ReadLine();
                    var item = string.Join(",", items);
                    Console.WriteLine($"{item} {reader.EndOfLine} {reader.EndOfInput}");
                }
            }
        }

        [TestMethod]
        public void TestReadDictionary()
        {
            using (var reader = new CsvReader(new StringReader(input)))
            {
                reader.ReadHeaders();
                while (!reader.EndOfInput)
                {
                    var dictionary = reader.ReadDictionary();
                    var item = string.Join(",", dictionary.Select(i => i.Key + "=" + i.Value));
                    Console.WriteLine($"{item} {reader.EndOfLine} {reader.EndOfInput}");
                }
            }
        }

        [TestMethod]
        public void TestReadObject()
        {
            using (var reader = new CsvReader(new StringReader(input)))
            {
                reader.ReadHeaders();
                while (!reader.EndOfInput)
                {
                    var item = reader.ReadObject<Item>();
                    Console.WriteLine($"{item} {reader.EndOfLine} {reader.EndOfInput}");
                }
            }
        }
    }
}
