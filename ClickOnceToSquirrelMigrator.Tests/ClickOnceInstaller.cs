using System;
using System.Deployment.Application;
using System.Threading.Tasks;

namespace ClickOnceToSquirrelMigrator.Tests
{
    internal class ClickOnceInstaller
    {
        public async Task InstallClickOnceApp(Uri deploymentUri)
        {
            using (var host = new InPlaceHostingManager(deploymentUri, false))
            {
                await GetApplicationManifest(host);
                AssertApplicationRequirements(host);
                await DownloadApplication(host);
            }
        }

        private static void AssertApplicationRequirements(InPlaceHostingManager host)
        {
            host.AssertApplicationRequirements(true);
        }

        private static Task DownloadApplication(InPlaceHostingManager host)
        {
            var completion = new TaskCompletionSource<int>();

            host.DownloadApplicationCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    completion.SetException(e.Error);
                }

                else
                {
                    completion.SetResult(0);
                }
            };

            host.DownloadApplicationAsync();

            return completion.Task;
        }

        private static Task GetApplicationManifest(InPlaceHostingManager host)
        {
            var completion = new TaskCompletionSource<int>();

            host.GetManifestCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    completion.SetException(e.Error);
                }

                else
                {
                    completion.SetResult(0);
                }
            };

            host.GetManifestAsync();

            return completion.Task;
        }
    }
}