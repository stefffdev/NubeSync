using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NubeSync.Client.Data
{
    public abstract class NubeTable
    {
        public DateTimeOffset CreatedAt { get; set; }

        public string Id { get; set; } = null!;

        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Returns all the prorperties of the object that should be stored in the operations.
        /// This method can be overwritten to avoid reflection.
        /// </summary>
        public virtual Dictionary<string, string?> GetProperties()
        {
            var result = new Dictionary<string, string?>();
            IList<PropertyInfo> props = new List<PropertyInfo>(GetType().GetProperties()
                .Where(p => p.Name != nameof(Id)));

            foreach (var prop in props)
            {
                if (prop.GetValue(this, null) is object value &&
                    Convert.ToString(value, CultureInfo.InvariantCulture) is string stringValue)
                {
                    result.Add(prop.Name, stringValue);
                }
                else
                {
                    result.Add(prop.Name, null);
                }
            }

            return result;
        }
    }
}