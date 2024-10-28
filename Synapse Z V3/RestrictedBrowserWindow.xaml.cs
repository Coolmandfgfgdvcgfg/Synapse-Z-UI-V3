using System;
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
            string url = "https://synapse-z.netlify.app/docs/"; // Specify the URL to open
            WebView2Control.Source = new Uri(url); // Set the source of the WebView2 control
        }
    }
}
