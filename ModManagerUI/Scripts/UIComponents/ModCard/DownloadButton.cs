using System;
using System.Linq;
using Modio.Models;
using ModManager.AddonSystem;
using ModManager.VersionSystem;
using ModManagerUI.EventSystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;
using EventBus = ModManagerUI.EventSystem.EventBus;

namespace ModManagerUI.UIComponents.ModCard
{
    public class DownloadButton
    {
        private readonly Button _root;
        private readonly Mod _mod;
        
        public DownloadButton(Button root, Mod mod)
        {
            _root = root;
            _mod = mod;
        }

        public void Initialize()
        {
            EventBus.Instance.Register(this);
            _root.clicked += OnClick;
            Refresh();
        }

        [OnEvent]
        public void OnUpdatableModsRetrieved(UpdatableModsRetrievedEvent updatableModsRetrievedEvent)
        {
            Refresh();
        }
        
        [OnEvent]
        public void OnModDownloadProgress(ModDownloadProgressEvent modDownloadProgressEvent)
        {
            if (modDownloadProgressEvent.ModId == _mod.Id)
            {
                _root.text = $"{modDownloadProgressEvent.Progress:P0}";
            }
        }

        public void Refresh()
        {
            _root.text = TextGetter();
            if (_mod.Modfile == null)
            {
                _root.SetEnabled(false);
                return;
            }

            if (InstalledAddonRepository.Instance.TryGet(_mod.Id, out var manifest))
            {
                var isSameVersion = VersionComparer.IsSameVersion(manifest.Version, _mod.Modfile.Version);
                _root.SetEnabled(!isSameVersion);
            }
            else
            {
                _root.SetEnabled(true);
            }
        }

        private async void OnClick()
        {
            try
            {
                await InstallController.DownloadAndExtractWithDependencies(_mod);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error occurred while installing {_mod?.Name}: {ex.ToString().Replace(".\r\n\x00", "").Replace("\x00", "")}");
            }
            await UpdateableModRegistry.IndexUpdatableMods();
        }

        private string TextGetter()
        {
            if (UpdateableModRegistry.UpdateAvailable == null)
                return ModManagerUI.ModManagerPanel.Loc.T("Mods.Download");
            if (UpdateableModRegistry.UpdateAvailable.Values.Any(file => file.ModId == _mod.Id))
                return ModManagerUI.ModManagerPanel.Loc.T("Mods.Update");
            return ModManagerUI.ModManagerPanel.Loc.T("Mods.Download");
        }
    }
}