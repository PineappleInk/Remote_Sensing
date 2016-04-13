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

        public List<double> gDrList = new List<double>();
        
        public ColorSensing(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        // Get- och setfunktioner
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

        public List<double> createPulseList(List<int> rödapixlar, List<int> grönapixlar)
        {
            // Skapar lista med (Grön / Röd):a värden
            List<double> gDr = new List<double>();

            for (int i = 0; i < rödapixlar.Count; i++)
            {
                gDr.Add((double)grönapixlar[i] / (double)rödapixlar[i]);
            }
            
            // Sortera listan i storleksordning
            gDr.Sort();
            // Tar bort första 5:e-delen av listan
            gDr.RemoveRange(0, gDr.Count / 5);
            // Tar bort sista 5:e-delen av listan
            for (int i = (gDr.Count / 4) * 2; i < gDr.Count; i++)
            {
                gDr.RemoveAt(gDr.Count - 1);
            }

            double gDrAverage = 0;
            
            for (int i = 0; i < gDr.Count; i++)
            {
                gDrAverage += gDr[i];
            }
            gDrAverage = gDrAverage / gDr.Count;

            if (gDrList.Count == 0)
            {
                gDrList.Add(new double());
            }

            gDrList.Add(gDrAverage);
            
            rödapixlar.Clear();
            grönapixlar.Clear();

            return gDrList;
        }

    }
}
