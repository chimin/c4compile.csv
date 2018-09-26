﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace C4Compile.Csv
{
    public class CsvWriter : IDisposable
    {
        public bool LineHasItem { get; set; }
        public IList<string> Headers { get; set; }

        public Func<string, object, Type, string> ValueFormatter { get; set; } = FormatValue;

        private TextWriter writer;
        private char separator;
        private char quote;

        public CsvWriter(TextWriter writer = null, char separator = ',', char quote = '"')
        {
            this.writer = writer ?? new StringWriter();
            this.separator = separator;
            this.quote = quote;
        }

        public static string FormatValue(string name, object value, Type fromType)
        {
            return value?.ToString();
        }

        public void Write(object item)
        {
            if (LineHasItem)
                writer.Write(separator);

            LineHasItem = true;

            var str = item?.ToString() ?? "";
            var quoted = str?.Intersect(new[] { separator, quote }).Any() ?? false;
            if (quoted)
            {
                writer.Write(quote);
                for (var i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    if (c == quote)
                    {
                        writer.Write(quote);
                        writer.Write(quote);
                    }
                    else
                    {
                        writer.Write(c);
                    }
                }
                writer.Write(quote);
            }
            else
            {
                writer.Write(str);
            }
        }

        public void Write(params object[] items)
        {
            Write((IEnumerable<object>)items);
        }

        public void Write(IEnumerable<object> items)
        {
            foreach (var item in items)
                Write(item);
        }

        public void WriteLine()
        {
            writer.WriteLine();
            LineHasItem = false;
        }

        public void WriteLine(params object[] items)
        {
            WriteLine((IEnumerable<object>)items);
        }

        public void WriteLine(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                Write(item);
            }
            WriteLine();
        }

        public void WriteDictionary(IDictionary<string, object> dict)
        {
            if (Headers == null)
                throw new InvalidOperationException("Must set Headers before WriteDictionary");

            WriteLine(Headers.Select(i => dict.TryGetValue(i, out var item) ? item : null));
        }

        public void WriteObject(object obj)
        {
            if (Headers == null)
                throw new InvalidOperationException("Must set Headers before WriteObject");

            var type = obj.GetType();
            WriteLine(Headers.Select(i =>
            {
                var property = type.GetProperty(i);
                if (property != null)
                    return FormatValue(i, property.GetValue(obj, null), property.PropertyType);

                var field = type.GetField(i);
                if (field != null)
                    return FormatValue(i, field.GetValue(obj), field.FieldType);

                return null;
            }));
        }

        public override string ToString()
        {
            return writer.ToString();
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
