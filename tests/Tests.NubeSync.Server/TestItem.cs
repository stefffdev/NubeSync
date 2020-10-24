using NubeSync.Server.Data;

namespace Tests.NubeSync.Server
{
    public class TestItem : NubeServerTable
    {
        public string Name { get; set; }

        public int Value { get; set; }
    }
}