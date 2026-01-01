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
        private static bool TryLoadMod(ModManagerManifest modManagerManifest, out TimberbornMod? timberbornMod)
        {
            if (modManagerManifest.RootPath == Paths.Maps)
            {
                timberbornMod = null;
                return false;
            }
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

        public static bool CanBeEnabledOrDisabled(ModManagerManifest modManagerManifest)
        {
            if (modManagerManifest.RootPath == Paths.Maps)
                return false;

            return TryLoadMod(modManagerManifest, out _);
        }

        public static bool CanBeEnabledOrDisabled(Mod mod)
        {
            if (!InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
                return false;

            return CanBeEnabledOrDisabled(modManagerManifest);
        }

        public static ModPlayerPrefs? GetPlayerPrefs(uint modId)
        {
            if (!InstalledAddonRepository.Instance.TryGet(modId, out var modManagerManifest))
                return null;
            if (!TryLoadMod(modManagerManifest, out var timberbornMod))
                return null;
            var modPrefs = new ModPlayerPrefs();
            string modEnabledKey = ModPlayerPrefsHelper.GetModEnabledKey(timberbornMod);
            if (PlayerPrefs.HasKey(modEnabledKey))
            {
                modPrefs.EnabledKey = modEnabledKey;
                modPrefs.Enabled = PlayerPrefs.GetInt(modEnabledKey);
            }
            string modPriorityKey = ModPlayerPrefsHelper.GetModPriorityKey(timberbornMod);
            if (PlayerPrefs.HasKey(modPriorityKey))
            {
                modPrefs.PriorityKey = modPriorityKey;
                modPrefs.Priority = PlayerPrefs.GetInt(modPriorityKey);
            }
            return modPrefs;
        }

        public static void RestorePlayerPrefs(ModManagerManifest modManagerManifest, ModPlayerPrefs? previousPrefs)
        {
            if (previousPrefs == null)
                return;
            if (!TryLoadMod(modManagerManifest, out var timberbornMod))
                return;
            string modEnabledKey = ModPlayerPrefsHelper.GetModEnabledKey(timberbornMod);
            if (previousPrefs.EnabledKey != null && previousPrefs.EnabledKey != modEnabledKey)
            {
                PlayerPrefs.SetInt(modEnabledKey, previousPrefs.Enabled);
                PlayerPrefs.DeleteKey(previousPrefs.EnabledKey);
            }
            string modPriorityKey = ModPlayerPrefsHelper.GetModPriorityKey(timberbornMod);
            if (previousPrefs.PriorityKey != null && previousPrefs.PriorityKey != modPriorityKey)
            {
                PlayerPrefs.SetInt(modPriorityKey, previousPrefs.Priority);
                PlayerPrefs.DeleteKey(previousPrefs.PriorityKey);
            }
        }
    }
}