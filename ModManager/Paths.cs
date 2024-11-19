using System.IO;
using ModManager.StartupSystem;
using Timberborn.PlatformUtilities;

namespace ModManager
{
    public class Paths : Singleton<Paths>, ILoadable
    {
        public void Load(ModManagerStartupOptions startupOptions)
        {
            ModManagerRoot = startupOptions.ModManagerPath;
            GameRoot = startupOptions.GamePath;
            Mods = startupOptions.ModInstallationPath;

            EnsurePathExists(Mods);
            EnsurePathExists(ModManager.Temp);
        }

        private static void EnsurePathExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GameRoot { get; set; } = null!;

        public static string ModManagerRoot { get; set; } = null!;

        public static string Mods { get; set; } = null!;

        public static readonly string Maps = Path.Combine(UserDataFolder.Folder, "Maps");

        public static class ModManager
        {
            public static string Temp { get; set; } = Path.GetTempPath();
        }
    }
}