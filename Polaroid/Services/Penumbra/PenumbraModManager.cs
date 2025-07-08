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
        private static string ModName = "[Pistachio] Take real photos";
        private static string Penumbra = "Penumbra";
        public static string? GetCurrentSlotPath()
        {
            GetPenumbraModFolder();
            return null;
        }

        private static string GetPenumbraModFolder()
        {
            string rootPath = GetAllPluginConfigFolder();
            string penumbraConfigJson = File.ReadAllText(Path.Combine(rootPath, $"{Penumbra}.json"));
            JsonDocument penumbraConfig = JsonDocument.Parse(penumbraConfigJson);
            string penumbraModFilesPath = penumbraConfig.RootElement.GetProperty("ModDirectory").GetString();
            if (penumbraModFilesPath == null)
            {
                throw new Exception("Penumbra mod directory is not set!");
            }


            return penumbraModFilesPath;
        }

        public static int GetCurrentSlot()
        {
            string rootPath = GetAllPluginConfigFolder();
            // Step one: get the active collection
            var active_collections_path = Path.Combine(rootPath, Penumbra, "active_collections.json");
            string jsonText = File.ReadAllText(active_collections_path);
            JsonDocument activeColDocument = JsonDocument.Parse(jsonText);
            string currentColId = activeColDocument.RootElement.GetProperty("Current").GetString() ?? "Could not access current collection. There should be at least the default one?";
            var modConfigPath = Path.Combine(rootPath, Penumbra, "collections", $"{currentColId}.json");
            string modConfigJsonText = File.ReadAllText(modConfigPath);
            JsonDocument configDocument = JsonDocument.Parse(modConfigJsonText);
            int slot = configDocument
                .RootElement.GetProperty("Settings")
                .GetProperty(ModName)
                .GetProperty("Settings").GetProperty("Slot (please don't touch)")
                .GetInt32();

            Plugin.Log.Info("Slot: " + slot);

            return slot;
        }

        private static string GetAllPluginConfigFolder()
        {
            string polaroidConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
            string rootPath = polaroidConfigDirectory.Substring(0, polaroidConfigDirectory.LastIndexOf(Path.DirectorySeparatorChar));
            
            return rootPath;
        }
    }
}
