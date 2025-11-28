using Dalamud.Game.Network.Structures;
using ECommons.Reflection;
using FFXIVClientStructs;
using Newtonsoft.Json.Linq;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Polaroid.Services.Penumbra
{
    // Manages the mod that lets you show the last picture
    internal static class PenumbraModManager
    {
        private static string ModName = "Sign Holding [Hum] [Mittens]";
        private static string Penumbra = "Penumbra";
        private static string TextureFolderPath => Path.Combine(ModName, "Sign", "photograph", "vfx");
        private static string BaseFileName = "photograph_capture_";
        private static string Extension = ".atex";
        private static string Group2JsonFileName = "group_002_sign.json";
        private static Regex RouteMatch = new Regex($"{BaseFileName}([0-9]+)\\.atex");
        private static string MemoizedPenumbraModFolder = string.Empty;
        public static string PenumbraModFolder { get
            {
                if (MemoizedPenumbraModFolder == string.Empty)
                {
                    MemoizedPenumbraModFolder = GetPenumbraModFolder();
                }

                return MemoizedPenumbraModFolder;
        } }

        public static async Task EnsureModExists()
        {
            if (!DoesHoldSignModExist())
            {
                string errorMessage = "Sign Holding [Hum] [Mittens] mod is missing";
                Plugin.ChatGui.PrintError(errorMessage);
                Plugin.Log.Error(errorMessage);
                throw new NotImplementedException();
            }

            if (!IsModAdapated())
            {
                Plugin.Log.Warning("Adapting SignHolding mod to use this plugin");
                await AdaptDefault_Json();
                await Add_Photograph_Option();
                Generate_Directories();
                ReloadMod();
            }
            else
            {
                Plugin.Log.Info("SignHolding Mod is adapted");
            }
        }
        public static bool ReloadMod()
        {            
            Plugin.PenumbraIpc.ReloadMod(GetModRootPath(), ModName);
            Plugin.Log.Info("Mod reloaded");
            return true;
        }
        public static string GetModRootPath()
        {
            return Path.Combine(PenumbraModFolder, ModName);
        }
        public static string GetTextureFolder()
        {
            return Path.Combine(PenumbraModFolder, TextureFolderPath);
        }

        public static string GetNewTextureFullPath()
        {
            var fileName = GetNextFileName();
            return Path.Combine(GetTextureFolder(), GetNextFileName());
        }

        private static string GetTextureFileName(int number)
        {
            return $"{BaseFileName}{number}{Extension}";
        }

        private static string GetNextFileName()
        {
            var lastTextureFileName = Directory.EnumerateFiles(GetTextureFolder()).OrderBy(file => int.Parse(file.Split("_").Last().Split(".")[0])).LastOrDefault();
            Plugin.Log.Info("Last file name: " + lastTextureFileName);
            if (lastTextureFileName == null)
            {
                return GetTextureFileName(0);
            }

            var match = RouteMatch.Match(lastTextureFileName);
            if (match.Groups.Count < 2 || !int.TryParse(match.Groups[1].Value, out int number))
            {
                Plugin.Log.Error($"Could not find a file matching the pattern in folder {GetTextureFolder()}");
                return GetTextureFileName(0);
            }

            return GetTextureFileName(number + 1);
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

        public static async Task ModifyPhotographFileRoute()
        {
            var path = Path.Combine(GetModRootPath(), Group2JsonFileName);
            if (!File.Exists(path)) throw new Exception("Mod json file not found. Is it not installed?");
            string jsonFile = await File.ReadAllTextAsync(path);
            var match = RouteMatch.Match(jsonFile);
            string oldFileName = match.Groups[0].Value;
            string newFileName = GetNextFileName();
            Plugin.Log.Info("Old file in config: " + oldFileName);
            Plugin.Log.Info("New file in config: " + newFileName);
                
            jsonFile = jsonFile.Replace(oldFileName, newFileName);
            await File.WriteAllTextAsync(path, jsonFile);
        }

        //private static string GetModConfigFilePath()
        //{
        //    string rootPath = GetAllPluginConfigFolder();
        //    // Step one: get the active collection
        //    var active_collections_path = Path.Combine(rootPath, Penumbra, "active_collections.json");
        //    string jsonText = File.ReadAllText(active_collections_path);
        //    JsonDocument activeColDocument = JsonDocument.Parse(jsonText);
        //    string currentColId = activeColDocument.RootElement.GetProperty("Current").GetString() ?? "Could not access current collection. There should be at least the default one?";
        //    var modConfigPath = Path.Combine(rootPath, Penumbra, "collections", $"{currentColId}.json");

        //    return modConfigPath;
        //}


        //public static int AdvanceCurrentSlot()
        //{
        //    string modConfigPath = GetModConfigFilePath();
        //    string modConfigJsonText = File.ReadAllText(modConfigPath);
        //    ModConfigFileRoot modConfigFile = Newtonsoft.Json.JsonConvert.DeserializeObject<ModConfigFileRoot>(modConfigJsonText);
        //    long currentSlot = (long)modConfigFile.Settings[ModName].Settings[SlotPropertyName];
        //    int newSlot = GetNextSlot((int)currentSlot);
        //    modConfigFile.Settings[ModName].Settings[SlotPropertyName] = newSlot;
        //    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(modConfigFile);
        //    File.WriteAllText(modConfigPath, serialized);

        //    Plugin.Log.Info("New slot: " + newSlot);

        //    return (int)newSlot;
        //}

        private static string GetAllPluginConfigFolder()
        {
            string polaroidConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
            string rootPath = polaroidConfigDirectory.Substring(0, polaroidConfigDirectory.LastIndexOf(Path.DirectorySeparatorChar));
            
            return rootPath;
        }

        private static bool DoesHoldSignModExist()
        {
            return Directory.Exists(GetModRootPath());
        }

        private static bool IsModAdapated()
        {
            return Directory.Exists(GetTextureFolder());
        }

        private static async Task AdaptDefault_Json()
        {
            Plugin.Log.Info("Adapting default_mod.json");
            string path = Path.Combine(GetModRootPath(), "default_mod.json");
            var textJson = await File.ReadAllTextAsync(path);
            var file = Json.Deserialize<DefaultModDefinitionFile>(textJson);
            try
            {            
                file.Files.Add(@"Sign/photograph/vfx/signmod_df.atex", @"sign\\photograph\\vfx\\signmod_df.atex");
            }
            catch (ArgumentException)
            {
                Plugin.Log.Warning($"Entry already existed in {path}. Ignoring the file.");
            }
            await File.WriteAllTextAsync(path, Json.Serialize(file));
        }

        private static async Task Add_Photograph_Option()
        {
            Plugin.Log.Info("Adapting group_002_sign.json");
            string path = Path.Combine(GetModRootPath(), "group_002_sign.json");
            var textJson = await File.ReadAllTextAsync(path);
            var file = Json.Deserialize<ModOptionsFile>(textJson);
            var newOption = new OptionItem()
            {
                Name = "/photograph",
                Description = "Show the latest /photograph in the sign. You might need to use the \"hold sign\" emote again for the new picture to be loaded for others",
                Files = new() { { "vfx/signmod_df.atex", "Sign\\photograph\\vfx\\photograph_capture_0.atex" } },
                FileSwaps = new(),
                Manipulations = new()
            };
            try
            {
                file.Options.Add(newOption);
            }
            catch (ArgumentException)
            {
                Plugin.Log.Warning($"Entry already existed in {path}. Ignoring the file.");
            }
            await File.WriteAllTextAsync(path, Json.Serialize(file));
        }

        private static void Generate_Directories()
        {
            Plugin.Log.Info("Generating directories group_002_sign.json");
            Directory.CreateDirectory(Path.Combine(GetTextureFolder(), "Photos"));
        }
    }
}
