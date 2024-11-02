using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace SynZAPI
{
    public class Functions
    {
        private HashSet<int> trackedPids = new HashSet<int>(); // Track PIDs for injection status

        public string GetLoader()
        {
            // Get the current directory and then find the parent directory
            string currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string parentDirectory = Directory.GetParent(currentDirectory).FullName; // Get the parent directory
            string currentExeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            string[] exePaths = Directory.GetFiles(parentDirectory, "*.exe", SearchOption.TopDirectoryOnly); // Search in the parent directory

            // Find the first executable that isn't the current one and contains the required patterns
            foreach (var exePath in exePaths)
            {
                if (!exePath.EndsWith(currentExeName, StringComparison.OrdinalIgnoreCase) && ContainsRequiredPatterns(exePath))
                {
                    return exePath;
                }
            }

            return null; // Return null if no valid executable is found
        }

        public List<int> GetRunningRobloxProcesses()
        {
            var robloxProcesses = Process.GetProcessesByName("RobloxPlayerBeta");
            return robloxProcesses.Select(p => p.Id).ToList();
        }

        private bool isMonitoring = false; // Track whether monitoring is active
        private bool injecting = false;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public async void Inject()
        {
            // Get the loader executable path
            string loaderExePath = GetLoader();
            string binFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            // Locate the executable in the bin folder
            string exePath = FindExecutableInBin(binFolderPath);
            if (string.IsNullOrEmpty(exePath))
            {
                ShowMessage("Executable not found in bin folder.", "Error");
                return;
            }

            if (string.IsNullOrEmpty(loaderExePath))
            {
                ShowMessage("Loader executable not found.", "Error");
                return;
            }

            // Get all currently running Roblox processes
            List<int> runningRobloxPids = GetRunningRobloxProcesses();

            // Check if any Roblox processes are running
            if (runningRobloxPids.Count == 0)
            {
                ShowMessage("Open Roblox before injecting.", "Info");
                return; // Exit if no Roblox processes are found
            }

            // Check if already injected into all running processes
            bool allInjected = true;
            foreach (int pid in runningRobloxPids)
            {
                if (!trackedPids.Contains(pid))
                {
                    allInjected = false;
                    break;
                }
            }

            if (allInjected)
            {
                ShowMessage("Already injected into all running Roblox processes.", "Info");
                return; // Exit if already injected into all processes
            }

            if (injecting)
            {
                return;
            }
            injecting = true;

            // Start the loader executable
            Process loaderProcess = Process.Start(new ProcessStartInfo
            {
                FileName = loaderExePath,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            // Monitor the loader process
            await Task.Run(() => WaitForProcessAndHide(exePath));

            // Inject into all running Roblox processes only once
            foreach (int pid in runningRobloxPids)
            {
                if (!trackedPids.Contains(pid))
                {
                    trackedPids.Add(pid); // Add PID to tracked list
                }
            }

            // Start monitoring if it's not already running
            if (!isMonitoring)
            {
                isMonitoring = true;
                Task.Run(() => MonitorTrackedProcesses());
            }

            // Wait for 7 seconds before allowing further injections
            await Task.Delay(7000);
            injecting = false;
        }

        private string FindExecutableInBin(string binFolderPath)
        {
            var exeFiles = Directory.GetFiles(binFolderPath, "*.exe");
            return exeFiles.Length > 0 ? exeFiles[0] : null; // Return the first executable found
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Constants for SetWindowPos
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private async Task WaitForProcessAndHide(string exePath)
        {
            bool windowHidden = false; // Flag to indicate if the window is hidden

            while (!windowHidden)
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exePath));
                if (processes.Length > 0)
                {
                    var hWnd = processes[0].MainWindowHandle;

                    // Check if the window handle is valid
                    if (hWnd != IntPtr.Zero)
                    {
                        // Move the window far off the screen to hide it
                        SetWindowPos(hWnd, HWND_BOTTOM, -2000, -2000, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

                        // Hide the window explicitly
                        ShowWindow(hWnd, SW_HIDE);

                        // Check if the window is hidden
                        if (!IsWindowVisible(hWnd))
                        {
                            windowHidden = true; 
                        }
                    }
                }
                await Task.Delay(1); 
            }

            injecting = false; 
        }
        // Import the necessary user32.dll function to hide the window
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Constants for window visibility
        private const int SW_HIDE = 0;

        private void MonitorTrackedProcesses()
        {
            while (trackedPids.Count > 0)
            {
                foreach (int pid in trackedPids.ToList()) 
                {
                    if (!IsProcessRunning(pid))
                    {
                        trackedPids.Remove(pid); // Remove closed PID from tracking
                    }
                }

                Thread.Sleep(1000); // Adjust the sleep time as needed
            }

            isMonitoring = false; // Reset the monitoring state when done
        }

        // Helper method to check if a process is still running
        private bool IsProcessRunning(int pid)
        {
            try
            {
                return Process.GetProcessById(pid) != null;
            }
            catch (ArgumentException)
            {
                return false; // Process no longer exists
            }
        }
        // Helper method to show a message box
        private void ShowMessage(string message, string title)
        {
            // Show a topmost message box using the Win32 API
            MessageBox(IntPtr.Zero, message, title, 0x00000000 | 0x00040000);
        }

        // Import the necessary Win32 API method
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        public bool IsInjected()
        {
            if (injecting == true)
            {
                return false;
            }
            // Return true if there are any tracked PIDs, otherwise return false
            return trackedPids.Count > 0;
        }

        private bool ContainsRequiredPatterns(string filePath)
            {
                byte[] fileContent = File.ReadAllBytes(filePath);
                string fileText = System.Text.Encoding.ASCII.GetString(fileContent);

                return fileText.Contains(".grh0") && fileText.Contains(".grh1") && fileText.Contains(".grh2");
            }



        public async Task<string> GetAccountInfoAsync()
        {
            string apiUrl = "http://api.synapsez.net/info"; 
            string apiKey = GetAuthKey();

            using (HttpClient client = new HttpClient())
            {
                // Set custom header for the API key
                client.DefaultRequestHeaders.Add("key", apiKey);

                try
                {
                    // Prepare an empty body with the correct content type
                    var content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        // Log the raw response content for debugging
                        Console.WriteLine($"Raw response content: '{responseContent}'");

                        // Trim whitespace from the response content
                        responseContent = responseContent.Trim();

                        // Attempt to convert the trimmed response content to a long
                        if (int.TryParse(responseContent, out int totalSeconds))
                        {
                            // Convert seconds to days and hours
                            int days = totalSeconds / 86400; // 86400 seconds in a day
                            int hours = (totalSeconds % 86400) / 3600; // 3600 seconds in an hour

                            // Format and return the desired string
                            return $"You have {days.ToString("N0")} days and {hours.ToString("N0")} hours left of your subscription.";
                        }
                        else
                        {
                            return $"You have infinite days and infinite hours left of your subscription.";
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        return $"Error: {errorContent}"; // Return the error message
                    }
                }
                catch (Exception ex)
                {
                    return $"Exception occurred: {ex.Message}"; // Return exception message
                }
            }
        }

        public string GetAuthKey()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = System.IO.Path.Combine(localAppData, "auth.syn");

            try
            {
                // Read the content of the auth.syn file
                if (System.IO.File.Exists(filePath))
                {
                    return System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    return string.Empty; // Return an empty string if the file doesn't exist
                }
            }
            catch (Exception ex)
            {
                return string.Empty; // Return an empty string on error
            }
        }


        public async Task<int> ResetHWIDAsync()
        {
            string apiUrl = "http://api.synapsez.net/resethwid";
            string apiKey = GetAuthKey();

            using (HttpClient client = new HttpClient())
            {
                // Set custom header for the API key
                client.DefaultRequestHeaders.Add("key", apiKey);

                try
                {
                    // Prepare an empty body with the correct content type
                    var content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    
                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent == "reset hwid successfully")
                        {
                            // Return 0 on success
                            return 0;
                        }
                        else
                        {
                            return 1; 
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        return 1; // Return 1 for HTTP errors
                    }
                }
                catch (Exception ex)
                {
                    return 1; // Return 1 for exceptions
                }
            }
        }

        public async Task<string> RedeemKeyAsync(string licenseKey)
        {
            string apiUrl = "http://api.synapsez.net/redeem";
            string apiKey = GetAuthKey();

            using (HttpClient client = new HttpClient())
            {
                // Set custom header for the API key
                client.DefaultRequestHeaders.Add("key", apiKey);
                client.DefaultRequestHeaders.Add("license", licenseKey);

                try
                {
                    // Prepare an empty body with the correct content type
                    var content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != String.Empty)
                        {
                            return responseContent;
                        }
                        else
                        {
                            return "Invalid Key.";
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        return "Failed to redeem key."; 
                    }
                }
                catch (Exception ex)
                {
                    return "Failed to redeem key."; 
                }
            }
        }


        public async Task Execute(string scriptContent, int? pid = null)
        {
            // Get the loader executable path
            string loaderExePath = GetLoader();

            if (string.IsNullOrEmpty(loaderExePath))
            {
                ShowMessage("Loader executable not found.", "Error");
                return;
            }

            string schedulerFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "scheduler");

            // Create the 'scheduler' folder if it doesn't exist
            if (!Directory.Exists(schedulerFolderPath))
            {
                Directory.CreateDirectory(schedulerFolderPath);
            }

            // If a specific PID is provided, only create a file for that PID
            if (pid.HasValue)
            {
                await CreateLuaFileAsync(schedulerFolderPath, pid.Value, scriptContent);
            }
            else
            {
                // If no PID is provided, create files for all tracked PIDs
                var tasks = trackedPids.Select(trackedPid => CreateLuaFileAsync(schedulerFolderPath, trackedPid, scriptContent));
                await Task.WhenAll(tasks);
            }
        }

        private async Task CreateLuaFileAsync(string schedulerFolderPath, int pid, string scriptContent)
        {
            // Generate a random GUID
            string randomGuid = Guid.NewGuid().ToString();
            string fileName = $"{pid}_{randomGuid}.lua";
            string filePath = Path.Combine(schedulerFolderPath, fileName);

            // Create the Lua file with the script content
            File.WriteAllText(filePath, scriptContent + "@@FileFullyWritten@@");

        }

    }
}
