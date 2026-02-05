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
            var lazy = ModCache.GetOrAdd(modId, () => new Lazy<Task<Mod>>(() => RetrieveMod(modId)));
            try
            {
                return await lazy.Value;
            }
            catch
            {
                ModCache.TryUpdate(modId, new Lazy<Task<Mod>>(() => RetrieveMod(modId)), lazy);
                throw;
            }
        }
        
        public static async Task<Mod> Get(Dependency dependency)
        {
            return await Get(dependency.ModId);
        }

        private static async Task<Mod> RetrieveMod(uint modId)
        {
            return await ModIo.GameClient.Mods[modId].Get();
        }
    }
}