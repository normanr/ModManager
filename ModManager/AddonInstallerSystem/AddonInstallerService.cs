using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModManager.AddonSystem;
using Timberborn.Modding;
using Mod = Modio.Models.Mod;

namespace ModManager.AddonInstallerSystem
{
    public class AddonInstallerService
    {
        private readonly ModRepository _modRepository;
        
        private readonly IEnumerable<IAddonInstaller> _addonInstallers;

        public AddonInstallerService(ModRepository modRepository, IEnumerable<IAddonInstaller> addonInstallers)
        {
            _modRepository = modRepository;
            _addonInstallers = addonInstallers;
        }

        public async Task Install(Mod mod, string zipLocation, CancellationToken cancellationToken, Action<float> progress)
        {
            foreach (var installer in _addonInstallers)
            {
                if (await installer.Install(mod, zipLocation, cancellationToken, progress))
                {
                    _modRepository.Load();
                    
                    return;
                }
            }

            throw new AddonInstallerException($"{mod.Name} could not be installed by any installer");
        }

        public async Task Uninstall(ModManagerManifest modManagerManifest)
        {
            foreach (var installer in _addonInstallers)
            {
                if (await installer.Uninstall(modManagerManifest))
                {
                    _modRepository.Load();
                    
                    return;
                }
            }

            throw new AddonInstallerException($"{modManagerManifest.ModName} could not be uninstalled by any installer");
        }

        public async Task ChangeVersion(Mod mod, string zipLocation, CancellationToken cancellationToken, Action<float> progress)
        {
            foreach (var installer in _addonInstallers)
            {
                if (await installer.ChangeVersion(mod, zipLocation, cancellationToken, progress))
                {
                    _modRepository.Load();
                    
                    return;
                }
            }

            throw new AddonInstallerException($"The version of {mod.Name} could not be changed by any installer");
        }
    }
}