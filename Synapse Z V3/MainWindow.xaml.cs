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
using Synapse_Z_V3.Settings;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Effects;

namespace Synapse_Z_V3
{
    public partial class MainWindow : Window
    {
        Functions synapseZAPI = new Functions();
        private bool _isResizing = false; // Track if resizing is active
        private Point _startPoint;
        private Window1 browserWindow;

        public MainWindow()
        {
            Mutex mutex = new Mutex(true, "ROBLOX_singletonMutex");

          
            InitializeComponent();  // Initialize the UI components
            InitializeAsync();
            AnimateSplashScreen();  // Start the splash screen animation
            SplashScreen.Visibility = Visibility.Visible;
            EditorPage.Visibility = Visibility.Visible;
            SettingsPage.Visibility = Visibility.Collapsed;
        }


        private async void AnimateSplashScreen()
        {
            // Initial delay to display the splash screen for 1 second
            await Task.Delay(1000);
            InitializeWebView();
            // Create a BlurEffect and set its initial radius
            BlurEffect blurEffect = new BlurEffect
            {
                Radius = 10 // Adjust the blur intensity as needed
            };

            // Apply the blur effect to the MainWindowGrid
            MainWindowGrid.Effect = blurEffect;

            // Create the fade-in animation for the blur effect
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 1, // Fade in to full opacity
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Apply fade-in to the MainWindowGrid
            MainWindowGrid.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);

            // Create the slide down animation for the splash screen
            DoubleAnimation slideDownAnimation = new DoubleAnimation
            {
                To = Height, // Slide down to the height of the window
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Create the fade out animation for the splash screen
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                To = 0, // Fade to transparent
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Set the RenderTransform for the SplashScreen
            SplashScreen.RenderTransform = new TranslateTransform();

            // Begin the slide down animation
            SplashScreen.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideDownAnimation);

            // Begin the fade out animation
            SplashScreen.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);

            // Wait for the duration of the splash screen animations
            await Task.Delay(250); // Match the duration of the animations

            // Create the fade out animation for the blur effect
            DoubleAnimation fadeOutBlurAnimation = new DoubleAnimation
            {
                To = 0, // Fade out to no blur
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Animate the blur radius to 0 to fade out the blur effect
            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, fadeOutBlurAnimation);

            // Wait for the fade out of the blur effect to complete
            await Task.Delay(500); // Match the duration of the fade out animation

            // Remove the blur effect after fading out
            MainWindowGrid.Effect = null; // Remove the blur effect

