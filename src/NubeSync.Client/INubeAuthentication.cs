using System.Threading.Tasks;

namespace NubeSync.Client
{
    public interface INubeAuthentication
    {
        /// <summary>
        /// Gets the bearer token that is sent with the REST queries to authorize the client.
        /// </summary>
        /// <returns>The authentication token.</returns>
        Task<string> GetBearerTokenAsync();
    }
}
