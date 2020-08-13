namespace NubeSync.Client
{
    public interface INubeClientConfiguration
    {
        /// <summary>
        /// The address of the server hosting the NubeSync REST APIs
        /// </summary>
        string Server { get; }
    }
}
