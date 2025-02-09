using Modio.Models;
using ModManager.ExtractorSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ModManager.AddonSystem;
using File = System.IO.File;

namespace ModManager.ModSystem
{
    public class ModExtractor : IAddonExtractor
    {
        private List<string> _foldersToIgnore = new() { "configs" };

        public bool Extract(string addonZipLocation, Mod modInfo, out string extractLocation, bool overWrite = true)
        {
            extractLocation = "";
            if (!modInfo.Tags.Any(x => x.Name == "Mod"))
                return false;

            var modFolderName = $"{modInfo.NameId}_{modInfo.Id}_{modInfo.Modfile?.Version}";
            ClearOldModFiles(modInfo, modFolderName);
            extractLocation = Path.Combine(Paths.Mods, modFolderName);
            if (!Directory.Exists(extractLocation))
                Directory.CreateDirectory(extractLocation);
            ExtractZipWithoutCommonRoot(addonZipLocation, extractLocation);
            File.Delete(addonZipLocation);
            return true;
        }

        private void ClearOldModFiles(Mod modInfo, string modFolderName)
        {
            if (TryGetExistingModFolder(modInfo, out var dirs))
            {
                var directoryInfo = new DirectoryInfo(dirs);
                if (directoryInfo.Name.Equals(modFolderName))
                {
                    return;
                }

                directoryInfo.MoveTo(Path.Combine(Paths.Mods, modFolderName));
                DeleteModFiles(modFolderName);
            }
        }

        private bool TryGetExistingModFolder(Mod modInfo, out string directoryPath)
        {
            directoryPath = null;
            try
            {
                directoryPath ??= Directory.GetDirectories(Paths.Mods, $"{modInfo.NameId}_{modInfo.Id}*").SingleOrDefault();

                if (InstalledAddonRepository.Instance.TryGet(modInfo.Id, out var modManagerManifest))
                    directoryPath ??= modManagerManifest.RootPath;
            }
            catch (InvalidOperationException ex)
            {
                throw new AddonExtractorException($"Found multiple folders for \"{modInfo.Name}\"");
            }

            if (directoryPath != null)
            {
                return true;
            }

            return false;
        }

        private void DeleteModFiles(string modFolderName)
        {
            var modDirInfo = new DirectoryInfo(Path.Combine(Paths.Mods, modFolderName));
            var modSubFolders = modDirInfo
                .GetDirectories("*", SearchOption.AllDirectories)
                .Where(folder => !_foldersToIgnore.Contains(folder.FullName.Split(Path.DirectorySeparatorChar).Last()));
            foreach (var subDirectory in modSubFolders.Reverse())
            {
                DeleteFilesFromFolder(subDirectory);
                TryDeleteFolder(subDirectory);
            }

            DeleteFilesFromFolder(modDirInfo);
            TryDeleteFolder(modDirInfo);
        }

        private void DeleteFilesFromFolder(DirectoryInfo dir)
        {
            foreach (var file in dir.GetFiles().Where(file => !file.Name.EndsWith(Names.Extensions.Remove)))
            {
                try
                {
                    file.Delete();
                }
                catch (UnauthorizedAccessException ex)
                {
                    file.MoveTo($"{file.FullName}{Names.Extensions.Remove}");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void TryDeleteFolder(DirectoryInfo dir)
        {
            if (dir.EnumerateDirectories().Any() == false && dir.EnumerateFiles().Any() == false)
            {
                dir.Delete();
            }
        }

        private static void ExtractZipWithoutCommonRoot(string zipFilePath, string extractPath)
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            
            var allPaths = archive.Entries
                .Where(e => !string.IsNullOrEmpty(e.Name))
                .Select(e => e.FullName)
                .ToList();

            var commonRoot = GetCommonRoot(allPaths);

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var relativePath = entry.FullName.StartsWith(commonRoot)
                    ? entry.FullName.Substring(commonRoot.Length).TrimStart('/')
                    : entry.FullName;

                var destinationFilePath = Path.Combine(extractPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

                entry.ExtractToFile(destinationFilePath, overwrite: true);
            }
        }

        private static string GetCommonRoot(IReadOnlyCollection<string> paths)
        {
            if (!paths.Any()) return "";

            var splitPaths = paths.Select(path => path.Split('/')).ToList();
            var minLength = splitPaths.Min(splitPath => splitPath.Length);

            var commonRoot = "";
            for (var i = 0; i < minLength; i++)
            {
                var segment = splitPaths[0][i];
                if (splitPaths.All(p => p[i] == segment))
                {
                    commonRoot = commonRoot == "" ? segment : $"{commonRoot}/{segment}";
                }
                else
                {
                    break;
                }
            }

            return commonRoot + "/";
        }
    }
}