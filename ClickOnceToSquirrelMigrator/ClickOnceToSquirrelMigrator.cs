using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Splat;
using Squirrel;
using Wunder.ClickOnceUninstaller;

namespace ClickOnceToSquirrelMigrator
{
    /// <summary>
    /// A class to aid the migration from the ClickOnce based installer to the Squirrel.Windows
    /// based one.
    /// </summary>
    public class ClickOnceToSquirrelMigrator : IEnableLogger
    {
        private readonly string clickOnceAppName;
        private readonly IFileSystem fileSystem;
        private readonly IUpdateManager updateManager;
        private UninstallInfo clickOnceInfo;

        public ClickOnceToSquirrelMigrator(IUpdateManager updateManager, string clickOnceAppName, IFileSystem fileSystem = null)
        {
            if (updateManager == null)
                throw new ArgumentNullException("updateManager");

            if (String.IsNullOrWhiteSpace(clickOnceAppName))
                throw new ArgumentException("clickOnceAppName");

            this.updateManager = updateManager;
            this.clickOnceAppName = clickOnceAppName;
            this.fileSystem = fileSystem ?? new FileSystem();
        }

        public async Task InstallSquirrel()
        {
            await this.InstallSquirrelDeployment();

            await this.RemoveClickOnceShortcut();
        }

        public async Task UninstallClickOnce()
        {
            var uninstallInfo = await this.GetClickOnceInfo();

            if (uninstallInfo == null)
                return;

            var uninstaller = new Uninstaller();
            await Task.Run(() => uninstaller.Uninstall(uninstallInfo));
        }

        private async Task<UninstallInfo> GetClickOnceInfo()
        {
            if (clickOnceInfo != null)
                return this.clickOnceInfo;

            this.clickOnceInfo = await Task.Run(() => UninstallInfo.Find(this.clickOnceAppName));

            return this.clickOnceInfo;
        }

        private async Task InstallSquirrelDeployment()
        {
            this.Log().Info("Starting the install of the Squirrel version");

            try
            {
                await this.updateManager.FullInstall(true);
            }

            catch (Exception ex)
            {
                this.Log().FatalException("Failed to do a full Squirrel Install. Yikes!", ex);
                return;
            }

            this.Log().Info("Finished the install of the Squirrel version");
        }

        private async Task RemoveClickOnceShortcut()
        {
            this.Log().Info("Removing ClickOnce shortcut");

            UninstallInfo info = await this.GetClickOnceInfo();

            string programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string folder = Path.Combine(programsFolder, info.ShortcutFolderName);
            string suiteFolder = Path.Combine(folder, info.ShortcutSuiteName ?? string.Empty);
            string shortcut = Path.Combine(suiteFolder, info.ShortcutFileName + ".appref-ms");

            this.Log().Info("ClickOnce shortcut is located at {0}", shortcut);

            try
            {
                await Task.Run(() => this.fileSystem.File.Delete(shortcut));
            }

            catch (Exception ex)
            {
                this.Log().ErrorException("Failed to remove ClickOnce shortcut", ex);
                return;
            }

            this.Log().Info("Removed ClickOnce shortcut");
        }
    }
}