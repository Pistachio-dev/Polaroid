using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Camera
{
    public static class CameraOffsets
    {
        public static readonly Dictionary<string, Vector2> Offsets = new Dictionary<string, Vector2>()
        {
            {"Midlander_Male", new Vector2(-0.48007202f, -1.480011f) },
            {"Midlander_Female", new Vector2(-0.34005737f, -1.4500122f) },
            {"Highlander_Male", new Vector2(-0.5600586f, -1.6300049f) },
            {"Highlander_Female", new Vector2(-0.4100647f, -1.6300049f) },
            {"Miqote_Male", new Vector2(-0.47006226f, -1.3800049f) },
            {"Miqote_Female", new Vector2(-0.35006714f, -1.4200134f) },
            {"Roegadyn_Male", new Vector2(-0.52008057f, -1.9200134f) },
            {"Roegadyn_Female", new Vector2(-0.460083f, -1.9500122f) },
            {"Lalafell_Male", new Vector2(-0.20007324f, -0.76000977f) },
            {"Lalafell_Female", new Vector2(-0.20007324f, -0.76000977f) },
            {"AuRa_Male", new Vector2(-0.6600647f, -1.730011f) },
            {"AuRa_Female", new Vector2(-0.38006592f, -1.3500061f) },
            {"Hrothgar_Male", new Vector2(-0.55007935f, -1.7900085f) },
            {"Hrothgar_Female", new Vector2(-0.45007324f, -1.730011f) },
            {"Viera_Male", new Vector2(-0.5100708f, -1.4700012f) },
            {"Viera_Female", new Vector2(-0.5100708f, -1.4700012f) },
            {"Elezen_Male", new Vector2(-0.5800781f, -1.6900024f) },
            {"Elezen_Female", new Vector2(-0.42007446f, -1.7399902f) },
        };
    }
}
