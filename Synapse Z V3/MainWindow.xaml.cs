using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Synapse_Z_V3
{
    public partial class MainWindow : Window
    {
        private bool _isResizing = false; // Track if resizing is active
        private Point _startPoint;

        public MainWindow()
        {
            InitializeComponent();
            this.MinWidth = 600;  // Set minimum width
            this.MinHeight = 300; // Set minimum height
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
                this.Width = Math.Max(200, mousePos.X);
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
    }
}
