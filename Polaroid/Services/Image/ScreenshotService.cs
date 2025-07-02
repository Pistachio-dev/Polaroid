using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Photo;
using FFXIVClientStructs.STD.Helper;
using ImGuizmoNET;
using InputInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.ActionEffectHandler;

namespace Polaroid.Services.Image
{
    public unsafe static class ScreenshotService // TODO: Make it inherit IDisposable
    {
        private const int ScreenshotRetries = 3;
        private const int ScreenshotRetryFrameDelay = 1;

        static unsafe ScreenshotService()
        {
        }

        public static unsafe void TakeScreenshot()
        {
            InputFaker.PressScreenshotKey();
            Plugin.Framework.RunOnTick(() => HandleOrRetryScreenshot(0), delayTicks: ScreenshotRetryFrameDelay);
        }

        private static unsafe Task HandleOrRetryScreenshot(int retryCounter)
        {
            if (retryCounter == ScreenshotRetries)
            {
                Plugin.Log.Warning("Screenshot retrieval failed!");
                return Task.CompletedTask;
            }
            retryCounter++;
            string screenshotRoute = ScreenShot.Instance()->ThreadPtr->ScreenShotStorageDirectory.ToString();
            long timeStamp = ScreenShot.Instance()->ScreenShotTimestamp;
            string fileName = GetScreenshotFileName(timeStamp);
            Log.Information($"Looking for screenshot with timestamp {timeStamp} at {screenshotRoute}");
            Log.Information($"Expected file name: \"{fileName}\"");

            return Task.CompletedTask;
        }

        private static string GetScreenshotFileName(long timestamp)
        {
            StringBuilder s = new StringBuilder();
            DateTime scTime = UnixTimeStampToLocalDateTime(timestamp);
            s.Append("ffxiv_");
            s.Append(scTime.Day.ToString("D2"));
            s.Append(scTime.Month.ToString("D2"));
            s.Append(scTime.Year.ToString("D4"));
            s.Append("_");
            s.Append(scTime.Hour.ToString("D2"));
            s.Append(scTime.Minute.ToString("D2"));
            s.Append(scTime.Second.ToString("D2"));
            s.Append("_");
            s.Append(scTime.Millisecond.ToString("D3"));
            s.Append(".");
            string extension = ScreenShot.Instance()->ScreenShotFileFormat.ToString().ToLower();
            s.Append(extension);

            return s.ToString();
        }

        private static DateTime UnixTimeStampToLocalDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).DateTime.ToLocalTime();
            return dateTime;
        }

        public static void Dispose()
        {
        }
    }
}
