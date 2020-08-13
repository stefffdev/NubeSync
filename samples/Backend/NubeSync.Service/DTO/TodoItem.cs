using NubeSync.Server.Data;

namespace NubeSync.Service.DTO
{
    public class TodoItem : NubeTable
    {
        public string Name { get; set; }

        public bool IsChecked { get; set; }
    }
}