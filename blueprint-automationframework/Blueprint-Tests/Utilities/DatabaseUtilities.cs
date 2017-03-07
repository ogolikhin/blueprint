using System;
using System.Data.SqlClient;
using Common;
using System.ComponentModel;

namespace Utilities
{
    public static class DatabaseUtilities
    {
        /// <summary>
        /// Gets the specified field from the SqlDataReader stream as a string.
        /// </summary>
        /// <param name="reader">The SqlDataReader that holds the query results.</param>
        /// <param name="name">The name of the field to get.</param>
        /// <returns>The field value as a string.</returns>
        public static string GetValueAsString(SqlDataReader reader, string name)
        {
            ThrowIf.ArgumentNull(reader, nameof(reader));
            ThrowIf.ArgumentNull(name, nameof(name));

            try
            {
                int ordinal = reader.GetOrdinal(name);

                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return reader.GetValue(ordinal).ToString();
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.WriteError("*** Caught a IndexOutOfRangeException with field: {0}\n{1}", name, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the specified field from the SqlDataReader stream, or returns the default value if the field is null.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="reader">The SqlDataReader that holds the query results.</param>
        /// <param name="name">The name of the field to get.</param>
        /// <returns>The field value or the default value for the specified type.</returns>
        public static T GetValueOrDefault<T>(SqlDataReader reader, string name)
        {
            ThrowIf.ArgumentNull(reader, nameof(reader));
            ThrowIf.ArgumentNull(name, nameof(name));

            try
            {
                int ordinal = reader.GetOrdinal(name);

                if (reader.IsDBNull(ordinal))
                {
                    return default(T);
                }

                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(reader.GetValue(ordinal).ToString());
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.WriteError("*** Caught a IndexOutOfRangeException with field: {0}\n{1}", name, e.Message);
                return default(T);
            }
        }

        /// <summary>
        /// Gets the specified field from the SqlDataReader stream, or null.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="reader">The SqlDataReader that holds the query results.</param>
        /// <param name="name">The name of the field to get.</param>
        /// <returns>The field value or null.</returns>
        public static Nullable<T> GetValueOrNull<T>(SqlDataReader reader, string name) where T : struct
        {
            ThrowIf.ArgumentNull(reader, nameof(reader));
            ThrowIf.ArgumentNull(name, nameof(name));

            try
            {
                int ordinal = reader.GetOrdinal(name);

                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return (T)reader.GetValue(ordinal);
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.WriteError("*** Caught a IndexOutOfRangeException with field: {0}\n{1}", name, e.Message);
                return default(T);
            }
        }
    }
}
