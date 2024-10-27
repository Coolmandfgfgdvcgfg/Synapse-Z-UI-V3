using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SynZAPI;

namespace Synapse_Z_V3
{
    public partial class MainWindow : Window
    {
        Functions synapseZAPI = new Functions();
        private bool _isResizing = false; // Track if resizing is active
        private Point _startPoint;

        public MainWindow()
        {
            new Mutex(true, "ROBLOX_singletonMutex");
            InitializeComponent();
            InitializeWebView();
            this.Title = GenerateRandomTitle();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                string loaderPath = synapseZAPI.GetLoader();
                System.Diagnostics.Debug.WriteLine("Loader Path: " + loaderPath);

                // Await the async method to get account info
                string accountInfo = await synapseZAPI.GetAccountInfoAsync();
                System.Diagnostics.Debug.WriteLine("Account Info: " + accountInfo);

                // Update UI safely
                TimeLeft.Content = accountInfo;
            }
            catch (FileNotFoundException fnfEx)
            {
                System.Diagnostics.Debug.WriteLine("File not found: " + fnfEx.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private async void InitializeWebView()
        {
            await TemporyWebview.EnsureCoreWebView2Async(null);

            // Load the local HTML file
            string htmlFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.html");
            TemporyWebview.Source = new Uri(htmlFilePath);

            // Wait for the document to load
            TemporyWebview.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    // Call the SetTheme function with "studio" as an argument
                    string newprint = "print(\"Hello Sigma\")";
                    string script = "SetTheme('studio'); switchMinimap(true); SetText('" + newprint + "');";
                    await TemporyWebview.CoreWebView2.ExecuteScriptAsync(script);
                }
            };
        }


        private string GenerateRandomTitle()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = random.Next(15, 25); // Random length between 5 and 15 characters
            char[] titleChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                titleChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(titleChars);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Start dragging the window if the left mouse button is pressed
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Resizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                // Update the size of the window based on the current mouse position
                var mousePos = Mouse.GetPosition(this);
                this.Width = Math.Max(150, mousePos.X);
                this.Height = Math.Max(100, mousePos.Y);
            }
            else
            {
                // Change the cursor to indicate resizing
                if (e.LeftButton == MouseButtonState.Released)
                {
                    _isResizing = false;
                    Cursor = Cursors.Arrow;
                }
                else
                {
                    // Change the cursor based on the position
                    if (sender is Border resizer)
                    {
                        if (resizer.VerticalAlignment == VerticalAlignment.Stretch)
                        {
                            Cursor = Cursors.SizeWE; // Horizontal resizing
                        }
                        else
                        {
                            Cursor = Cursors.SizeNS; // Vertical resizing
                        }
                    }
                }
            }
        }

        private void Resizer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Begin resizing the window
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isResizing = true;
                _startPoint = e.GetPosition(this);
                Mouse.Capture((IInputElement)sender);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                double targetX = clickedButton == Button1 ? 0 : 28;
                if (clickedButton == Button1)
                {
                    EditorPage.Visibility = Visibility.Visible;
                    SettingsPage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EditorPage.Visibility = Visibility.Collapsed;
                    SettingsPage.Visibility = Visibility.Visible;
                }
                ThicknessAnimation marginAnimation = new ThicknessAnimation
                {
                    To = new Thickness(targetX, 28, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                Underline.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                ScaleTransform scaleTransform = new ScaleTransform(1.0, 1.0);
                button.RenderTransform = scaleTransform;
                button.RenderTransformOrigin = new Point(0.5, 0.5);

                DoubleAnimation scaleXAnimation = new DoubleAnimation(1.2, TimeSpan.FromMilliseconds(60));
                DoubleAnimation scaleYAnimation = new DoubleAnimation(1.2, TimeSpan.FromMilliseconds(60));

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.RenderTransform is ScaleTransform scaleTransform)
                {
                    DoubleAnimation scaleXAnimation = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(60));
                    DoubleAnimation scaleYAnimation = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(60));

                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
                }
            }
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            // Toggle full screen mode
            if (this.WindowState == WindowState.Normal)
            {
                // Store current window state and size before going full screen
                this.WindowState = WindowState.Maximized;
                this.Topmost = true; // Keep the window on top
            }
            else
            {
                // Restore to normal state
                this.WindowState = WindowState.Normal; // Restore window state
                this.Topmost = false; // Allow other windows to be on top
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            // Minimize the window
            this.WindowState = WindowState.Minimized;
        }
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                // Determine which ToggleButton was clicked and show the corresponding items
                if (toggleButton == ToggleLocalFiles)
                {
                    AnimateVisibility(LocalFilesItems, true);
                }
                else if (toggleButton == ToggleBookmarks)
                {
                    AnimateVisibility(BookmarksItems, true);
                }
                else if (toggleButton == ToggleGists)
                {
                    AnimateVisibility(GistsItems, true);
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                // Determine which ToggleButton was clicked and hide the corresponding items
                if (toggleButton == ToggleLocalFiles)
                {
                    AnimateVisibility(LocalFilesItems, false);
                }
                else if (toggleButton == ToggleBookmarks)
                {
                    AnimateVisibility(BookmarksItems, false);
                }
                else if (toggleButton == ToggleGists)
                {
                    AnimateVisibility(GistsItems, false);
                }
            }
        }

        private void AnimateVisibility(FrameworkElement element, bool isVisible)
        {
            // Set the desired visibility state
            element.Visibility = isVisible ? Visibility.Visible : Visibility.Visible; // Set to visible for animation

            // Create animations
            var fadeAnimation = new DoubleAnimation
            {
                From = isVisible ? 0 : 1,
                To = isVisible ? 1 : 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)), // Animation duration
                FillBehavior = FillBehavior.HoldEnd
            };

            var heightAnimation = new DoubleAnimation
            {
                From = isVisible ? 0 : element.ActualHeight,
                To = isVisible ? element.ActualHeight : 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)), // Animation duration
                FillBehavior = FillBehavior.HoldEnd
            };

            // Apply animations
            element.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
            element.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);

            // Set final visibility after animation completion
            if (!isVisible)
            {
                fadeAnimation.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
