using System;
using System.Collections.Generic;
using System.IO;
using Splat;

namespace ClickOnceToSquirrelMigrator
{
    internal class RemoveFiles : IUninstallStep, IEnableLogger
    {
        private string _clickOnceDataFolder;
        private string _clickOnceFolder;
        private List<string> _filesToRemove;
        private List<string> _foldersToRemove;

        public void Dispose()
        {
        }

        public void Execute()
        {
            if (string.IsNullOrEmpty(_clickOnceFolder) || !Directory.Exists(_clickOnceFolder))
                throw new InvalidOperationException("Call Prepare() first.");

            foreach (var folder in _foldersToRemove)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            foreach (var file in _filesToRemove)
            {
                File.Delete(file);
            }
        }

        public void Prepare(List<string> componentsToRemove)
        {
            _foldersToRemove = new List<string>();

            _clickOnceFolder = FindClickOnceFolder();
            foreach (var directory in Directory.GetDirectories(_clickOnceFolder))
            {
                if (componentsToRemove.Contains(Path.GetFileName(directory)))
                {
                    _foldersToRemove.Add(directory);
                }
            }

            _clickOnceDataFolder = FindClickOnceDataFolder();
            foreach (var directory in Directory.GetDirectories(_clickOnceDataFolder))
            {
                if (componentsToRemove.Contains(Path.GetFileName(directory)))
                {
                    _foldersToRemove.Add(directory);
                }
            }

            _filesToRemove = new List<string>();
            foreach (var file in Directory.GetFiles(Path.Combine(_clickOnceFolder, "manifests")))
            {
                if (componentsToRemove.Contains(Path.GetFileNameWithoutExtension(file)))
                {
                    _filesToRemove.Add(file);
                }
            }
        }

        public void PrintDebugInformation()
        {
            if (string.IsNullOrEmpty(_clickOnceFolder) || !Directory.Exists(_clickOnceFolder))
                throw new InvalidOperationException("Call Prepare() first.");

            this.Log().Info("Remove files from " + _clickOnceFolder);
            this.Log().Info("Remove files from " + _clickOnceDataFolder);

            foreach (var folder in _foldersToRemove)
            {
                this.Log().Info("Delete folder " + folder.Substring(_clickOnceFolder.Length + 1));
            }

            foreach (var file in _filesToRemove)
            {
                this.Log().Info("Delete file " + file.Substring(_clickOnceFolder.Length + 1));
            }
        }

        private static string DescendIntoSubfolders(string baseFolder)
        {
            if (!Directory.Exists(baseFolder)) throw new ArgumentException("Could not find ClickOnce folder.");

            foreach (var subFolder in Directory.GetDirectories(baseFolder))
            {
                if ((Path.GetFileName(subFolder) ?? string.Empty).Length == 12)
                {
                    foreach (var subSubFolder in Directory.GetDirectories(subFolder))
                    {
                        if ((Path.GetFileName(subSubFolder) ?? string.Empty).Length == 12)
                        {
                            return subSubFolder;
                        }
                    }
                }
            }

            throw new ArgumentException("Could not find ClickOnce folder");
        }

        private static string GetApps20Folder()
        {
            return IsWindowsXp()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\Apps\2.0")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Apps\2.0");
        }

        private static bool IsWindowsXp()
        {
            return Environment.OSVersion.Version.Major == 5;
        }

        private string FindClickOnceDataFolder()
        {
            return DescendIntoSubfolders(Path.Combine(GetApps20Folder(), "Data"));
        }

        private string FindClickOnceFolder()
        {
            return DescendIntoSubfolders(GetApps20Folder());
        }
    }
}