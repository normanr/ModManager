using System;
using System.Threading;
using System.Threading.Tasks;
using Modio.Models;

namespace ModManager.ExtractorSystem
{
    public interface IAddonExtractor
    {
        Task<string?> Extract(string addonZipLocation, Mod modInfo, CancellationToken cancellationToken, Action<float> progress);
    }
}
