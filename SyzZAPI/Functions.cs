using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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

            var outputBuilder = new StringBuilder();
            TaskCompletionSource<bool> outputReceived = new TaskCompletionSource<bool>();

            using (Process process = new Process())
            {
                process.EnableRaisingEvents = true;
                process.StartInfo.FileName = loaderPath;
                process.StartInfo.Arguments = startParams;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = false; // Show the console window

                // Start process
                process.Start();

                // Read the output continuously until there's no more
                while (!process.HasExited)
                {
                    string outputLine = await process.StandardOutput.ReadLineAsync();
                    if (outputLine != null) // Check if outputLine is not null
                    {
                        outputBuilder.AppendLine(outputLine);
                        process.Kill();
                    }
                    else
                    {
                        // Break the loop if no more output is returned
                        break;
                    }
                }

       

                // Return the collected output
                return outputBuilder.ToString();
            }
        }


        public async Task<string> GetAccountInfoAsync()
        {

            return await RunLoaderAsync("info");
        }


    }
}
