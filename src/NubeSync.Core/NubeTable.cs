using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NubeSync.Core
{
    public abstract class NubeTable
    {
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
        public virtual Dictionary<string, string?> GetProperties()
        {
            var result = new Dictionary<string, string?>();
            IList<PropertyInfo> props = new List<PropertyInfo>(GetType()
                .GetProperties()
                .Where(p => p.CanWrite &&
                p.Name != nameof(Id) && 
                p.Name != "ClusteredIndex" &&
                p.Name != "UserId" && 
                p.Name != "ServerUpdatedAt" && 
                p.Name != "DeletedAt"));

            foreach (var prop in props)
            {
                if (prop.GetValue(this, null) is object value &&
                    _ConvertToString(value) is string stringValue)
                {
                    result.Add(prop.Name, stringValue);
                }
                else
                {
                    result.Add(prop.Name, null);
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
    }
}