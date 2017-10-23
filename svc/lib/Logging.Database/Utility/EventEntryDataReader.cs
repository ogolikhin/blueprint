// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.SqlServer.Server;

namespace Logging.Database.Utility
{
    internal sealed class EventEntryDataReader : IDataReader, IConvertible
    {
        private readonly string _instanceName;
        private IEnumerator<EventEntry> _enumerator;
        private int _recordsAffected;
        private SqlDataRecord _currentRecord;

        public EventEntryDataReader(IEnumerable<EventEntry> collection, string instanceName)
        {
            _enumerator = collection.GetEnumerator();
            this._instanceName = instanceName;
        }

        #region IDataReader

        [ExcludeFromCodeCoverage]
        public int Depth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsClosed
        {
            get
            {
                return _enumerator == null;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return _recordsAffected;
            }
        }

        public int FieldCount
        {
            get
            {
                return EventEntryExtensions.Fields.Length;
            }
        }

        [ExcludeFromCodeCoverage]
        public object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ExcludeFromCodeCoverage]
        public object this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Close()
        {
            Dispose();
            _enumerator = null;
        }

        [ExcludeFromCodeCoverage]
        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            return Read();
        }

        public bool Read()
        {
            bool result = _enumerator.MoveNext();
            if (result)
            {
                _recordsAffected++;
                _currentRecord = _enumerator.Current.ToSqlDataRecord(_instanceName);
            }

            return result;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        [ExcludeFromCodeCoverage]
        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            return EventEntryExtensions.Fields[i];
        }

        public int GetOrdinal(string name)
        {
            return Array.IndexOf(EventEntryExtensions.Fields, name);
        }

        [ExcludeFromCodeCoverage]
        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            return _currentRecord.GetValue(i);
        }

        [ExcludeFromCodeCoverage]
        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IConvertible

        [ExcludeFromCodeCoverage]
        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            var list = new List<SqlDataRecord>();

            while (_enumerator.MoveNext())
            {
                list.Add(_enumerator.Current.ToSqlDataRecord(_instanceName));
            }

            _enumerator.Reset();

            return list;
        }

        [ExcludeFromCodeCoverage]
        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
