using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ClickOnceToSquirrelMigrator.Tests
{
    public class IntegrationTest
    {
        [Fact]
        public async Task UninstallsClickOnceApp()
        {
            string clickOnceApp = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "TestApp/ClickOnceApp.application"); // omg
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