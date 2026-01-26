using System.Threading.Tasks;
using Modio.Models;
using ModManager;
using ModManager.AddonSystem;
using ModManager.ModIoSystem;
using ModManagerUI.EventSystem;
using File = Modio.Models.File;

namespace ModManagerUI
{
    public class InstallController : Singleton<InstallController>
    {
        private static readonly AddonService? AddonService = AddonService.Instance;
        
        public static async Task DownloadAndExtract(Mod mod, File? file)
        {
            var modCard = ModCardRegistry.Get(mod);
            modCard?.ModActionStarted();
            var liveModfile = mod.Modfile;
            try
            {
                if (file != null) mod.Modfile = file;
                var downloadedMod = await AddonService!.Download(mod, (progress) =>
                {
                    EventBus.Instance.PostEvent(new ModDownloadProgressEvent(mod.Id, progress));
                });
                TryInstall(downloadedMod);
            }
            finally
            {
                mod.Modfile = liveModfile;
                modCard?.ModActionStopped();
            }
        }
        
        public static async Task DownloadAndExtractWithDependencies(Mod mod)
        {
            await foreach (var dependency in AddonService!.GetDependencies(mod))
            {
                if (dependency.IsInstalled())
                    continue;
                var dependencyFile = await AddonService.TryGetCompatibleVersion(dependency.ModId, ModManagerPanel.CheckForHighestInsteadOfLive);
                var dependencyMod = ModIoModRegistry.Get(dependency.ModId);
                await DownloadAndExtract(dependencyMod, dependencyFile);
            }
            var file = await AddonService.TryGetCompatibleVersion(mod.Id, ModManagerPanel.CheckForHighestInsteadOfLive);
            await DownloadAndExtract(mod, file);
        }
        
        public static void Uninstall(Mod mod)
        {
            var modCard = ModCardRegistry.Get(mod);
            modCard?.ModActionStarted();
            
            try
            {
                AddonService!.Uninstall(mod.Id);
            }
            finally
            {
                modCard?.ModActionStopped();
            }
        }
        
        private static void TryInstall((string location, Mod Mod) mod)
        {
            if (InstalledAddonRepository.Instance.TryGet(mod.Mod.Id, out var manifest) && manifest.Version != mod.Mod.Modfile!.Version)
            {
                AddonService!.ChangeVersion(mod.Mod, mod.location);
            }
            else
            {
                AddonService!.Install(mod.Mod, mod.location);
            }
        }
    }
}