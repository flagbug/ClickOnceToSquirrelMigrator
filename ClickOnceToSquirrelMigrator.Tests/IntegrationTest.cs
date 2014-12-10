using System;
using System.IO;
using System.Threading.Tasks;
using Squirrel;
using Xunit;

namespace ClickOnceToSquirrelMigrator.Tests
{
    public class IntegrationTest
    {
        [Fact]
        public async Task FirstStepInstallsSquirrelApp()
        {
            string squirrelUpdatePath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "SquirrelApp"); // omg

            string rootDir;

            using (IntegrationTestHelper.WithTempDirectory(out rootDir))
            {
                using (var updateManager = new UpdateManager(squirrelUpdatePath, "SquirrelApp", FrameworkVersion.Net45, rootDir))
                {
                    var migrator = new ClickOnceToSquirrelMigrator(updateManager, "ClickOnceApp");

                    await migrator.InstallSquirrel();

                    Assert.True(File.Exists(Path.Combine(rootDir, "SquirrelApp", "packages", "RELEASES")));
                }
            }
        }

        [Fact]
        public async Task UninstallsClickOnceApp()
        {
            string clickOnceApp = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "ClickOnceApp/ClickOnceApp.application"); // omg
            var installer = new ClickOnceInstaller();
            await installer.InstallClickOnceApp(new Uri(clickOnceApp));

            UninstallInfo theApp = UninstallInfo.Find("ClickOnceApp");

            Assert.NotNull(theApp);

            var uninstaller = new Uninstaller();
            uninstaller.Uninstall(theApp);

            UninstallInfo shouldBeNull = UninstallInfo.Find("ClickOnceApp");

            Assert.Null(shouldBeNull);
        }
    }
}