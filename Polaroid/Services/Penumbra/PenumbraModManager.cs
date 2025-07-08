using Swan.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Polaroid.Services.Penumbra
{
    // Manages the mod that lets you show the last picture
    internal static class PenumbraModManager
    {
        public static string? GetCurrentSlotPath()
        {
            GetPenumbraModFolder();
            return null;
        }

        private static void GetPenumbraModFolder()
        {
            string polaroidConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
            string rootPath = polaroidConfigDirectory.Substring(0, polaroidConfigDirectory.LastIndexOf(Path.DirectorySeparatorChar));
            string penumbraConfigJson = File.ReadAllText(Path.Combine(rootPath, "Penumbra.json"));
            JsonDocument penumbraConfig = JsonDocument.Parse(penumbraConfigJson);
            string penumbraModFilesPath = penumbraConfig.RootElement.GetProperty("ModDirectory").GetString();
            if (penumbraModFilesPath == null)
            {
                throw new Exception("Penumbra mod directory is not set!");
            }

            Plugin.Log.Info("Path: " + penumbraModFilesPath);
        }
    }
}
