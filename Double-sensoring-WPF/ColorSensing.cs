using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    class ColorSensing
    {

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        private KinectSensor kinectSensor;

        public ColorSensing(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;
        }

        public ColorFrameReader getColorFrameReader()
        {
            return colorFrameReader;
        }

        public void setColorFrameReader(ColorFrameReader colorFrameReader)
        {
            this.colorFrameReader = colorFrameReader;
        }

        public WriteableBitmap getColorBitmap()
        {
            return colorBitmap;
        }

        public void createColorSensor()
        {
            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }
        
    }
}
