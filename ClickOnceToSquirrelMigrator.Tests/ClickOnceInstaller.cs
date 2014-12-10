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

        /// <summary>
        /// Verifies that the computer meets the requirements of the application based on its
        /// manifest. (ie. assemblies that are required to be in the GAC are already present,
        /// framework version, etc.)
        /// </summary>
        /// <remarks>
        /// If an error occurs, the process will exit and the method will not return. Cheap, I know.
        /// I'm in a hurry.
        /// </remarks>
        /// <param name="host">The ClickOnce hosting manager.</param>
        private static void AssertApplicationRequirements(InPlaceHostingManager host)
        {
            host.AssertApplicationRequirements();
        }

        /// <summary>
        /// Begins downloading the application binaries and blocks until complete.
        /// </summary>
        /// <remarks>
        /// If an error occurs, the process will exit and the method will not return. Cheap, I know.
        /// I'm in a hurry.
        /// </remarks>
        /// <param name="host">The ClickOnce hosting manager.</param>
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

        /// <summary>
        /// Begins downloading the application manifest and blocks until complete.
        /// </summary>
        /// <remarks>
        /// If an error occurs, the process will exit and the method will not return. Cheap, I know.
        /// I'm in a hurry.
        /// </remarks>
        /// <param name="host">The ClickOnce hosting manager.</param>
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