namespace NubeSync.Server.Data
{
    public enum ProcessingType : byte
    {
        Processed,
        DiscaredOutdated,
        DiscardedDeleted
    }
}