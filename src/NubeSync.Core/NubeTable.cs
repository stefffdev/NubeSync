using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NubeSync.Core
{
    public abstract class NubeTable
    {
        private static readonly List<Type> VALID_TYPES = new List<Type>
        {
            typeof (string),
            typeof (char),
            typeof (Guid),
            typeof (bool),
            typeof (byte),
            typeof (short),
            typeof (int),
            typeof (long),
            typeof (float),
            typeof (double),
            typeof (decimal),
            typeof (sbyte),
            typeof (ushort),
            typeof (uint),
            typeof (ulong),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (TimeSpan),
        };

        /// <summary>
        /// The timestamp when the record was saved for the first time.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The id of the record.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The timestamp when the record was saved the last time.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Returns all the prorperties of the object that should be stored in the operations.
        /// This method can be overwritten to avoid reflection.
        /// </summary>
        public virtual Dictionary<string, (string? Value, bool IsDefault)> GetProperties()
        {
            var result = new Dictionary<string, (string? Value, bool IsDefault)>();
            IList<PropertyInfo> props = new List<PropertyInfo>(GetType()
                .GetProperties()
                .Where(p => p.CanWrite &&
                p.Name != nameof(Id) &&
                p.Name != "ClusteredIndex" &&
                p.Name != "UserId" &&
                p.Name != "ServerUpdatedAt" &&
                p.Name != "DeletedAt" &&
                _IsValidType(p.PropertyType)));

            foreach (var prop in props)
            {
                if (prop.GetValue(this, null) is object value &&
                    _ConvertToString(value) is string stringValue)
                {
                    result.Add(prop.Name, (stringValue, _IsDefaultValue(value)));
                }
                else
                {
                    result.Add(prop.Name, (null, true)); 
                }
            }

            static string? _ConvertToString(object value)
            {
                if (value is DateTimeOffset dateTime)
                {
                    return dateTime.ToString("o", CultureInfo.InvariantCulture);
                }

                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return result;
        }

        private static bool _IsDefaultValue(object value)
        {
            return value switch
            {
                string stringValue => string.IsNullOrEmpty(stringValue),
                char charValue => charValue == default,
                Guid guidValue => guidValue == default,
                bool boolValue => boolValue == default,
                byte byteValue => byteValue == default,
                short shortValue => shortValue == default,
                int intValue => intValue == default,
                long longValue => longValue == default,
                float floatValue => floatValue == default,
                double doubleValue => doubleValue == default,
                decimal decimalValue => decimalValue == default,
                sbyte sbyteValue => sbyteValue == default,
                ushort ushortValue => ushortValue == default,
                uint uintValue => uintValue == default,
                ulong ulongValue => ulongValue == default,
                DateTime dateTimeValue => dateTimeValue == default,
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue == default,
                TimeSpan timeSpanValue => timeSpanValue == default,
                _ => false,
            };
        }

        private static bool _IsValidType(Type type)
        {
            return VALID_TYPES.Contains(type) || type.IsEnum;
        }
    }
}