            // Now you can initialize the rest of the application
            InitializeUI(); // Call a separate method to initialize other components
        }

        private void InitializeUI()
        {
            // Now you can proceed with the rest of your initialization
           

            
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            InitializeCheckboxStates();
          
            // Attach event handlers
            this.Closing += MainWindow_Closing;
            this.Deactivated += MainWindow_Deactivated;
            this.Activated += MainWindow_Activated;
            this.SizeChanged += Window_SizeChanged;
        }
        public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new System.Windows.Thickness(7);
            }
            else
            {
                this.BorderThickness = new System.Windows.Thickness(0);
            }
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            AnimateOpacity(1.0);
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            if (GlobalSettings.Transparency == true)
            {
                AnimateOpacity(0.7);
            }
        }

        private void AnimateOpacity(double targetOpacity)
        {
            // Create a DoubleAnimation to animate the Opacity property
            DoubleAnimation animation = new DoubleAnimation
            {
                To = targetOpacity,
                Duration = TimeSpan.FromSeconds(0.25), // Duration of the animation
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } // Smooth animation
            };

            // Begin the animation on the Opacity property
            this.BeginAnimation(Window.OpacityProperty, animation);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalSettings.SaveSettings(); // Save settings before closing
        }


        private void InitializeCheckboxStates()
        {
            MinimapToggle.IsChecked = GlobalSettings.Minimap;
            TransparencyToggle.IsChecked = GlobalSettings.Transparency;
            TopmostToggle.IsChecked = GlobalSettings.Topmost;
            RandomizeToggle.IsChecked = GlobalSettings.Randomize;
            // Initialize other checkboxes similarly
            // Assuming you have another checkbox named OtherToggle
            // OtherToggle.IsChecked = GlobalSettings.OtherSetting; // Example for another setting
            CheckAllCheckboxes();
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

        private async void UpdateAccountInfoAsync(object sender, RoutedEventArgs e)
        {
            TimeLeft.Content = "...";
            try
            {
                // Await the async method to get account info
                string accountInfo = await synapseZAPI.GetAccountInfoAsync();

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
            string htmlFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "editor.html");
            TemporyWebview.Source = new Uri(htmlFilePath);

            // Wait for the document to load
            TemporyWebview.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    // Call the SetTheme function with "studio" as an argument
                    string newprint = "print(\"Hello Sigma\")";
                    string minimapScript = GlobalSettings.Minimap ? "switchMinimap(true);" : "switchMinimap(false);";
                    string script = $"SetTheme('studio'); SetText('{newprint}');";

                    // Combine both scripts into one execution command
                    string combinedScript = $"{minimapScript} {script}";
                    await TemporyWebview.CoreWebView2.ExecuteScriptAsync(combinedScript);
                }
            };
        }



        private void OpenWebPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the browser window is already open
            if (browserWindow == null || !browserWindow.IsVisible)
            {
                browserWindow = new Window1(); // Create a new instance
                browserWindow.Show(); // Show the window
            }
            else
            {
                browserWindow.Focus(); // Bring the existing window to the front
            }
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
                // Retrieve current Y positions for each page to keep them consistent
                double currentY = EditorPage.Margin.Top;

                // Define target margins for horizontal sliding effect
                Thickness editorTargetMargin = clickedButton == Button1 ?
                    new Thickness(0, currentY, 0, 0) : new Thickness(-300, currentY, 300, 0);
                Thickness settingsTargetMargin = clickedButton == Button1 ?
                    new Thickness(300, currentY, -300, 0) : new Thickness(0, currentY, 0, 0);

                // Slide animations for EditorPage and SettingsPage
                ThicknessAnimation editorSlideAnimation = new ThicknessAnimation
                {
                    To = editorTargetMargin,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                ThicknessAnimation settingsSlideAnimation = new ThicknessAnimation
                {
                    To = settingsTargetMargin,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                // Fade-out animation for the current page
                DoubleAnimation fadeOutAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                fadeOutAnimation.Completed += (s, a) =>
                {
                    if (clickedButton == Button1)
                        SettingsPage.Visibility = Visibility.Collapsed;
                    else
                        EditorPage.Visibility = Visibility.Collapsed;
                };

                // Fade-in animation for the new page
                DoubleAnimation fadeInAnimation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(150),
                    BeginTime = TimeSpan.FromMilliseconds(150) // Sync fade-in with fade-out
                };

                // Apply animations based on the clicked button
                if (clickedButton == Button1)
                {
                    SettingsPage.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnimation);
                    SettingsPage.BeginAnimation(FrameworkElement.MarginProperty, settingsSlideAnimation);

                    EditorPage.Visibility = Visibility.Visible;
                    EditorPage.BeginAnimation(FrameworkElement.OpacityProperty, fadeInAnimation);
                    EditorPage.BeginAnimation(FrameworkElement.MarginProperty, editorSlideAnimation);
                }
                else
                {
                    EditorPage.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnimation);
                    EditorPage.BeginAnimation(FrameworkElement.MarginProperty, editorSlideAnimation);

                    SettingsPage.Visibility = Visibility.Visible;
                    SettingsPage.BeginAnimation(FrameworkElement.OpacityProperty, fadeInAnimation);
                    SettingsPage.BeginAnimation(FrameworkElement.MarginProperty, settingsSlideAnimation);
                }

                // Animate underline movement
                double underlineTargetX = clickedButton == Button1 ? 0 : 28;
                ThicknessAnimation underlineAnimation = new ThicknessAnimation
                {
                    To = new Thickness(underlineTargetX, 28, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(140),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                Underline.BeginAnimation(FrameworkElement.MarginProperty, underlineAnimation);
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

        private void CheckAllCheckboxes()
        {
            // Assuming your ScrollViewer is named 'CheckboxesSettings'
            if (CheckboxesSettings.Content is Panel panel) // Check if the content is a Panel
            {
                foreach (var child in panel.Children) // Loop through children of the Panel
                {
                    if (child is CheckBox checkBox)
                    {
                        // Call the original event handler with the checkbox state
                        HandleCheckboxLogic(checkBox, checkBox.IsChecked);
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CheckboxesSettings's content is not a Panel.");
            }
        }

        private void CheckboxesSettings_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Top and bottom of the visible scroll area
            double visibleTopY = e.VerticalOffset;
            double visibleBottomY = visibleTopY + e.ViewportHeight;

            // Dictionary to store distances to top/bottom edges
            var sectionDistances = new Dictionary<ToggleButton, double>();

            // Calculate the distances for each section
            sectionDistances[EditorScrollButton] = CalculateNearestDistanceToTop(EditorScroll, visibleTopY, visibleBottomY);
            sectionDistances[InjectionScrollButton] = CalculateNearestDistanceToTop(InjectionScroll, visibleTopY, visibleBottomY);
            sectionDistances[ConfigScrollButton] = CalculateNearestDistanceToTop(ConfigScroll, visibleTopY, visibleBottomY);
            sectionDistances[GeneralScrollButton] = CalculateNearestDistanceToTop(GeneralScroll, visibleTopY, visibleBottomY);


            // Find the ToggleButton with the closest distance to top or bottom
            var closestButton = sectionDistances.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;

            // Update ToggleButtons, ensuring only the closest button is checked
            foreach (ToggleButton button in IconStack.Children)
            {
                button.IsChecked = (button == closestButton);
            }
        }
        private double CalculateNearestDistanceToTop(FrameworkElement element, double visibleTopY, double visibleBottomY)
        {
            // Get the element's top position relative to ScrollViewer
            GeneralTransform transform = element.TransformToAncestor(CheckboxesSettings);
            Point elementPosition = transform.Transform(new Point(0, 0));
            double elementTopY = elementPosition.Y;
            double elementBottomY = elementTopY + element.ActualHeight;

            // Calculate distance to top
            double distanceToTop = Math.Abs(elementTopY - visibleTopY);

            // If the element's bottom is close to the visible bottom, return the distance to the bottom instead
            if (elementBottomY >= visibleBottomY)
            {
                double distanceToBottom = Math.Abs(elementBottomY - visibleBottomY);
                return distanceToBottom;
            }

            // Otherwise, return the distance to the top
            return distanceToTop;
        }


        private async Task HandleCheckboxLogic(CheckBox checkBox, bool? overwriteChecked)
        {
            // Determine the toggle state
            bool isChecked = overwriteChecked ?? (checkBox.IsChecked == true);

            // Use the Name property for functionality determination
            switch (checkBox.Name)
            {
                case "MinimapToggle":
                    GlobalSettings.Minimap = isChecked;
                    string minimapScript = isChecked ? "switchMinimap(true);" : "switchMinimap(false);";
                    await ExecuteJsScriptAsync(minimapScript);
                    break;

                case "TransparencyToggle":
                    GlobalSettings.Transparency = isChecked;
                    AnimateOpacity(1);
                    break;

                case "TopmostToggle":
                    GlobalSettings.Topmost = isChecked;
                    this.Topmost = GlobalSettings.Topmost;
                    break;

                case "RandomizeToggle":
                    GlobalSettings.Randomize = isChecked;
                    if (isChecked)
                    {
                        this.Title = GenerateRandomTitle();
                    }
                    else
                    {
                        this.Title = "SynapseZv3";
                    }
                    break;

                case "AutoInjectToggle":
                    GlobalSettings.AutoInject = isChecked;
    
                    break;

                default:
                    Console.WriteLine("Unknown checkbox.");
                    break;
            }
        }

        private async void CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            // Determine which checkbox triggered the event
            var checkBox = sender as CheckBox;

            if (checkBox == null) return; // Safety check

            // Call the new method with the checkbox state
            await HandleCheckboxLogic(checkBox, checkBox.IsChecked);
        }

        // Helper method to execute script with null check
        private async Task ExecuteJsScriptAsync(string script)
        {
            if (TemporyWebview.CoreWebView2 != null)
            {
                await TemporyWebview.CoreWebView2.ExecuteScriptAsync(script);
            }
            else
            {
                Console.WriteLine("CoreWebView2 is not initialized.");
            }
        }

        private void ResetHwidClick(object sender, RoutedEventArgs e)
        {
            synapseZAPI.ResetHWID();
        }
    }
}
