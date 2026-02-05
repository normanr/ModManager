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
            return await ModDependenciesCache.GetOrAdd(modId, () => new Lazy<Task<IReadOnlyList<Dependency>>>(() => RetrieveMod(modId))).Value;
        }
        
        public static async Task<IReadOnlyList<Dependency>> Get(Mod mod)
        {
            return await ModDependenciesCache.GetOrAdd(mod.Id, () => new Lazy<Task<IReadOnlyList<Dependency>>>(() => RetrieveMod(mod.Id))).Value;
        }
        
        public static async Task<IReadOnlyList<Dependency>> Get(Dependency dependency)
        {
            return await ModDependenciesCache.GetOrAdd(dependency.ModId, () => new Lazy<Task<IReadOnlyList<Dependency>>>(() => RetrieveMod(dependency.ModId))).Value;
        }

        private static async Task<IReadOnlyList<Dependency>> RetrieveMod(uint modId)
        {
            return await ModIo.ModsClient[modId].Dependencies.Get();
        }
    }
}