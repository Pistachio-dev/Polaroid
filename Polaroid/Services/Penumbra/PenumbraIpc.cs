using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Penumbra
{
    public class PenumbraIpc
    {
        ICallGateSubscriber<string, string, int> ReloadModSubscriber;

        public PenumbraIpc(IDalamudPluginInterface pluginInterface)
        {
            ReloadModSubscriber = pluginInterface.GetIpcSubscriber<string, string, int>($"Penumbra.ReloadMod.V5");
        }

        public void ReloadMod(string modFolder, string modName)
        {
            ReloadModSubscriber.InvokeFunc(modFolder, modName);
        }
    }
}
