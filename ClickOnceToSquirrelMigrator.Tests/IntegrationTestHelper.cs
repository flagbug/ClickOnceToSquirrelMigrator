using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using Squirrel;
using Xunit;

namespace ClickOnceToSquirrelMigrator.Tests
{
    public class IntegrationTestHelper
    {
        public static readonly string ClickOnceAppName = "ClickOnceApp";

        private static string directoryChars;

        public static IDisposable WithClickOnceApp()
        {
            string clickOnceApp = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "ClickOnceApp/ClickOnceApp.application"); // omg
            var installer = new ClickOnceInstaller();
            installer.InstallClickOnceApp(new Uri(clickOnceApp)).Wait();

            return Disposable.Create(() =>
            {
                UninstallInfo theApp = UninstallInfo.Find(ClickOnceAppName);
                var uninstaller = new Uninstaller();
                uninstaller.Uninstall(theApp);
            });
        }

        public static IDisposable WithSquirrelApp()
        {
            string squirrelUpdatePath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "SquirrelApp"); // omg
            string rootDir;

            var tempDisp = WithTempDirectory(out rootDir);

            var updateManager = new UpdateManager(squirrelUpdatePath, "SquirrelApp", FrameworkVersion.Net45, rootDir);

            updateManager.FullInstall(true).Wait();

            return Disposable.Create(() =>
            {
                updateManager.FullUninstall().Wait();
                updateManager.Dispose();
                tempDisp.Dispose();
            });
        }

        public static IDisposable WithTempDirectory(out string path)
        {
            var di = new DirectoryInfo(Environment.GetEnvironmentVariable("SQUIRREL_TEMP") ?? Environment.GetEnvironmentVariable("TEMP") ?? "");
            if (!di.Exists)
            {
                throw new Exception("%TEMP% isn't defined, go set it");
            }

            var tempDir = default(DirectoryInfo);

            directoryChars = directoryChars ?? (
                "abcdefghijklmnopqrstuvwxyz" +
                Enumerable.Range(0x4E00, 0x9FCC - 0x4E00)  // CJK UNIFIED IDEOGRAPHS
                    .Aggregate(new StringBuilder(), (acc, x) => { acc.Append(Char.ConvertFromUtf32(x)); return acc; })
                    .ToString());

            foreach (var c in directoryChars)
            {
                var target = Path.Combine(di.FullName, c.ToString());

                if (!File.Exists(target) && !Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                    tempDir = new DirectoryInfo(target);
                    break;
                }
            }

            path = tempDir.FullName;

            return Disposable.Create(() =>
            {
                try
                {
                    Directory.Delete(tempDir.FullName, true);
                }
                catch (Exception)
                { }
            });
        }
    }
}