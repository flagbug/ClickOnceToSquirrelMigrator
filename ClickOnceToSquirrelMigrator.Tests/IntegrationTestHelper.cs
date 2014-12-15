using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Squirrel;

namespace ClickOnceToSquirrelMigrator.Tests
{
    public static class IntegrationTestHelper
    {
        public static readonly string ClickOnceAppName = "ClickOnceApp";
        public static readonly string ClickOnceTestAppPath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "ClickOnceApp/ClickOnceApp.application"); // omg
        public static readonly string SquirrelAppName = "SquirrelApp";
        public static readonly string SquirrelTestAppPath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "SquirrelApp"); // omg

        private static string directoryChars;

        public static IDisposable CleanupSquirrel(IUpdateManager updateManager)
        {
            return Disposable.Create(() =>
            {
                updateManager.FullUninstall().Wait();
                updateManager.RemoveUninstallerRegistryEntry();
            });
        }

        public static async Task DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            // From http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true/329502#329502
            var files = new string[0];
            try
            {
                files = Directory.GetFiles(directoryPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                var message = String.Format("The files inside {0} could not be read", directoryPath);
            }

            var dirs = new string[0];
            try
            {
                dirs = Directory.GetDirectories(directoryPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                var message = String.Format("The directories inside {0} could not be read", directoryPath);
            }

            var fileOperations = files.ForEachAsync(file =>
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                catch (Exception)
                { }
            });

            var directoryOperations =
                dirs.ForEachAsync(async dir => await DeleteDirectory(dir));

            await Task.WhenAll(fileOperations, directoryOperations);

            File.SetAttributes(directoryPath, FileAttributes.Normal);

            try
            {
                Directory.Delete(directoryPath, false);
            }
            catch (Exception ex)
            {
                var message = String.Format("DeleteDirectory: could not delete - {0}", directoryPath);
            }
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Action<T> body, int degreeOfParallelism = 4)
        {
            return ForEachAsync(source, x => Task.Run(() => body(x)), degreeOfParallelism);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int degreeOfParallelism = 4)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(degreeOfParallelism)
                select Task.Run(async () =>
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        public static UpdateManager GetSquirrelUpdateManager(string rootDir)
        {
            return new UpdateManager(IntegrationTestHelper.SquirrelTestAppPath, SquirrelAppName, FrameworkVersion.Net45, rootDir);
        }

        public static IDisposable WithClickOnceApp()
        {
            string clickOnceApp = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "ClickOnceApp/ClickOnceApp.application"); // omg
            var installer = new ClickOnceInstaller();
            installer.InstallClickOnceApp(new Uri(clickOnceApp)).Wait();

            return Disposable.Create(() =>
            {
                UninstallInfo theApp = UninstallInfo.Find(ClickOnceAppName);

                if (theApp == null)
                    return;

                var uninstaller = new Uninstaller();
                uninstaller.Uninstall(theApp);
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

            return Disposable.Create(() => Task.Run(async () => await DeleteDirectory(tempDir.FullName)).Wait());
        }
    }
}