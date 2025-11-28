using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Polaroid;
using System;
using System.Linq;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.AchievementListModule.Delegates;


// Taken from https://github.com/MgAl2O4/PatMeDalamud and modified by me as needed
namespace Polaroid.Services.EmoteDetection
{    
    public class EmoteReaderHooks : IDisposable
    {
        public const int PhotographEmoteId = 288;
        public const int U_PhotograhEmoteId = 290;
        public const int PhotographScreenshotDelayMs = 100;//6280;

        public const int VisageEmoteId = 286;

        public Action<IPlayerCharacter, ushort>? OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate>? hookEmote;

        public bool IsValid = false;

        public EmoteReaderHooks()
        {
            try
            {
                hookEmote = Plugin.SigScanner.HookFromSignature<OnEmoteFuncDelegate>("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 4C 89 74 24", OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex, "failed to hook emotes!");
            }
        }

        public void Dispose()
        {
            hookEmote?.Dispose();
            IsValid = false;
        }

        void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2)
        {
            TakePictureIfConditionsMet(instigatorAddr, emoteId, targetId);
            hookEmote?.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }



        private void TakePictureIfConditionsMet(ulong instigatorAddr, ushort emoteId, ulong targetId)
        {
            if (Plugin.ClientState.LocalPlayer == null)
            {
                return;
            }

            // Verify it is the current player emoting
            if (instigatorAddr != (ulong)(Plugin.ClientState.LocalPlayer?.Address ?? 0))
            {
                return;
            }

            Plugin.Log.Warning($"Emote detected with id: {emoteId}");
            if (emoteId == PhotographEmoteId || emoteId == U_PhotograhEmoteId)
            {
                Orchestrator.OnPhotographEmote(Plugin.ClientState.LocalPlayer!);
            }            
        }
    }
}
