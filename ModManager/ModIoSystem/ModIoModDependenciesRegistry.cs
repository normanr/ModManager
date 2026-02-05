using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.Models;
using Timberborn.Common;

namespace ModManager.ModIoSystem
{
    public abstract class ModIoModDependenciesRegistry
    {
        private static readonly ConcurrentDictionary<uint, Lazy<Task<IReadOnlyList<Dependency>>>> ModDependenciesCache = new();

        public static async Task<IReadOnlyList<Dependency>> Get(uint modId)
        {
            var lazy = ModDependenciesCache.GetOrAdd(modId, () => new Lazy<Task<IReadOnlyList<Dependency>>>(() => RetrieveMod(modId)));
            try
            {
                return await lazy.Value;
            }
            catch
            {
                ModDependenciesCache.TryUpdate(modId, new Lazy<Task<IReadOnlyList<Dependency>>>(() => RetrieveMod(modId)), lazy);
                throw;
            }
        }
        
        public static async Task<IReadOnlyList<Dependency>> Get(Mod mod)
        {
            return await Get(mod.Id);
        }
        
        public static async Task<IReadOnlyList<Dependency>> Get(Dependency dependency)
        {
            return await Get(dependency.ModId);
        }

        private static async Task<IReadOnlyList<Dependency>> RetrieveMod(uint modId)
        {
            return await ModIo.ModsClient[modId].Dependencies.Get();
        }
    }
}