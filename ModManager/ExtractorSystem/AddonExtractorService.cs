using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Modio.Models;

namespace ModManager.ExtractorSystem
{
    public class AddonExtractorService
    {
        private readonly IEnumerable<IAddonExtractor> _addonInstallers;

        public AddonExtractorService(IEnumerable<IAddonExtractor> addonInstallers)
        {
            _addonInstallers = addonInstallers;
        }

        public async Task<string> Extract(Mod addonInfo, string addonZipLocation, CancellationToken cancellationToken, Action<float> progress)
        {
            foreach (var extractor in _addonInstallers)
            {
                var extractLocation = await extractor.Extract(addonZipLocation, addonInfo, cancellationToken, progress);
                if (!string.IsNullOrEmpty(extractLocation))
                {
                    return extractLocation;
                }
            }

            throw new AddonExtractorException($"{addonInfo.Name} could not be installed by any extractor.");
        }
    }
}