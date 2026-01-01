using Modio.Models;
using ModManager.AddonSystem;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModManager.MapSystem
{
    public class MapModManagerManifest : ModManagerManifest
    {
        public new const string FileName = ModManagerManifest.FileName;

        public List<string> MapFileNames { get; set; } = null!;

        [JsonConstructor]
        public MapModManagerManifest()
        {
        }
        
        public MapModManagerManifest(string installLocation, Mod mod, List<string> mapFleNames)
            : base(installLocation, mod)
        {
            MapFileNames = mapFleNames;
        }
    }
}