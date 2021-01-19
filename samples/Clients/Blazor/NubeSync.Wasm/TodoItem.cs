using System;

namespace NubeSync.Wasm
{
    public class TodoItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsChecked { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}