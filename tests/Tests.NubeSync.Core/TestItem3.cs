using NubeSync.Core;
using System;

namespace Tests.NubeSync.Core
{
    public class TestItem3 : NubeTable
    {
        public string[] Array { get; set; }

        public bool Bool { get; set; }

        public byte Byte { get; set; }

        public char Char { get; set; }

        public TestItem2 ComplexType { get; set; }

        public DateTime DateTime { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public decimal Decimal { get; set; }

        public double Double { get; set; }

        public TestEnum Enum { get; set; }

        public float Float { get; set; }

        public Guid Guid { get; set; }

        public int Int { get; set; }

        public long Long { get; set; }

        public sbyte SByte { get; set; }

        public short Short { get; set; }

        public string SimpleType { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public uint UInt { get; set; }

        public ulong ULong { get; set; }

        public ushort UShort { get; set; }
    }
}