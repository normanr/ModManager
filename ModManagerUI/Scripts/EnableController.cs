using Modio.Models;
using ModManager.AddonSystem;
using ModManager.PlayerPrefsSystem;
using UnityEngine;

namespace ModManagerUI
{
    public abstract class EnableController
    {
        public static bool AllowedToChangeState(ModManagerManifest modManagerManifest)
        {
            if (ModHelper.IsModManager(modManagerManifest))
                return false;
            return true;
        }
        
        public static bool AllowedToChangeState(Mod mod)
        {
            if (ModHelper.IsModManager(mod))
                return false;
            return true;
        }
        
        public static void ChangeState(ModManagerManifest modManagerManifest, bool newState)
        {
            if (AllowedToChangeState(modManagerManifest))
            {
                ChangeState(modManagerManifest.ResourceId, newState);
            }
            else
            {
                Debug.LogWarning($"Changing state of {modManagerManifest.ModName} is not allowed.");
            }
        }
        
        public static void ChangeState(Mod mod, bool newState)
        {
            if (AllowedToChangeState(mod))
            {
                ChangeState(mod.Id, newState);
            }
            else
            {
                Debug.LogWarning($"Changing state of {mod.Name} is not allowed.");
            }
        }

        private static void ChangeState(uint modId, bool newState)
        {
            var modCard = ModCardRegistry.Get(modId);
            modCard?.ModActionStarted();
            try
            {
                if (PlayerPrefsHelper.TrySetEnabled(modId, newState))
                {
                    ModManagerPanel.ModsWereChanged = true;
                }
            }
            catch (AddonException ex)
            {
                Debug.LogWarning(ex.Message);
            }
            modCard?.ModActionStopped();
        }
    }
}