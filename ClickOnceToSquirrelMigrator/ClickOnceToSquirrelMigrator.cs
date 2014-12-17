using System;
using System.Threading.Tasks;
using Splat;
using Squirrel;

namespace ClickOnceToSquirrelMigrator
{
    /// <summary>
    /// The migrator used in the ClickOnce version of your application.
    /// </summary>
    public class InClickOnceAppMigrator : IEnableLogger
    {
        private readonly string clickOnceAppName;
        private readonly IUpdateManager updateManager;

        /// <summary>
        /// Creates a new instance of the <see cref="InClickOnceAppMigrator" /> class.
        /// </summary>
        /// <param name="updateManager">
        /// The Squirrel <see cref="IUpdateManager" />, used to install the Squirrel-based application.
        /// </param>
        /// <param name="clickOnceAppName">
        /// The name of the installed ClickOnce app, used to remove the shortcuts.
        /// </param>
        public InClickOnceAppMigrator(IUpdateManager updateManager, string clickOnceAppName)
        {
            if (updateManager == null)
                throw new ArgumentNullException("updateManager");

            if (String.IsNullOrWhiteSpace(clickOnceAppName))
                throw new ArgumentException("clickOnceAppName");

            this.updateManager = updateManager;
            this.clickOnceAppName = clickOnceAppName;
        }

        /// <summary>
        /// Installs the Squirrel version of your app and removes the ClickOnce shortcuts so the
        /// users doesn't have duplicate shortcuts.
        /// 
        /// Call this method in a new version of your ClickOnce app.
        /// </summary>
        /// <exception cref="MigrationException">
        /// The exception that is thrown if the Squirrel-based install fails.
        /// </exception>
        public async Task Execute()
        {
            await this.InstallSquirrelDeployment();

            await this.RemoveClickOnceShortcuts();
        }

        private async Task InstallSquirrelDeployment()
        {
            this.Log().Info("Starting the installation of the Squirrel version");

            try
            {
                await this.updateManager.FullInstall(true);
            }

            catch (Exception ex)
            {
                this.Log().FatalException("Failed to do a full Squirrel install. Yikes!", ex);

                throw new MigrationException("Squirrel-based install failed.", ex);
            }

            await this.updateManager.CreateUninstallerRegistryEntry();

            this.Log().Info("Finished the installation of the Squirrel version");
        }

        private async Task RemoveClickOnceShortcuts()
        {
            this.Log().Info("Removing ClickOnce shortcuts");

            UninstallInfo info = await Task.Run(() => UninstallInfo.Find(this.clickOnceAppName));

            if (info == null)
            {
                this.Log().Info("Couldn't find the ClickOnce deployment, bailing...");
                return;
            }

            var remover = new RemoveStartMenuEntry(info);

            try
            {
                await Task.Run(() =>
                {
                    remover.Prepare();
                    remover.Execute();
                });
            }

            catch (Exception ex)
            {
                this.Log().ErrorException("Failed to remove ClickOnce shortcuts", ex);
                return;
            }

            this.Log().Info("Removed ClickOnce shortcuts");
        }
    }

    /// <summary>
    /// The migrator used in the Squirrel version of your application.
    /// </summary>
    public class InSquirrelAppMigrator : IEnableLogger
    {
        private readonly string clickOnceAppName;

        /// <summary>
        /// Creates a new instance of the <see cref="InSquirrelAppMigrator" /> class.
        /// </summary>
        /// <param name="clickOnceAppName">
        /// The name of the ClickOnce version of your app. This is used to find the application and
        /// uninstall it.
        /// </param>
        public InSquirrelAppMigrator(string clickOnceAppName)
        {
            if (String.IsNullOrWhiteSpace(clickOnceAppName))
                throw new ArgumentException("clickOnceAppName");

            this.clickOnceAppName = clickOnceAppName;
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
        public async Task Execute()
        {
            UninstallInfo uninstallInfo = await Task.Run(() => UninstallInfo.Find(this.clickOnceAppName));

            if (uninstallInfo == null)
            {
                this.Log().Info("Couldn't find the ClickOnce deployment, bailing...");
                return;
            }

            var uninstaller = new Uninstaller();
            await Task.Run(() => uninstaller.Uninstall(uninstallInfo));
        }
    }
}