using Modio.Models;
using ModManager.AddonSystem;

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
    }
}