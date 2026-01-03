using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Penumbra.Model
{
    public class DefaultModDefinitionFile
    {
        public int Version { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public Dictionary<string, string> FileSwaps { get; set; }
        public List<string> Manipulations { get; set; }
    }
}
