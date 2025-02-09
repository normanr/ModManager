using System.IO;
using ModManager.AddonSystem;
using ModManagerUI.StaticInstanceSystem;
using Timberborn.Modding;
using Timberborn.Versioning;
using Mod = Modio.Models.Mod;

namespace ModManagerUI
{
    public abstract class EnabledHelper
    {
        public static bool IsEnabled(ModManagerManifest modManagerManifest)
        {
            return StaticInstanceLoader.ModLoader.TryLoadMod(new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", GameVersions.CurrentVersion, false), out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
        }
        
        public static bool IsEnabled(Mod mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
            {
                return StaticInstanceLoader.ModLoader.TryLoadMod(new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", GameVersions.CurrentVersion, false), out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
            }

            return false;
        }
        
        public static bool CanBeEnabledOrDisabled(Mod mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
            {
                return StaticInstanceLoader.ModLoader.TryLoadMod(new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", GameVersions.CurrentVersion, false), out _);
            }

            return false;
        }
    }
}