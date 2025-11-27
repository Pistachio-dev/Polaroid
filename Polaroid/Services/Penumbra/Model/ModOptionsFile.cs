using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Penumbra.Model
{
    public class ModOptionsFile
    {
        public int Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Page { get; set; }
        public int Priority { get; set; }
        public string Type { get; set; }
        public int DefaultSettings { get; set; }

        public List<OptionItem> Options { get; set; }
    }

    public class OptionItem
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Dictionary<string, string> Files { get; set; }
        public Dictionary<string, string> FileSwaps { get; set; }

        public List<object> Manipulations { get; set; }
    }
}
