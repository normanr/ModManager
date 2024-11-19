using System.IO;
using ModManager.AddonSystem;
using Timberborn.Modding;
using Timberborn.Versioning;
using Mod = Modio.Models.Mod;

namespace ModManagerUI
{
    public abstract class EnabledHelper
    {
        public static bool IsEnabled(ModManagerManifest modManagerManifest)
        {
            return ModManagerPanel.ModLoader.TryLoadMod(new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", Versions.CurrentGameVersion, false), out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
        }
        
        public static bool IsEnabled(Mod mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
            {
                return ModManagerPanel.ModLoader.TryLoadMod(new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", Versions.CurrentGameVersion, false), out var timberbornMod) && ModPlayerPrefsHelper.IsModEnabled(timberbornMod);
            }

            return false;
        }
        
        public static bool CanBeEnabledOrDisabled(Mod mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Id, out var modManagerManifest))
            {
                return ModManagerPanel.ModLoader.TryLoadMod(new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", Versions.CurrentGameVersion, false), out _);
            }

            return false;
        }
    }
}