using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace C4Compile.Csv
{
    public class CsvReader : IDisposable
    {
        public bool EndOfLine { get; private set; }
        public bool EndOfInput { get; private set; }
        public IList<string> Headers { get; set; }

        public Func<string, string, Type, object> ValueParser { get; set; } = ParseValue;

        private StringBuilder sb = new StringBuilder();
        private TextReader reader;
        private char separator;
        private char quote;

        public CsvReader(TextReader reader, char separator = ',', char quote = '"')
        {
            this.reader = reader;
            this.separator = separator;
            this.quote = quote;
        }

        public static object ParseValue(string name, string value, Type toType)
        {
            return Convert.ChangeType(value, toType);
        }

        public string Read()
        {
            EndOfLine = false;
            EndOfInput = false;
            sb.Length = 0;

            var quoted = false;
            for (; ; )
            {
                var c = reader.Read();
                if (c == -1)
                {
                    EndOfInput = true;
                    break;
                }

                if (quoted)
                {
                    if (c == quote)
                    {
                        if (reader.Peek() == quote)
                        {
                            sb.Append((char)c);
                        }
                        else
                        {
                            quoted = false;
                        }
                    }
                    else
                    {
                        sb.Append((char)c);
                    }
                }
                else
                {
                    if (c == '\r')
                    {
                        if (reader.Peek() == '\n')
                            reader.Read();
                        EndOfLine = true;
                        break;
                    }
                    else if (c == '\n')
                    {
                        EndOfLine = true;
                        break;
                    }
                    else if (c == separator)
                    {
                        break;
                    }
                    else if (c == quote)
                    {
                        quoted = true;
                    }
                    else
                    {
                        sb.Append((char)c);
                    }
                }
            }
            return sb.ToString();
        }

        public IList<string> ReadLine()
        {
            if (EndOfInput)
                return null;

            var items = new List<string>();
            for (; ; )
            {
                var item = Read();
                if (EndOfLine || EndOfInput)
                    if (items.Count == 0 && IsNullOrWhiteSpace(item))
                        return items;

                items.Add(item);
                if (EndOfLine || EndOfInput)
                    return items;
            }
        }

        public IList<string> ReadHeaders()
        {
            return Headers = ReadLine();
        }

        public IDictionary<string, string> ReadDictionary()
        {
            if (Headers == null)
                throw new InvalidOperationException("Must use ReadHeaders before ReadDictionary");

            if (EndOfInput)
                return null;

            var dict = new Dictionary<string, string>();
            var index = 0;
            for (; ; )
            {
                if (index > Headers.Count)
                    throw new InvalidOperationException("Line items cannot be more than headers");

                var item = Read();
                if (EndOfLine || EndOfInput)
                    if (index == 0 && IsNullOrWhiteSpace(item))
                        return dict;

                dict[Headers[index++]] = item;
                if (EndOfLine || EndOfInput)
                    break;
            }
            return dict;
        }

        public IEnumerable<IDictionary<string, string>> EnumerateDictionary()
        {
            while (!EndOfInput)
                yield return ReadDictionary();
        }

        public T ReadObject<T>(Func<T> creator)
        {
            if (Headers == null)
                throw new InvalidOperationException("Must use ReadHeaders before ReadObject");

            if (EndOfInput)
                return default(T);

            var type = typeof(T);
            var t = creator();
            var index = 0;
            for (; ; )
            {
                if (index > Headers.Count)
                    throw new InvalidOperationException("Line items cannot be more than headers");

                var item = Read();
                if (EndOfLine || EndOfInput)
                    if (index == 0 && IsNullOrWhiteSpace(item))
                        return t;

                var header = Headers[index++];
                var property = type.GetProperty(header);
                if (property != null)
                {
                    var value = ValueParser(header, item, property.PropertyType);
                    property.SetValue(t, value, null);
                }
                else
                {
                    var field = type.GetField(header);
                    if (field != null)
                    {
                        var value = ValueParser(header, item, field.FieldType);
                        field.SetValue(t, value);
                    }
                }

                if (EndOfLine || EndOfInput)
                    break;
            }
            return t;
        }

        public T ReadObject<T>() where T : new()
        {
            return ReadObject(() => new T());
        }

        public IEnumerable<T> EnumerateObject<T>(Func<T> creator)
        {
            while (!EndOfInput)
                yield return ReadObject(creator);
        }

        public IEnumerable<T> EnumerateObject<T>() where T : new()
        {
            return EnumerateObject(() => new T());
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        private static bool IsNullOrWhiteSpace(string str)
        {
            return string.IsNullOrEmpty(str) || str.All(char.IsWhiteSpace);
        }
    }
}
