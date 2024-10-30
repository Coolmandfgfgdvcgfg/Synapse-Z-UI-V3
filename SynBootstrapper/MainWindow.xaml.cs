using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.IO.Compression;

namespace SynBootstrapper
{
    public partial class MainWindow : Window
    {
        private static readonly string downloadUrl = "https://synapsez.net/download";
        private static readonly string zipFileName = "downloadedFile.zip";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StatusLabel.Content = "Getting Data";

            try
            {
                // Start the progress bar at 0
                Progress.Value = 0;

                StatusLabel.Content = "Downloading Synapse Z Launcher";
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string zipFilePath = Path.Combine(baseDirectory, zipFileName);

                // Download with progress
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0L;
                    var downloadedBytes = 0L;

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;

                        // Check if totalBytes is available or unknown
                        bool unknownSize = (totalBytes == 0);
                        double progressStep = unknownSize ? 0.5 : 90.0 / totalBytes; // Adjust to fill to 90%

                        // Read data in chunks and update the progress bar
                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fs.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            // Update the progress bar
                            if (unknownSize)
                            {
                                Progress.Value += progressStep;
                                if (Progress.Value >= 90) Progress.Value = 90; // Cap at 90% until completion
                            }
                            else
                            {
                                Progress.Value = (double)downloadedBytes / totalBytes * 90; // Fill to 90%
                            }
                        }
                    }
                }

                // Ensure progress bar completes at 90% before extraction
                Progress.Value = 90;

                StatusLabel.Content = "Extracting files";
                ZipFile.ExtractToDirectory(zipFilePath, baseDirectory, overwriteFiles: true);

                // Delete all .cmd files after extraction
                foreach (string file in Directory.GetFiles(baseDirectory, "*.cmd"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore errors here
                    }
                }

                File.Delete(zipFilePath); // Delete the downloaded zip file

                StatusLabel.Content = "Initializing";
                Progress.Value = 95;
                await Task.Delay(1000);
                Progress.Value = 100;
                StatusLabel.Content = "Done";
                await Task.Delay(500);

                StatusLabel.Content = "Launching Synapse UI";
                await Task.Delay(1000);

                string uiBinPath = Path.Combine(baseDirectory, "UI-bin", "SynapseUI.exe");
                if (File.Exists(uiBinPath))
                {
                    System.Diagnostics.Process.Start(uiBinPath);
                    this.Close();
                }
                else
                {
                    StatusLabel.Content = "Error: SynapseUI.exe not found after extraction";
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Content = "Error: " + ex.Message;
                await Task.Delay(2000);
            }
        }
    }
}
