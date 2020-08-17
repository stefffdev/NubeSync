using NubeSync.Server.Data;

namespace NubeSync.Service.DTO
{
    public class TodoItem : NubeServerTable
    {
        public string Name { get; set; }

        public bool IsChecked { get; set; }
    }
}