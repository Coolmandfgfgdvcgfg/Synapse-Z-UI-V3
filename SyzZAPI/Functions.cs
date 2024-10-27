using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SynZAPI
{
    public class Functions
    {
        public string GetLoader()
            {
                string currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string currentExeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                string[] exePaths = Directory.GetFiles(currentDirectory, "*.exe", SearchOption.TopDirectoryOnly);

                // Find the first executable that isn't the current one and contains the required patterns
                foreach (var exePath in exePaths)
                {
                    if (!exePath.EndsWith(currentExeName, StringComparison.OrdinalIgnoreCase) && ContainsRequiredPatterns(exePath))
                    {
                        return exePath;
                    }
                }

                return null;
            }

            private bool ContainsRequiredPatterns(string filePath)
            {
                byte[] fileContent = File.ReadAllBytes(filePath);
                string fileText = System.Text.Encoding.ASCII.GetString(fileContent);

                return fileText.Contains(".grh0") && fileText.Contains(".grh1") && fileText.Contains(".grh2");
            }

            public async Task<string> RunLoaderAsync(string startParams)
            {
                string loaderPath = GetLoader();

                if (loaderPath == null)
                {
                    throw new FileNotFoundException("No suitable loader executable found.");
                }

                using (Process process = new Process())
                {
                    process.EnableRaisingEvents = true;
                    process.StartInfo.FileName = loaderPath;
                    process.StartInfo.Arguments = startParams;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardError = true; // Capture standard error
                    process.StartInfo.CreateNoWindow = true; // Prevent the console window from appearing

                    var outputTaskCompletionSource = new TaskCompletionSource<bool>();
                    var outputBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            outputBuilder.AppendLine(e.Data);
                            outputTaskCompletionSource.TrySetResult(true);
                        }
                        else
                        {
                            // Signal that the output reading is complete
                            outputTaskCompletionSource.TrySetResult(true);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine(); 

                    await process.StandardInput.WriteLineAsync(); 
                    await outputTaskCompletionSource.Task;

                    return outputBuilder.ToString();
                }
            }

        public async Task<string> GetAccountInfoAsync()
        {
            // Get the account info string, e.g., "12 days 5 hours"
            string accountInfo = await RunLoaderAsync("info");

            // Split the account info string to extract days and hours
            var parts = accountInfo.Split(' ');
            if (parts.Length >= 4) // Ensure we have at least "X days Y hours"
            {
                // Parse days and hours
                if (int.TryParse(parts[0], out int days) && int.TryParse(parts[2], out int hours))
                {
                    // Convert hours to days if over 24
                    days += hours / 24;
                    hours = hours % 24;

                    // Format with commas and return the desired string
                    return $"You have {days.ToString("N0")} days and {hours.ToString("N0")} hours left of your subscription.";
                }
            }

            // If the format is not as expected, return an error or default message
            return "Account information is not in the expected format.";
        }
    }
}
