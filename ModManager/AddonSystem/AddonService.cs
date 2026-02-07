using Modio.Models;
using ModManager.ModIoSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModManager.AddonInstallerSystem;
using ModManager.VersionSystem;
using File = Modio.Models.File;
using Mod = Modio.Models.Mod;
using Mono.Security.Cryptography;
using System.Security.Cryptography;
using System.Threading;

namespace ModManager.AddonSystem
{
    public class AddonService
    {
        public static AddonService? Instance;

        private readonly InstalledAddonRepository _installedAddonRepository;
        private readonly AddonInstallerService _addonInstallerService;

        private readonly Dictionary<Uri, byte[]> _imageCache = new();

        public AddonService(InstalledAddonRepository installedAddonRepository, AddonInstallerService addonInstallerService)
        {
            Instance = this;
            _installedAddonRepository = installedAddonRepository;
            _addonInstallerService = addonInstallerService;
        }

        public void Install(Mod mod, string zipLocation)
        {
            if (mod.IsInstalled())
            {
                throw new AddonException($"{mod.Name} is already installed. Use method `{nameof(ChangeVersion)}` to change the version of an installed mod.");
            }

            _addonInstallerService.Install(mod, zipLocation);
        }

        public void Uninstall(uint modId)
        {
            if (!_installedAddonRepository.TryGet(modId, out var manifest))
            {
                throw new AddonException($"Cannot uninstall modId: {modId}. Mod is not installed.");
            }

            _addonInstallerService.Uninstall(manifest);
        }

        public void ChangeVersion(Mod mod, string zipLocation)
        {
            if (!mod.IsInstalled())
            {
                throw new AddonException($"Cannot change version of {mod.Name}. Mod is not installed.");
            }
            if (_installedAddonRepository.Get(mod.Id).Version == mod.Modfile!.Version)
            {
                throw new AddonException($"{mod.Name} is already installed with version {mod.Modfile!.Version}.");
            }

            _addonInstallerService.ChangeVersion(mod, zipLocation);
        }
        
        public IAsyncEnumerable<Dependency> GetDependencies(Mod mod)
        {
            var list = new List<Mod>();

            return GetUniqueDependencies(mod, list);
        }
        
        private static async IAsyncEnumerable<Dependency> GetUniqueDependencies(Mod mod, List<Mod> list)
        {
            list.Add(mod);
            
            var dependencies = await ModIoModDependenciesRegistry.Get(mod);

            foreach (var dependency in dependencies)
            {
                yield return dependency;
                
                var dependencyMod = await ModIoModRegistry.Get(dependency);
                if (list.Contains(dependencyMod))
                    continue;
                
                await foreach (var dep2 in GetUniqueDependencies(dependencyMod, list))
                {
                    yield return dep2;
                }
            }
        }

        public async Task<string> Download(Mod mod, CancellationToken cancellationToken, Action<float> progress)
        {
            if (mod.IsInstalled())
            {
                if (!_installedAddonRepository.TryGet(mod.Id, out var manifest))
                {
                    throw new AddonException($"Couldn't find installed mod'd manifest.");
                }
                if (manifest.Version == mod.Modfile!.Version)
                {
                    throw new AddonException($"Mod {mod.Name} is already installed with version {mod.Modfile!.Version}.");
                }
            }

            progress(0);
            Directory.CreateDirectory($"{Paths.ModManager.Temp}");
            var tempZipLocation = Path.Combine(Paths.ModManager.Temp, $"{mod.Id}_{mod.Modfile!.Version}.zip");

            var length = mod.Modfile!.FileSize;
            var position = 0;
            var streamProgress = new Progress<int>((count) =>
            {
                position += count;
                progress((float)position / length);
            });

            try
            {
                using var stream = new FileInfo(tempZipLocation).Create();
                using var md5sum = MD5.Create();
                using var cstream = new CryptoStream(stream, md5sum, CryptoStreamMode.Write);
                using var pstream = new ProgressStream.ProgressStream(cstream, writeProgress: streamProgress);

                await ModIo.Client.Download(ModIoGameInfo.GameId, mod.Id, mod.Modfile!.Id, pstream, cancellationToken);
                cstream.FlushFinalBlock();

                var wantMd5 = CryptoConvert.FromHex(mod.Modfile!.FileHash!.Md5);
                if (!CryptographicOperations.FixedTimeEquals(wantMd5, md5sum.Hash)) {
                    throw new AddonException($"Mod {mod.Name} download hash mismatch for version {mod.Modfile!.Version}.");
                }
            }
            catch
            {
                System.IO.File.Delete(tempZipLocation);
                throw;
            }

            return tempZipLocation;
        }

        public async Task<byte[]> GetImage(Uri uri)
        {
            if (_imageCache.TryGetValue(uri, out var imageBytes))
            {
                return imageBytes;
            }

            var byteArray = await ModIo.HttpClient.GetByteArrayAsync(uri);
            _imageCache[uri] = byteArray;
            return byteArray;
        }
        
        public async Task<File?> TryGetCompatibleVersion(uint modId, bool downloadHighestInsteadOfLive)
        {
            var orderedFiles = await ModIoModFilesRegistry.GetDesc(modId);
            var latestCompatibleFile = orderedFiles.FirstOrDefault(file => VersionStatusService.GetVersionStatus(file) == VersionStatus.Compatible);
            if (latestCompatibleFile != null)
            {
                return latestCompatibleFile;
            }

            if (downloadHighestInsteadOfLive)
            {
                var latestUnknownFileOrNull = orderedFiles.FirstOrDefault(file => VersionStatusService.GetVersionStatus(file) == VersionStatus.Unknown);
                return latestUnknownFileOrNull;
            }
            
            return null;
        }
    }
}