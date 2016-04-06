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
    class IRSensing
    {
        /// <summary>
        /// Reader for IR frames
        /// </summary>
        private InfraredFrameReader infraredFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap infraredBitmap = null;

        private KinectSensor kinectSensor;

        //public List<List<double>> biglist = new List<List<double>>();
        public IRSensing(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;

            // open the reader for the infrared frames
            this.infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();

            // create the infraredFrameDescription from the InfraredFrameSource
            FrameDescription infraredDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;
            
            // create the bitmap to display
            this.infraredBitmap = new WriteableBitmap(infraredDescription.Width, infraredDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        public InfraredFrameReader getInfraredFrameReader()
        {
            return infraredFrameReader;
        }

        public void setInfraredFrameReader(InfraredFrameReader infraredFrameReader)
        {
            this.infraredFrameReader = infraredFrameReader;
        }

        public WriteableBitmap getInfraredBitmap()
        {
            return infraredBitmap;
        }
    }
}