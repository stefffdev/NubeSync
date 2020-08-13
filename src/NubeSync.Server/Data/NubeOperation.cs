using System;
using System.Text.Json.Serialization;

namespace NubeSync.Server.Data
{
    public class NubeOperation
    {
        [JsonIgnore]
        public int ClusteredIndex { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string Id { get; set; } = null!;

        public string? InstallationId { get; set; }

        public string ItemId { get; set; } = null!;

        public string? OldValue { get; set; }

        public ProcessingType ProcessingType { get; set; }

        public string? Property { get; set; }

        public DateTimeOffset ServerUpdatedAt { get; set; }

        public string TableName { get; set; } = null!;

        public OperationType Type { get; set; }

        public string? UserId { get; set; }

        public string? Value { get; set; }

        public override string ToString()
        {
            return $"Id {Id}, {Type} in table {TableName} for item {ItemId} with value {Value} (old: {OldValue}) {CreatedAt}";
        }
    }
}