using Modio.Models;
using ModManager.ExtractorSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ModManager.StartupSystem;
using System.Threading;
using System.Threading.Tasks;

namespace ModManager.ModManagerSystem
{
    public class ModManagerExtractor : Singleton<ModManagerExtractor>, IAddonExtractor, ILoadable
    {
        private string? _modManagerFolderPath;
        private const string _modManagerPackageName = "Mod Manager";
        private List<string> _foldersToIgnore = new() { "temp" };
        
        public void Load(ModManagerStartupOptions startupOptions)
        {
            _modManagerFolderPath = startupOptions.ModManagerPath;
        }
        
        public async Task<string?> Extract(string addonZipLocation, Mod modInfo, CancellationToken cancellationToken, Action<float> progress)
        {
            if (modInfo.Name != _modManagerPackageName)
            {
                return null;
            }
            ClearOldModFiles(_modManagerFolderPath!);
            // TODO: Maybe backport ExtractToDirectoryAsync?
            ZipFile.ExtractToDirectory(addonZipLocation, Paths.Mods, overwriteFiles: true);

            return _modManagerFolderPath;
        }
        
        private void ClearOldModFiles(string modFolderName)
        { 
            DeleteModFiles(modFolderName);
        }

        private void DeleteModFiles(string modFolderName)
        {
            var modDirInfo = new DirectoryInfo(Path.Combine(Paths.Mods, modFolderName));
            var modSubFolders = 
                modDirInfo.GetDirectories("*", SearchOption.AllDirectories)
                          .Where(folder => !_foldersToIgnore.Contains(folder.FullName
                                                            .Split(Path.DirectorySeparatorChar)
                                                            .Last()));
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
                    if(!file.Name.EndsWith(".dll"))
                    {
                        continue;
                    }
                    file.Delete();
                }
                catch (UnauthorizedAccessException)
                {
                    file.MoveTo($"{file.FullName}{Names.Extensions.Remove}");
                }
                catch (IOException)
                {
                    try
                    {
                        file.MoveTo($"{file.FullName}{Names.Extensions.Remove}");
                    }
                    catch(IOException)
                    {
                        throw;
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void TryDeleteFolder(DirectoryInfo dir)
        {
            try
            {
                if (dir.EnumerateDirectories().Any() == false && dir.EnumerateFiles().Any() == false)
                {
                    dir.Delete();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
