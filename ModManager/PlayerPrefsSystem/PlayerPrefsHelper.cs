using System.Collections.Generic;
using System.IO;
using ModManager.AddonSystem;
using ModManager.StaticInstanceSystem;
using Timberborn.Modding;
using Timberborn.Versioning;
using UnityEngine;
using Mod = Modio.Models.Mod;
using TimberbornMod = Timberborn.Modding.Mod;

namespace ModManager.PlayerPrefsSystem
{
    public static class PlayerPrefsHelper
    {
        private static bool TryLoadMod(ModManagerManifest modManagerManifest, out TimberbornMod timberbornMod)
        {
            var modDirectory = new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", GameVersions.CurrentVersion, false);
            if (ModRepository.TryGetModDirectory(modDirectory, out var versionedModDirectory))
            {
                modDirectory = versionedModDirectory;
            }
            return StaticInstanceLoader.ModLoader.TryLoadMod(modDirectory, out timberbornMod);
        }

        public static bool IsEnabled(ModManagerManifest modManagerManifest)
        {
            return TryLoadMod(modManagerManifest, out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
        }

        public static bool IsEnabled(Mod mod)
        {
            if (!InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
                return false;

            return IsEnabled(modManagerManifest);
        }

        public static bool TrySetEnabled(uint modId, bool enabled)
        {
            if (!InstalledAddonRepository.Instance.TryGet(modId, out var modManagerManifest))
                return false;
            if (!TryLoadMod(modManagerManifest, out var timberbornMod))
                return false;

            ModPlayerPrefsHelper.ToggleMod(enabled, timberbornMod);
            return true;
        }

        public static bool CanBeEnabledOrDisabled(Mod mod)
        {
            if (!InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
                return false;

            return TryLoadMod(modManagerManifest, out _);
        }

        public static KeyValuePair<string, int>? GetPlayerPrefs(uint modId)
        {
            if (!InstalledAddonRepository.Instance.TryGet(modId, out var modManagerManifest))
                return null;
            if (!TryLoadMod(modManagerManifest, out var timberbornMod))
                return null;
            string modEnabledKey = ModPlayerPrefsHelper.GetModEnabledKey(timberbornMod);
            if (PlayerPrefs.HasKey(modEnabledKey))
            {
                return new KeyValuePair<string, int>(modEnabledKey, PlayerPrefs.GetInt(modEnabledKey));
            }
            return null;
        }

        public static void RestorePlayerPrefs(ModManagerManifest modManagerManifest, KeyValuePair<string, int>? previousPrefs)
        {
            if (previousPrefs == null)
                return;
            if (!TryLoadMod(modManagerManifest, out var timberbornMod))
                return;
            string modEnabledKey = ModPlayerPrefsHelper.GetModEnabledKey(timberbornMod);
            PlayerPrefs.SetInt(modEnabledKey, previousPrefs.Value.Value);
            if (previousPrefs.Value.Key != modEnabledKey)
                PlayerPrefs.DeleteKey(previousPrefs.Value.Key);
        }
    }
}