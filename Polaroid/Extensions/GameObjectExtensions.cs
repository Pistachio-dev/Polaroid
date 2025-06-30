using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Polaroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructsObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace Polaroid.Extensions
{
    public static class GameObjectExtensions
    {
        public static string GetFriendlyName(this IGameObject go)
        {
            switch (go.ObjectKind)
            {
                case ObjectKind.Ornament:
                    return $"Ornament ({go.ObjectIndex})";
                case ObjectKind.MountType:
                    return $"Mount ({go.ObjectIndex})";
                default:
                    return $"{go.Name} ({go.ObjectIndex})";
            }
        }
        public unsafe static StructsObject* Native(this IGameObject go)
        {
            return (StructsObject*)go.Address;
        }

        public unsafe static float GetHeight(this IGameObject go)
        {
            return go.Native()->Height;
        }

        public unsafe static Sex GetSex(this IGameObject go)
        {
            return go.Native()->Sex == byte.MinValue ? Sex.Male : Sex.Female; // Taking a stab in the dark as to which is which
        }
    }
}
