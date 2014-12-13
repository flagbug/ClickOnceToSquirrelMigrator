using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Splat;
using Squirrel;

namespace ClickOnceToSquirrelMigrator
{
    internal class RemoveStartMenuEntry : IUninstallStep, IEnableLogger
    {
        private readonly UninstallInfo _uninstallInfo;
        private List<string> _filesToRemove;
        private List<string> _foldersToRemove;

        public RemoveStartMenuEntry(UninstallInfo uninstallInfo)
        {
            _uninstallInfo = uninstallInfo;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            if (_foldersToRemove == null)
                throw new InvalidOperationException("Call Prepare() first.");

            foreach (var file in _filesToRemove)
            {
                try
                {
                    File.Delete(file);
                }

                catch (IOException ex)
                {
                    this.Log().WarnException("Failed to remove shortcut file " + file, ex);
                }
            }

            foreach (var folder in _foldersToRemove)
            {
                try
                {
                    Directory.Delete(folder, false);
                }

                catch (IOException ex)
                {
                    this.Log().WarnException("Failed to remove folder " + folder, ex);
                }
            }
        }

        public void Prepare(List<string> componentsToRemove = null)
        {
            var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            var folder = Path.Combine(programsFolder, _uninstallInfo.ShortcutFolderName);
            var suiteFolder = Path.Combine(folder, _uninstallInfo.ShortcutSuiteName ?? string.Empty);
            var shortcut = Path.Combine(suiteFolder, _uninstallInfo.ShortcutFileName + ".appref-ms");
            var supportShortcut = Path.Combine(suiteFolder, _uninstallInfo.SupportShortcutFileName + ".url");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var desktopShortcut = Path.Combine(desktopFolder, _uninstallInfo.ShortcutFileName + ".appref-ms");

            string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string taskbarShortcut = Path.Combine(roamingFolder, @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar",
                _uninstallInfo.ShortcutFileName + ".appref-ms");

            if (File.Exists(taskbarShortcut))
            {
                try
                {
                    TaskbarHelper.UnpinFromTaskbar(taskbarShortcut);
                }

                catch (Exception ex)
                {
                    this.Log().ErrorException("Failed to unpin shortcut " + taskbarShortcut, ex);
                }
            }

            _filesToRemove = new List<string>();
            if (File.Exists(shortcut)) _filesToRemove.Add(shortcut);
            if (File.Exists(supportShortcut)) _filesToRemove.Add(supportShortcut);
            if (File.Exists(desktopShortcut)) _filesToRemove.Add(desktopShortcut);
            if (File.Exists(taskbarShortcut)) _filesToRemove.Add(taskbarShortcut);

            _foldersToRemove = new List<string>();
            if (Directory.Exists(suiteFolder) && Directory.GetFiles(suiteFolder).All(d => _filesToRemove.Contains(d)))
            {
                _foldersToRemove.Add(suiteFolder);

                if (Directory.GetDirectories(folder).Count() == 1 && !Directory.GetFiles(folder).Any())
                    _foldersToRemove.Add(folder);
            }
        }

        public void PrintDebugInformation()
        {
            if (_foldersToRemove == null)
                throw new InvalidOperationException("Call Prepare() first.");

            var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            this.Log().Info("Remove start menu entries from " + programsFolder);

            foreach (var file in _filesToRemove)
            {
                this.Log().Info("Delete file " + file);
            }

            foreach (var folder in _foldersToRemove)
            {
                this.Log().Info("Delete folder " + folder);
            }
        }
    }
}