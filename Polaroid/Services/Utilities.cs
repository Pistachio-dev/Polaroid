using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Polaroid.Extensions;
using Polaroid;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Polaroid.Services
{
    public static class Utilities
    {
        public static void DumpGameObject(IGameObject? go)
        {
            if (go == null)
            {
                Plugin.Log.Info("Target is null");
                return;
            }
            
            Plugin.Log.Info($"{go.Name} P:{go.Position} R:{go.Rotation} H:{go.GetHeight()}" +
                $"\nHbR:{go.HitboxRadius} Sex:{go.GetSex()} Kind: {go.ObjectKind.ToString()} Subkind:{go.SubKind}");
        }

        public static bool IsFileInUse(string fullRoute)
        {
            FileInfo fi = new FileInfo(fullRoute);
            try
            {
                using(FileStream stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }
    }
}
