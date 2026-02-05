using System.IO;
using System.Threading.Tasks;
using ModManager.AddonSystem;
using ModManager.ModIoSystem;
using ModManager.PersistenceSystem;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ModManagerUI
{
    public class ModManagerRegisterer : ILoadableSingleton
    {
        private readonly PersistenceService _persistenceService = PersistenceService.Instance;

        private readonly ModRepository _modRepository;
            
        public ModManagerRegisterer(ModRepository modRepository)
        {
            _modRepository = modRepository;
        }
        
        public void Load()
        {
            Debug.LogWarning($"InstalledAddonRepository.Instance.TryGet(ModHelper.ModManagerUintId, out _) {InstalledAddonRepository.Instance.TryGet(ModHelper.ModManagerUintId, out _)}");
            if (InstalledAddonRepository.Instance.TryGet(ModHelper.ModManagerUintId, out _))
                return;
            
            foreach (var mod in _modRepository.Mods)
            {
                if (mod.Manifest.Id != ModHelper.ModManagerStringId || !mod.IsEnabled)
                    continue;
                var modIoMod = Task.Run(() => ModIoModRegistry.Get(ModHelper.ModManagerUintId)).Result;
                var modManagerManifest = new ModManagerManifest(mod.ModDirectory.OriginPath, modIoMod, mod.Manifest.Version.Numeric);
                var modManifestPath = Path.Combine(mod.ModDirectory.OriginPath, ModManagerManifest.FileName);
                _persistenceService.SaveObject(modManagerManifest, modManifestPath);
                InstalledAddonRepository.Instance.Add(modManagerManifest);
            }
        }
    }
}