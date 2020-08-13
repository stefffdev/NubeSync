using System.Linq;
using Microsoft.AspNetCore.Http;

namespace NubeSync.Service
{
    public static class HttpRequestExtension
    {
        public static string GetHeader(this HttpRequest request, string key)
        {
            return request.Headers.FirstOrDefault(x => x.Key.ToLower() == key.ToLower()).Value.FirstOrDefault();
        }

        public static string GetInstallationId(this HttpRequest request)
        {
            return request.GetHeader("NUBE-INSTALLATION-ID");
        }
    }
}