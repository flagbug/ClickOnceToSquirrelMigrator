using System;
using System.IO;
using System.Threading.Tasks;
using Splat;
using Squirrel;

namespace ClickOnceToSquirrelMigrator
{
    /// <summary>
    /// A class to aid the migration from the ClickOnce based installer to the Squirrel.Windows
    /// based one.
    /// </summary>
    public class ClickOnceToSquirrelMigrator : IEnableLogger
    {
        private readonly string clickOnceAppName;
        private readonly IUpdateManager updateManager;
        private UninstallInfo clickOnceInfo;

        /// <summary>
        /// Creates a new instance of the <see cref="ClickOnceToSquirrelMigrator" /> class.
        /// </summary>
        /// <param name="updateManager">The Squirrel <see cref="IUpdateManager" /></param>
        /// <param name="clickOnceAppName">
        /// The name of the ClickOnce version of your app. This is used to find the application and
        /// uninstall it.
        /// </param>
        public ClickOnceToSquirrelMigrator(IUpdateManager updateManager, string clickOnceAppName)
        {
            if (updateManager == null)
                throw new ArgumentNullException("updateManager");

            if (String.IsNullOrWhiteSpace(clickOnceAppName))
                throw new ArgumentException("clickOnceAppName");

            this.updateManager = updateManager;
            this.clickOnceAppName = clickOnceAppName;
        }

        /// <summary>
        /// Installs the Squirrel version of your app and removes the ClickOnce shortcut so the
        /// users doesn't have a duplicate shortcut.
        /// 
        /// Call this method in a new version of your ClickOnce app.
        /// </summary>
        public async Task InstallSquirrel()
        {
            await this.InstallSquirrelDeployment();

            await this.RemoveClickOnceShortcut();
        }

        /// <summary>
        /// Uninstalls the ClickOnce version of your application.
        /// 
        /// Call this method from the Squirrel version of your application.
        /// </summary>
        /// <remarks>
        /// After this method completes, you may want to set a flag somewhere, so you don't call
        /// this method the next time your Squirrel app starts.
        /// </remarks>
        public async Task UninstallClickOnce()
        {
            var uninstallInfo = await this.GetClickOnceInfo();

            if (uninstallInfo == null)
            {
                this.Log().Info("Couldn't find the ClickOnce deployment, bailing...");
                return;
            }

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

            if (info == null)
            {
                this.Log().Info("Couldn't find the ClickOnce deployment, bailing...");
                return;
            }

            string programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string folder = Path.Combine(programsFolder, info.ShortcutFolderName);
            string suiteFolder = Path.Combine(folder, info.ShortcutSuiteName ?? string.Empty);
            string shortcut = Path.Combine(suiteFolder, info.ShortcutFileName + ".appref-ms");

            this.Log().Info("ClickOnce shortcut is located at {0}", shortcut);

            try
            {
                await Task.Run(() => File.Delete(shortcut));
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