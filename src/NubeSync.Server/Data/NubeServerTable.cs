using System;
using System.Text.Json.Serialization;
using NubeSync.Core;

namespace NubeSync.Server.Data
{
    public class NubeServerTable : NubeTable
    {
        [JsonIgnore]
        public int ClusteredIndex { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset ServerUpdatedAt { get; set; }

        public string? UserId { get; set; }
    }
}