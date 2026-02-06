using System;
using Modio.Models;
using ModManager.ModIoSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModManagerUI.UIComponents.ModCard
{
    public class UninstallButton
    {
        private readonly Button _root;
        private readonly Mod _mod;
        
        public UninstallButton(Button root, Mod mod)
        {
            _root = root;
            _mod = mod;
        }

        public void Initialize()
        {
            _root.clicked += OnClick; 
            Refresh();
        }

        public void Refresh()
        {
            _root.visible = IsVisible();
            _root.SetEnabled(IsEnabled());
        }
        private async void OnClick()
        {
            try
            {
                InstallController.Uninstall(_mod);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error occurred while uninstalling {_mod?.Name}: {ex.ToString().Replace(".\r\n\x00", "").Replace("\x00", "")}");
            }
            await UpdateableModRegistry.IndexUpdatableMods();
        }

        private bool IsVisible()
        {
            if (ModHelper.IsModManager(_mod))
            {
                return true;
            }
            
            return _mod.IsInstalled();
        }

        private bool IsEnabled()
        {
            if (ModHelper.IsModManager(_mod))
            {
                return false;
            }

            return true;
        }
    }
}