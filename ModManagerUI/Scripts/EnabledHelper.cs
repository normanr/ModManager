using System.Collections.Generic;
using ModManager.AddonSystem;
using Timberborn.Modding;
using UnityEngine;
using Mod = Modio.Models.Mod;

namespace ModManagerUI
{
    public abstract class EnabledHelper
    {
        public static bool IsEnabled(ModManagerManifest modManagerManifest)
        {
            return ModHelper.TryLoadMod(modManagerManifest, out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
        }
        
        public static bool IsEnabled(Mod mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
            {
                return ModHelper.TryLoadMod(modManagerManifest, out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
            }

            return false;
        }
        
        public static bool CanBeEnabledOrDisabled(Mod mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
            {
                return ModHelper.TryLoadMod(modManagerManifest, out _);
            }

            return false;
        }

        public static KeyValuePair<string, int>? GetEnabledState(ModManagerManifest modManagerManifest)
        {
            if (ModHelper.TryLoadMod(modManagerManifest, out var timberbornMod))
            {
                string modEnabledKey = ModPlayerPrefsHelper.GetModEnabledKey(timberbornMod);
                if (PlayerPrefs.HasKey(modEnabledKey))
                {
                    return new KeyValuePair<string, int>(modEnabledKey, PlayerPrefs.GetInt(modEnabledKey));
                }
            }
            return null;
        }
    }
}