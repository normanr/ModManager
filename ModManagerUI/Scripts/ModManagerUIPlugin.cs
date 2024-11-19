using System.IO;
using ModManager.StartupSystem;
using Timberborn.Modding;
using Timberborn.ModManagerScene;
using Timberborn.PlatformUtilities;
using UnityEngine;

namespace ModManagerUI
{
    public class ModManagerUIPlugin : IModStarter
    {
        public void StartMod(IModEnvironment modEnvironment)
        {
            ModManagerStartup.Run("MOD_IO_APIKEY", options =>
            {
                options.GameId = 3659;
                options.GamePath = Application.dataPath;
                options.IsGameRunning = true;
                options.ModInstallationPath = Path.Combine(UserDataFolder.Folder, UserFolderModsProvider.ModsDirectoryName);
                options.ModIoGameUrl = "https://mod.io/g/timberborn";
                options.ModManagerPath = modEnvironment.ModPath;
                options.Logger = new ModManagerLogger();
            });
        }
    }
}
