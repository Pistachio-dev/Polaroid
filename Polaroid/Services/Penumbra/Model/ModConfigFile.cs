using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Penumbra.Model
{
    internal class ModConfigFileRoot
    {
        public int Version { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, ModConfigEntry> Settings { get; set; }

        public object[] Inheritance { get; set; }
    }

    internal class ModConfigEntry
    {
        public Dictionary<string, object> Settings { get; set; }
        public int Priority { get; set; }
        public bool Enabled { get; set; }
    }
}
