using Dalamud.Game.Network.Structures;
using ECommons.Reflection;
using FFXIVClientStructs;
using Polaroid.Services.Penumbra.Model;
using Serilog;
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
        private static string SlotPropertyName = "Slot (please don't touch)";
        private static string Penumbra = "Penumbra";
        private static string TextureFolderPath => Path.Combine(ModName, "vfx", "common", "texture");
        private static string Extension = ".tex";


        public static string CleanOldFilesAndPrepareNextTexturePath()
        {
            int currentSlot = GetCurrentSlot();
            var nextTextureRoute = GetTextureFullPath(GetNextSlot(currentSlot));
            if (File.Exists(nextTextureRoute))
            {
                Log.Information("Deleting old texture at " + nextTextureRoute);
                File.Delete(nextTextureRoute);
            }

            int newSlot = AdvanceCurrentSlot();
            Log.Information("Slot moved to " + newSlot);
            return GetTextureFullPath(newSlot);
        }

        public static string GetTextureFolder()
        {
            return Path.Combine(GetPenumbraModFolder(), TextureFolderPath);
        }

        private static string GetTextureFileName(int slot)
        {
            return $"flag_f{slot}{Extension}";
        }

        private static int GetNextSlot(int slot)
        {
            return (slot + 1) % 20;
        }



        private static string GetTextureFullPath(int slot)
        {
            string penumbraModFolder = GetPenumbraModFolder();
            string fullPath = Path.Combine(penumbraModFolder, TextureFolderPath, GetTextureFileName(slot));
            return fullPath;
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

        private static int GetCurrentSlot()
        {
            string modConfigPath = GetModConfigFilePath();
            string modConfigJsonText = File.ReadAllText(modConfigPath);
            ModConfigFileRoot modConfigFile = Json.Deserialize<ModConfigFileRoot>(modConfigJsonText);
            int currentSlot = (int)modConfigFile.Settings[ModName].Settings[SlotPropertyName];

            return currentSlot;
        }
        private static string GetModConfigFilePath()
        {
            string rootPath = GetAllPluginConfigFolder();
            // Step one: get the active collection
            var active_collections_path = Path.Combine(rootPath, Penumbra, "active_collections.json");
            string jsonText = File.ReadAllText(active_collections_path);
            JsonDocument activeColDocument = JsonDocument.Parse(jsonText);
            string currentColId = activeColDocument.RootElement.GetProperty("Current").GetString() ?? "Could not access current collection. There should be at least the default one?";
            var modConfigPath = Path.Combine(rootPath, Penumbra, "collections", $"{currentColId}.json");

            return modConfigPath;
        }


        public static int AdvanceCurrentSlot()
        {
            string modConfigPath = GetModConfigFilePath();
            string modConfigJsonText = File.ReadAllText(modConfigPath);
            ModConfigFileRoot modConfigFile = Newtonsoft.Json.JsonConvert.DeserializeObject<ModConfigFileRoot>(modConfigJsonText);
            long currentSlot = (long)modConfigFile.Settings[ModName].Settings[SlotPropertyName];
            int newSlot = GetNextSlot((int)currentSlot);
            modConfigFile.Settings[ModName].Settings[SlotPropertyName] = newSlot;
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(modConfigFile);
            File.WriteAllText(modConfigPath, serialized);

            Plugin.Log.Info("New slot: " + newSlot);

            return (int)newSlot;
        }

        private static string GetAllPluginConfigFolder()
        {
            string polaroidConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
            string rootPath = polaroidConfigDirectory.Substring(0, polaroidConfigDirectory.LastIndexOf(Path.DirectorySeparatorChar));
            
            return rootPath;
        }
    }
}
