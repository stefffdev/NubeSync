using System;
using System.Text.Json.Serialization;
using NubeSync.Core;

namespace NubeSync.Server.Data
{
    public class NubeServerOperation : NubeOperation
    {
        [JsonIgnore]
        public int ClusteredIndex { get; set; }

        public string? InstallationId { get; set; }

        public ProcessingType ProcessingType { get; set; }

        public DateTimeOffset ServerUpdatedAt { get; set; }

        public string? UserId { get; set; }

        public override string ToString()
        {
            return $"Id {Id}, {Type} in table {TableName} for item {ItemId} with value {Value} (old: {OldValue}) {CreatedAt}";
        }
    }
}