using System;

namespace NubeSync.Client.Data
{
    public class NubeOperation
    {
        public NubeOperation()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTimeOffset.Now;
        }

        public DateTimeOffset CreatedAt { get; set; }

        public string Id { get; set; }

        public string ItemId { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? Property { get; set; }

        public string TableName { get; set; } = string.Empty;

        public OperationType Type { get; set; }

        public string? Value { get; set; }

        public override string ToString()
        {
            return $"Id {Id}, {Type} {Property} in table {TableName} for item {ItemId} with value {Value} (old: {OldValue})";
        }
    }
}