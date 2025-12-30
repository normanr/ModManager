using ModManager.AddonSystem;
using Timberborn.Modding;
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
    }
}