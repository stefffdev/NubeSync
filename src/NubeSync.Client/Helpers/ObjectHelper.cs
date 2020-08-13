using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace NubeSync.Client.Helpers
{
    internal static class ObjectHelper
    {
        internal static T Clone<T>(this T source)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source));
        }

        internal static void CopyProperties(this object source, object destination)
        {
            if (source == null || destination == null)
            {
                throw new ArgumentNullException("Source and/or Destination Objects are null");
            }

            var typeDest = destination.GetType();
            var typeSrc = source.GetType();

            if (typeSrc != typeDest)
            {
                throw new InvalidOperationException("Cannot copy properties to different object types");
            }

            var srcProps = typeSrc.GetProperties().Where(p => p.Name != "Id");
            foreach (var srcProp in srcProps)
            {
                if (srcProp.CanRead && typeDest.GetProperty(srcProp.Name) is PropertyInfo targetProperty &&
                    targetProperty.CanWrite && targetProperty.GetSetMethod() is MethodInfo methodInfo && methodInfo.IsPublic && (methodInfo.Attributes & MethodAttributes.Static) == 0 &&
                    targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
                }
            }
        }
    }
}