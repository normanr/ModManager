using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Modio.Models;
using ModManager.ExtractorSystem;

namespace ModManager.MapSystem
{
    public class MapExtractor : IAddonExtractor
    {
        public async Task<string?> Extract(string addonZipLocation, Mod modInfo, CancellationToken cancellationToken, Action<float> progress)
        {
            if (!modInfo.Tags.Any(x => x.Name == "Map"))
            {
                return null;
            }

            using (var zipFile = ZipFile.OpenRead(addonZipLocation))
            {
                var timberFiles = zipFile.Entries
                    .Where(x => x.Name.Contains(".timber"))
                    .ToList();

                if (timberFiles.Count() == 0)
                {
                    throw new MapException("Map zip does not contain an entry for a .timber file");
                }

                var timer = Stopwatch.StartNew();
                progress(0);
                foreach (var (i, timberFile) in timberFiles.Select((f, i) => (i, f)))
                {
                    var filename = timberFile.Name.Replace(Names.Extensions.TimberbornMap, "");
                    var files = Directory.GetFiles(Paths.Maps, filename);
                    if (files.Length > 0)
                    {
                        filename += $"_{files.Length + 1}";
                    }

                    // TODO: Maybe backport ExtractToFileAsync?
                    timberFile.ExtractToFile(Path.Combine(Paths.Maps, timberFile.Name), overwrite: true);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (timer.ElapsedMilliseconds > 33)
                    {
                        progress((float)i / timberFiles.Count());
                        await Task.Yield();
                        timer.Restart();
                    }
                }
            }

            return Paths.Maps;
        }
    }
}