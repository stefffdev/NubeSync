using SQLite;

namespace NubeSync.Client.SQLiteStore
{
    internal class NubeSetting
    {
        [PrimaryKey]
        public string Id { get; set; } = null!;

        public string? Value { get; set; }
    }
}