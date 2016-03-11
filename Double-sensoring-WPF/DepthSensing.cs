/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;*/
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    class DepthSensing
    {
        private KinectSensor kinectSensor;

        // open the reader for the depth frames
        private DepthFrameReader depthFrameReader = null;

        public DepthSensing(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;            
            depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
        }


        public DepthFrameReader getDepthFrameReader()
        {
            return depthFrameReader;
        }

        public void setdepthFrameReader(DepthFrameReader depthFrameReader)
        {
            this.depthFrameReader = depthFrameReader;
        }

        public void setDepthFrameReader(DepthFrameReader depthFrameReader)
        {
            this.depthFrameReader = depthFrameReader;
        }

        public void createDepthSensor()
        {
            // open the reader for the color frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
        }
    }
}
