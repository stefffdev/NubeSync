using System;
using NubeSync.Core;

namespace Tests.NubeSync.Core
{
    public class TestItem : NubeTable
    {
        public DateTimeOffset? DeletedAt { get; set; }

        public string Name { get; set; }
    }
}