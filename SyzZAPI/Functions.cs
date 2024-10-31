using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace SynZAPI
{
    public class Functions
    {
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

    }
}
