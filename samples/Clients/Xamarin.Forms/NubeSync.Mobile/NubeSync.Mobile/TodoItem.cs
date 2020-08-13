using System;
using System.Collections.Generic;
using System.Text;
using NubeSync.Client.Data;

namespace NubeSync.Mobile
{
    public class TodoItem : NubeTable
    {
        public string Name { get; set; }

        public bool IsChecked { get; set; }
    }
}
