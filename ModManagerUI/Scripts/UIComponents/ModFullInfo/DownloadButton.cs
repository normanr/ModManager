using System;
using Modio.Models;
using ModManager.AddonSystem;
using ModManager.ModIoSystem;
using ModManager.VersionSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModManagerUI.UIComponents.ModFullInfo
{
    public class DownloadButton
    {
        private readonly Button _root;
        private readonly Mod _mod;
        private readonly ModFullInfoController _modFullInfoController;

        private DownloadButton(Button root, Mod mod, ModFullInfoController modFullInfoController)
        {
            _root = root;
            _mod = mod;
            _modFullInfoController = modFullInfoController;
        }

        public static DownloadButton Create(Button root, Mod mod, ModFullInfoController modFullInfoController)
        {
            var downloadButton = new DownloadButton(root, mod, modFullInfoController);
            root.clicked += () =>
            {
                root.SetEnabled(false);
                downloadButton.Download();
            };
            downloadButton.Refresh();
            return downloadButton;
        }

        private async void Download()
        {
            try
            {
                await InstallController.DownloadAndExtract(_mod, _modFullInfoController.CurrentFile);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error occurred while installing mod: {ex.ToString().Replace(".\r\n\x00", "").Replace("\x00", "")}");
            }
            await UpdateableModRegistry.IndexUpdatableMods();
            _modFullInfoController.Refresh();
        }

        public void Refresh()
        {
            _root.text = TextGetter();
            if (_mod.Modfile == null)
            {
                _root.SetEnabled(false);
                return;
            }

            if (!_mod.IsInstalled())
            {
                _root.SetEnabled(true);
                return;
            }

            if (_modFullInfoController.CurrentFile == null)
            {
                _root.SetEnabled(true);
                return;
            }
            
            if (InstalledAddonRepository.Instance.TryGet(_mod.Id, out var manifest))
            {
                var isSameVersion = VersionComparer.IsSameVersion(manifest.Version, _modFullInfoController.CurrentFile.Version);
                _root.SetEnabled(!isSameVersion);
            }
            else
            {
                _root.SetEnabled(true);
            }
        }

        private string TextGetter()
        {
            if (ModManagerUI.ModManagerPanel.CheckForHighestInsteadOfLive)
                return ModManagerUI.ModManagerPanel.Loc.T("Mods.Download");

            if (!InstalledAddonRepository.Instance.TryGet(_mod.Id, out var manifest)) 
                return ModManagerUI.ModManagerPanel.Loc.T("Mods.Download");

            if (_mod.Modfile == null)
                return ModManagerUI.ModManagerPanel.Loc.T("Mods.Download");

            if (VersionComparer.IsSameVersion(_mod.Modfile.Version, manifest.Version))
                return ModManagerUI.ModManagerPanel.Loc.T("Mods.Download");

            return ModManagerUI.ModManagerPanel.Loc.T("Mods.Update");
        }
    }
}