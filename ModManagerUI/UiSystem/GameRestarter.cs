using System.Diagnostics;
using Timberborn.ApplicationLifetime;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ModManagerUI.UiSystem
{
    public abstract class GameRestarter
    {
        private static readonly bool ProcShell = false;
        private static readonly bool ProcWindow = false;
        private static readonly Process Proc = new();

        public static void QuitOrRestart()
        {
            Debug.LogError($"IsRestartCompatible: {SteamChecker.IsRestartCompatible()}");
            if (SteamChecker.IsRestartCompatible())
                Restart();
            else
                GameQuitter.Quit();
        }

        private static void Restart()
        {
            Proc.StartInfo.FileName = "steam.exe";
            Proc.StartInfo.Arguments = "-applaunch 1062090";
            Proc.StartInfo.UseShellExecute = ProcShell;
            Proc.StartInfo.CreateNoWindow = ProcWindow;
            Proc.Start();
            Application.Quit();
        }
    }
}