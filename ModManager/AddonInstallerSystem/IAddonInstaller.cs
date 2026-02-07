using System;
using System.Threading;
using System.Threading.Tasks;
using Modio.Models;
using ModManager.AddonSystem;

namespace ModManager.AddonInstallerSystem
{
    public interface IAddonInstaller
    {
        public Task<bool> Install(Mod mod, string zipLocation, CancellationToken cancellationToken, Action<float> progress);

        public Task<bool> Uninstall(ModManagerManifest modManagerManifest);

        public Task<bool> ChangeVersion(Mod mod, string zipLocation, CancellationToken cancellationToken, Action<float> progress);
    }
}