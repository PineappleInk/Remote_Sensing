using System;
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
using System.Windows.Media.Imaging;
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

            // open the reader for the color frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
        }

        // Get- och setfunktioner
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

        public List<double> createDepthListAvarage(CoordinateMapper coordinateMapper, Joint bellyJoint, ushort[] pixelData)
        {
            double average = 0;
            List<double> pixelDepthList = new List<double>();
            List<double> listDepthMatlab = new List<double>();

            DepthSpacePoint depthSpacePoint =
                coordinateMapper.MapCameraPointToDepthSpace(bellyJoint.Position);
            //for-loop för att hämta djupvärdet i punkter utgående från midSpine
            for (int ix = (int)depthSpacePoint.X - 10;
                    ix <= (int)depthSpacePoint.X + 10; ix++)
            {
                for (int iy = (int)depthSpacePoint.Y - 10;
                    iy <= (int)depthSpacePoint.X + 10; iy++)
                {
                    pixelDepthList.Add(pixelData[((iy - 1) * 512 + ix)]);
                }
            }

            pixelDepthList.Sort();

            //Filtrera pixelDepthList från extremvärden
            if (pixelDepthList.Count > 0)
            {
                for (int i = 0; i < pixelDepthList.Count / 3; i++)
                {
                    pixelDepthList.RemoveAt(0);
                }
                for (int i = pixelDepthList.Count * (2 / 3); i <= pixelDepthList.Count; i++)
                {
                    pixelDepthList.RemoveAt(pixelDepthList.Count - 1);
                }
            }
            //for-loop följt av division för att ta fram medelvärdet över de intressanta djupvärdena
            for (int i = 0; i < pixelDepthList.Count; i++)
            {
                average += pixelDepthList[i];
            }
            average = average / pixelDepthList.Count;

            //lägg till average i listan med alla djupvärden
            //skicka listan om den blivit tillräckligt stor
            if (listDepthMatlab.Count >= 900)
            {
                listDepthMatlab.RemoveAt(0);
                listDepthMatlab.Add(average);
            }
            else
            {
                listDepthMatlab.Add(average);
            }

            if (listDepthMatlab.Count >= 900)
            {
                for (int i = 0; i < 30; ++i)
                {
                    listDepthMatlab.RemoveAt(0);
                }
            }
            return listDepthMatlab;
        }
    }
}
