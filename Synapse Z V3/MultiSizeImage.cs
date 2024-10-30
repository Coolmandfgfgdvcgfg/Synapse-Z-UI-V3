using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Synapse_Z_V3
{
    public class MultiSizeImage : Image
    {
        private List<BitmapFrame> _availableFrames = new List<BitmapFrame>();

        static MultiSizeImage()
        {
            // Tell WPF to inform us whenever the Source dependency property is changed
            SourceProperty.OverrideMetadata(typeof(MultiSizeImage),
                    new FrameworkPropertyMetadata(HandleSourceChanged));
        }

        private static void HandleSourceChanged(
                                DependencyObject sender,
                                DependencyPropertyChangedEventArgs e)
        {
            MultiSizeImage img = (MultiSizeImage)sender;
            // Tell the instance to load all frames in the new image source
            img.UpdateAvailableFrames();
        }

        private void UpdateAvailableFrames()
        {
            _availableFrames.Clear();
            if (Source is not BitmapFrame bmFrame)
                return;

            var decoder = bmFrame.Decoder;
            if (decoder != null)
            {
                // Select one frame per size, ordered by size
                var framesInSizeOrder = from frame in decoder.Frames
                                        let frameSize = frame.PixelHeight * frame.PixelWidth
                                        group frame by frameSize into g
                                        orderby g.Key
                                        select g.OrderByDescending(GetFramePixelDepth).First();

                _availableFrames.AddRange(framesInSizeOrder);
            }
        }

        private int GetFramePixelDepth(BitmapFrame frame)
        {
            return frame.Format.BitsPerPixel;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Source == null)
            {
                base.OnRender(dc);
                return;
            }

            ImageSource src = Source;
            var ourSize = RenderSize.Width * RenderSize.Height;

            foreach (var frame in _availableFrames)
            {
                src = frame;
                if (frame.PixelWidth * frame.PixelHeight >= ourSize)
                    // Found a frame matching or exceeding the control's render size
                    break;
            }

            dc.DrawImage(src, new Rect(new Point(0, 0), RenderSize));
        }
    }
}
