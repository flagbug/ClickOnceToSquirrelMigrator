using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;

namespace ClickOnceToSquirrelMigrator.Tests
{
    public class IntegrationTestHelper
    {
        private static string directoryChars;

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

            return Disposable.Create(() => Directory.Delete(tempDir.FullName, true));
        }
    }
}