using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.IO.Compression;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SynBootstrapper
{
    public partial class MainWindow : Window
    {
        private static readonly string downloadUrl = "https://synapsez.net/download";
        private static readonly string zipFileName = "downloadedFile.zip";
        private static readonly string authFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "auth.syn");

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StatusLabel.Content = "Checking Authorization";

            await Task.Delay(500);
            // Check if auth.syn exists in %localappdata%
            if (!File.Exists(authFilePath))
            {
                Progress.Value = 10;
                HandleMissingAuthFile();
                return; // Stop further execution if auth file is missing
            }
            ContinueLoading();


        }

        private async void ContinueLoading()
        {
            StatusLabel.Content = "Getting Data";

            try
            {
                // Start the progress bar at 0
                //Progress.Value = 0;

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
                Progress.Value = 92;
                await Task.Delay(500);
                StatusLabel.Content = "Setting HWID";
                await ResetHWIDAsync();
                Progress.Value = 100;
                StatusLabel.Content = "Done";
                await Task.Delay(500);

                StatusLabel.Content = "Launching Synapse UI";
                await Task.Delay(1000);

                string uiBinPath = Path.Combine(baseDirectory, "UI-bin", "SynapseUI.exe");
                if (File.Exists(uiBinPath))
                {
                    bool isRobloxRunning = false;
                    foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
                    {
                        isRobloxRunning = true;
                        break; // Exit loop as soon as we find one instance
                    }

                    if (isRobloxRunning)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            "Roblox is currently open on startup. In order to use all the UI functions properly, Roblox must be closed. If you continue, Roblox will remain open but some functions may be limited like Multi-Instance.",
                            "Warning",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Warning
                        );
                        if (result != MessageBoxResult.OK)
                        { 
                            this.Close();
                            return; // Exit the method if the user chooses not to continue
                        }
                    }


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

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Hide the placeholder if there is text in the PasswordBox
            PlaceholderText.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }


        private async void LoginFunction(object sender, RoutedEventArgs e)
        {
            string apiUrl = "http://api.synapsez.net/info";
            string apiKey = passwordBox.Password;

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

                        if (responseContent == "0")
                        {
                            MessageBox.Show("Invalid Key.");
                        }
                        else
                        {
                            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                            string filePath = System.IO.Path.Combine(localAppData, "auth.syn");

                            try
                            {
                                System.IO.File.WriteAllText(filePath, apiKey);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to save key: {ex.Message}");
                                this.Close();
                                return;
                            }


                            LoginButton.IsEnabled = false;
                            ShowLogoGrid();
                            ContinueLoading();
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {response.StatusCode} - {response.ReasonPhrase}\n{errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        public string GetAuthKey() // taken from api
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


        public async Task<int> ResetHWIDAsync() // taken from api 
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

        private void HandleMissingAuthFile()
        {
            StatusLabel.Content = "Please login.";

            ShowLoginGrid();
        }

        private void ShowLoginGrid()
        {
            // Create easing function for smooth animations
            QuadraticEase easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

            // Create blur effects
            BlurEffect blurEffect = new BlurEffect { Radius = 0 };
            LoginGrid.Effect = blurEffect;

            // Fade out, slide up, and blur LogoGrid
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
            DoubleAnimation slideUpAnimation = new DoubleAnimation(0, -ActualHeight, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
            DoubleAnimation blurOutAnimation = new DoubleAnimation(0, 10, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };

            fadeOutAnimation.Completed += (s, e) =>
            {
                LogoGrid.Visibility = Visibility.Collapsed; // Hide LogoGrid
                LoginGrid.Visibility = Visibility.Visible; // Show LoginGrid

                // Fade in, slide in, and remove blur on LoginGrid
                DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
                DoubleAnimation slideInAnimation = new DoubleAnimation(ActualHeight, 0, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
                DoubleAnimation blurInAnimation = new DoubleAnimation(10, 0, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };

                LoginGrid.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                TranslateTransform loginTranslateTransform = new TranslateTransform(0, ActualHeight);
                LoginGrid.RenderTransform = loginTranslateTransform;

                loginTranslateTransform.BeginAnimation(TranslateTransform.YProperty, slideInAnimation);
                blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurInAnimation);
            };

            // Apply animations to LogoGrid
            LogoGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            LogoGrid.Effect = blurEffect;
            TranslateTransform logoTranslateTransform = new TranslateTransform(0, 0);
            LogoGrid.RenderTransform = logoTranslateTransform;
            logoTranslateTransform.BeginAnimation(TranslateTransform.YProperty, slideUpAnimation);
            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurOutAnimation);
        }

        private void ShowLogoGrid()
        {
            // Create easing function for smooth animations
            QuadraticEase easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

            // Create blur effects
            BlurEffect blurEffect = new BlurEffect { Radius = 0 };
            LogoGrid.Effect = blurEffect;

            // Fade out, slide down, and blur LoginGrid
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
            DoubleAnimation slideDownAnimation = new DoubleAnimation(0, ActualHeight, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
            DoubleAnimation blurOutAnimation = new DoubleAnimation(0, 10, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };

            fadeOutAnimation.Completed += (s, e) =>
            {
                LoginGrid.Visibility = Visibility.Collapsed; // Hide LoginGrid
                LogoGrid.Visibility = Visibility.Visible; // Show LogoGrid

                // Fade in, slide in, and remove blur on LogoGrid
                DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
                DoubleAnimation slideInAnimation = new DoubleAnimation(-ActualHeight, 0, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };
                DoubleAnimation blurInAnimation = new DoubleAnimation(10, 0, TimeSpan.FromSeconds(0.25)) { EasingFunction = easingFunction };

                LogoGrid.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                TranslateTransform logoTranslateTransform = new TranslateTransform(0, -ActualHeight);
                LogoGrid.RenderTransform = logoTranslateTransform;

                logoTranslateTransform.BeginAnimation(TranslateTransform.YProperty, slideInAnimation);
                blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurInAnimation);
            };

            // Apply animations to LoginGrid
            LoginGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            LoginGrid.Effect = blurEffect;
            TranslateTransform loginTranslateTransform = new TranslateTransform(0, 0);
            LoginGrid.RenderTransform = loginTranslateTransform;
            loginTranslateTransform.BeginAnimation(TranslateTransform.YProperty, slideDownAnimation);
            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurOutAnimation);
        }

    }
}
