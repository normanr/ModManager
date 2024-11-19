using Modio.Models;
using Newtonsoft.Json;

namespace ModManager.AddonSystem
{
    public class ModManagerManifest
    {
        public const string FileName = "mod_manager_manifest.json";

        [JsonConstructor]
        public ModManagerManifest()
        {
        }
        
        public ModManagerManifest(string installLocation, Mod mod, File file)
        {
            RootPath = installLocation;
            // FileId = file.Id;
            ResourceId = mod.Id;
            Version = file.Version!;
        }
        
        public ModManagerManifest(
            string installLocation, 
            Mod mod, 
            string version)
        {
            RootPath = installLocation;
            // FileId = file.Id;
            ResourceId = mod.Id;
            Version = version;
        }

        [JsonIgnore]
        public string RootPath { get; set; } = null!;

        // public uint FileId { get; set; }
        
        public uint ResourceId { get; set; }
        
        [JsonProperty(Required = Required.AllowNull)]
        public string? ModName { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string? Version { get; set; }
    }
}
