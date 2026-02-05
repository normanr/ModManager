using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Modio.Models;
using Timberborn.Common;

namespace ModManager.ModIoSystem
{
    public abstract class ModIoModRegistry
    {
        private static readonly ConcurrentDictionary<uint, Lazy<Task<Mod>>> ModCache = new();

        public static async Task<Mod> Get(uint modId)
        {
            return await ModCache.GetOrAdd(modId, () => new Lazy<Task<Mod>>(() => RetrieveMod(modId))).Value;
        }
        
        public static async Task<Mod> Get(Dependency dependency)
        {
            return await ModCache.GetOrAdd(dependency.ModId, () => new Lazy<Task<Mod>>(() => RetrieveMod(dependency.ModId))).Value;
        }

        private static async Task<Mod> RetrieveMod(uint modId)
        {
            return await ModIo.GameClient.Mods[modId].Get();
        }
    }
}