using System;
using System.IO;
using System.Windows;

namespace Synapse_Z_V3
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent(); // Initialize the XAML components
            Loaded += WebView2BrowserWindow_Loaded; // Handle Loaded event
        }

        private async void WebView2BrowserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await WebView2Control.EnsureCoreWebView2Async(null); // Ensure the control is initialized

            // Get the path to the local HTML file
            string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "index.html");
            Uri localFileUri = new Uri(localFilePath); // Create a URI for the local file

            WebView2Control.Source = localFileUri; // Set the source of the WebView2 control
        }
    }
}
