using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modio.Filters;
using Modio.Models;
using Timberborn.Common;

namespace ModManager.ModIoSystem
{
    public abstract class ModIoModFilesRegistry
    {
        private static readonly ConcurrentDictionary<uint, Lazy<Task<IEnumerable<File>>>> ModIoModFiles = new();

        public static async Task<IEnumerable<File>> Get(uint modId)
        {
            return await ModIoModFiles.GetOrAdd(modId, () => new Lazy<Task<IEnumerable<File>>>(() => RetrieveFiles(modId))).Value;
        }

        public static async Task<IEnumerable<File>> GetDesc(uint modId)
        {
            var files = await ModIoModFiles.GetOrAdd(modId, () => new Lazy<Task<IEnumerable<File>>>(() => RetrieveFiles(modId))).Value;
            return files.OrderByDescending(file => file.Id).ToList().AsReadOnly();
        }

        private static async Task<IEnumerable<File>> RetrieveFiles(uint modId)
        {
            return await ModIo.ModsClient[modId].Files.Search(FileFilter.Id.Desc()).ToList();
        }
    }
}