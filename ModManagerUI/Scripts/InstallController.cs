using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Modio.Models;
using ModManager;
using ModManager.AddonSystem;
using ModManager.ModIoSystem;
using ModManagerUI.EventSystem;
using UnityEngine;
using File = Modio.Models.File;

namespace ModManagerUI
{
    public class InstallController : Singleton<InstallController>
    {
        private static readonly AddonService? AddonService = AddonService.Instance;

        private static readonly Dictionary<uint, CancellationTokenSource> pendingDownloads = new();
        
        public static async Task<bool> DownloadAndExtract(Mod mod, File? file)
        {
            if (pendingDownloads.TryGetValue(mod.Id, out var pendingSource))
            {
                pendingSource.Cancel();
                return false;
            }
            using var source = new CancellationTokenSource();
            pendingDownloads.Add(mod.Id, source);
            var modCard = ModCardRegistry.Get(mod);
            modCard?.ModActionStarted();
            try
            {
                if (file != null) {
                    mod = JsonSerializer.Deserialize<Mod>(JsonSerializer.Serialize(mod))!;
                    mod.Modfile = file;
                }
                var downloadedMod = await AddonService!.Download(mod, source.Token, (progress) =>
                {
                    EventBus.Instance.PostEvent(new ModDownloadProgressEvent(mod.Id, progress));
                });
                TryInstall(downloadedMod);
                return true;
            }
            catch (Exception)
            {
                if (source.IsCancellationRequested)
                {
                    Debug.Log($"Install of {mod?.Name} was canceled");
                    return false;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                pendingDownloads.Remove(mod.Id);
                modCard?.ModActionStopped();
            }
        }
        
        public static async Task DownloadAndExtractWithDependencies(Mod mod)
        {
            // TODO: This actually needs a full dependency tracking hierachy
            // to support downloading of multiple mods simulatenously and
            // to correctly cancel download requests of dependencies.
            // TODO: Change _downloadButton texts to Queued and back
            await foreach (var dependency in AddonService!.GetDependencies(mod))
            {
                if (dependency.IsInstalled())
                    continue;
                var dependencyFile = await AddonService.TryGetCompatibleVersion(dependency.ModId, ModManagerPanel.CheckForHighestInsteadOfLive);
                var dependencyMod = await ModIoModRegistry.Get(dependency.ModId);
                if (!await DownloadAndExtract(dependencyMod, dependencyFile))
                {
                    return;
                }
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