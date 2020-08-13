using System;
using NubeSync.Client.Data;

namespace Tests.NubeSync.Client
{
    public class TestItem : NubeTable
    {
        public DateTimeOffset? DeletedAt { get; set; }

        public string Name { get; set; }
    }
}