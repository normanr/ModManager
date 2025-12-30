using System.IO;
using ModManager.AddonSystem;
using ModManagerUI.StaticInstanceSystem;
using Timberborn.Modding;
using Timberborn.Versioning;
using Mod = Modio.Models.Mod;
using TimberbornMod = Timberborn.Modding.Mod;

namespace ModManagerUI
{
    public abstract class ModHelper
    {
        public static uint ModManagerUintId => 2541476;
        
        public static string ModManagerStringId => "ModManager";

        public static bool IsModManager(Mod mod)
        {
            return mod.Id == ModManagerUintId;
        }
        
        public static bool IsModManager(ModManagerManifest modManagerManifest)
        {
            return modManagerManifest.ResourceId == ModManagerUintId;
        }

        public static bool TryLoadMod(ModManagerManifest modManagerManifest, out TimberbornMod timberbornMod)
        {
            var modDirectory = new ModDirectory(new DirectoryInfo(modManagerManifest.RootPath), true, "Local", GameVersions.CurrentVersion, false);
            if (ModRepository.TryGetModDirectory(modDirectory, out var versionedModDirectory))
            {
                modDirectory = versionedModDirectory;
            }
            return StaticInstanceLoader.ModLoader.TryLoadMod(modDirectory, out timberbornMod);
        }
    }
}