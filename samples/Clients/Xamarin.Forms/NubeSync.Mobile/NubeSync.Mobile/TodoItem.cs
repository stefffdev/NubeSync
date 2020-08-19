using NubeSync.Core;

namespace NubeSync.Mobile
{
    public class TodoItem : NubeTable
    {
        public string Name { get; set; }

        public bool IsChecked { get; set; }
    }
}
