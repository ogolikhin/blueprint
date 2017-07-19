using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServiceLibrary.Helpers
{
    public sealed class ComplexObjectReader : IDisposable
    {
        private const char FalseChar = '0';
        private const char Delimeter = ' ';

        private readonly TextReader _reader;

        public ComplexObjectReader(TextReader reader)
        {
            _reader = reader;
        }

        public ComplexObjectReader(string value)
            : this(new StringReader(value))
        {
        }

        public string ReadString()
        {
            int count = ReadInt32();
            if (count == -1)
            {
                return null;
            }

            var buffer = new char[count];
            _reader.Read(buffer, 0, count);
            return new string(buffer);
        }

        public bool ReadBoolean()
        {
            return (_reader.Read() != FalseChar);
        }

        public int ReadInt32()
        {
            return ParseDelimitedString(int.Parse);
        }

        public long ReadLong()
        {
            return ParseDelimitedString(long.Parse);
        }

        public decimal ReadDecimal()
        {
            return ParseDelimitedString(Decimal.Parse);
        }

        public DateTime ReadDateTime()
        {
            return ParseDelimitedString(DateTime.Parse);
        }

        // e.g. reader.ReadArray(r => r.ReadString());
        public T[] ReadArray<T>(Func<ComplexObjectReader, T> readElement)
        {
            int count = ReadInt32();
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = readElement(this);
            }
            return result;
        }

        // e.g. reader.ReadCollection(new HashSet<int>(), r => r.ReadInt32());
        public T ReadCollection<T, TElement>(T collection, Func<ComplexObjectReader, TElement> readElement)
            where T : ICollection<TElement>
        {
            return ReadCollection(_ => collection, readElement);
        }

        // e.g. reader.ReadCollection(n => new List<int>(n), r => r.ReadInt32());
        public T ReadCollection<T, TElement>(Func<int, T> createCollection, Func<ComplexObjectReader, TElement> readElement)
            where T : ICollection<TElement>
        {
            int count = ReadInt32();
            T result = createCollection(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(readElement(this));
            }
            return result;
        }

        private T ParseDelimitedString<T>(Func<string, T> parse)
        {
            var builder = new StringBuilder();

            int c;
            while ((c = _reader.Read()) != Delimeter)
            {
                builder.Append((char)c);
            }
            return parse(builder.ToString());
        }

        #region IDisposable

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _reader.Dispose();

            _isDisposed = true;
        }

        #endregion IDisposable
    }
}
