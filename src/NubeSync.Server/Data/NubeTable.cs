using System;
using System.Text.Json.Serialization;

namespace NubeSync.Server.Data
{
    public class NubeTable
    {
        [JsonIgnore]
        public int ClusteredIndex { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public string Id { get; set; } = null!;

        public DateTimeOffset ServerUpdatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public string? UserId { get; set; }
    }
}