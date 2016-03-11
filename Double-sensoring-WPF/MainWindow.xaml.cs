﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BodyBasics
{
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
    using System.Linq;

    /// <summary>
    /// hej
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        //COLOR
        private ColorSensing colorSensing;

        //Depth
        private DepthSensing depthSensing;

        //BODY
        private BodySensing bodySensning;

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        // Create the MATLAB instance 
        MLApp.MLApp matlab = new MLApp.MLApp();

        // Current directory
        string path = Path.Combine(Directory.GetCurrentDirectory());

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;
            //BODY
            this.bodySensning = new BodySensing(kinectSensor);
            //bodySensning.createBodySensor();

            //COLOR
            this.colorSensing = new ColorSensing(kinectSensor);
            colorSensing.createColorSensor();

            //Depth
            this.depthSensing = new DepthSensing(kinectSensor);
            depthSensing.createDepthSensor();

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource1
        {
            get
            {
                return bodySensning.getImageSource();
            }
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource2
        {
            get
            {
                return colorSensing.getColorBitmap();
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (bodySensning.getBodyFrameReader() != null)
            {
                bodySensning.getBodyFrameReader().FrameArrived += bodySensning.Reader_FrameArrived;
            }

            if (colorSensing.getColorFrameReader() != null)
            {
                colorSensing.getColorFrameReader().FrameArrived += Reader_ColorFrameArrived;
            }

            if (depthSensing.getDepthFrameReader() != null)
            {
                depthSensing.getDepthFrameReader().FrameArrived += breathingDepthAverage;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (bodySensning.getBodyFrameReader() != null)
            {
                // BodyFrameReader is IDisposable
                bodySensning.getBodyFrameReader().Dispose();
                bodySensning.setBodyFrameReader(null);
            }

            if (this.colorSensing.getColorFrameReader() != null)
            {
                // ColorFrameReder is IDisposable
                this.colorSensing.getColorFrameReader().Dispose();
                this.colorSensing.setColorFrameReader(null);
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// Funktion som tar ut färgvärdena för en pixel
        /// 

       private int getcolorfrompixel(int width, int heigth, byte[] array, string color)
        {
            if ((array.Length == 8294400) && (heigth - 5 > 0) && (width - 5 > 0) && (heigth + 5 < 1080) && (width + 5 < 1920))
            {
                int startposition = (((1920 * (heigth - 1)) + width) * 4) - 1;

                if (color == "red")
                {
                return Convert.ToInt32(array.GetValue(startposition + 3)); // Röd
            }
                else if (color == "green")
                {
                    return Convert.ToInt32(array.GetValue(startposition + 2)); // Grön
                }
                else if (color == "blue")
                {
                    return Convert.ToInt32(array.GetValue(startposition + 1)); // Blå
                }
            else
            {
                return 0;
            }
        }
            else
            {
                return 0;
            }
        }

        private void ChangePixelColor(int x, int y, byte[] array, string color)
        {
            if ((array.Length == 8294400) && (y - 5 > 0) && (x - 5 > 0) && (y + 5 < 1080) && (x + 5 < 1920))
            {
                int startposition = (((1920 * (y - 1)) + x) * 4) - 1;
                if (color == "blue")
                {
                    array[startposition] = 0;
                    array[startposition + 1] = 255;
                    array[startposition + 2] = 0;
                    array[startposition + 3] = 0;
                }
                else if (color == "green")
                {
                    array[startposition] = 0;
                    array[startposition + 1] = 0;
                    array[startposition + 2] = 255;
                    array[startposition + 3] = 0;
                }
                else if (color == "red")
                {
                    array[startposition] = 0;
                    array[startposition + 1] = 0;
                    array[startposition + 2] = 0;
                    array[startposition + 3] = 255;
                }
            }
        }


        List<double> matlabPulsLista = new List<double>();

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                    
                    int width = colorFrame.FrameDescription.Width;
                    int height = colorFrame.FrameDescription.Height;

                    byte[] pixels = new byte[width * height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];

                    if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        colorFrame.CopyRawFrameDataToArray(pixels);
                    }
                    else
                    {
                        colorFrame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
                    }

                    
                    //--------------------------------------------Puls----------------------------------------------------------------------
                    if (bodySensning.getHeadJoint().JointType == JointType.Head)
                    {
                        try
                        {
                            
                            ColorSpacePoint colorSpacePoint = bodySensning.getCoordinateMapper().MapCameraPointToColorSpace(bodySensning.getHeadJoint().Position);
                            textBlock2.Text = "Huvudet befinner sig vid pixel/punkt(?): " + Math.Round(colorSpacePoint.X, 0).ToString() + ", " + Math.Round(colorSpacePoint.Y, 0).ToString();
                            

                            // Här tar vi ut alla röda värden i de intressanta pixlarna
                            List<int> rödapixlar = null;
                            rödapixlar = new List<int>();

                            for (int i = (Convert.ToInt32(Math.Round(colorSpacePoint.X)) - 10); i <= (Convert.ToInt32(Math.Round(colorSpacePoint.X)) + 10); ++i)
                            {
                                for (int j = (Convert.ToInt32(Math.Round(colorSpacePoint.Y)) - 10); j <= (Convert.ToInt32(Math.Round(colorSpacePoint.Y)) + 10); ++j)
                                {
                                    rödapixlar.Add(getcolorfrompixel(i, j, pixels, "red"));
                                    ChangePixelColor(i, j, pixels, "green");
                                }
                            }

                            // ----- Värden från gröna kanalerna, för tester
                            //Här tar vi ut alla gröna värden i de intressanta pixlarna
                            /*
                            List<int> grönapixlar = null;
                            grönapixlar = new List<int>();

                            for (int i = (Convert.ToInt32(Math.Round(colorSpacePoint.X)) - 5); i <= (Convert.ToInt32(Math.Round(colorSpacePoint.X)) + 5); ++i)
                            {
                                for (int j = (Convert.ToInt32(Math.Round(colorSpacePoint.Y)) - 5); j <= (Convert.ToInt32(Math.Round(colorSpacePoint.Y)) + 5); ++j)
                                {
                                    grönapixlar.Add(getcolorfrompixel(i, j, pixels, "green"));
                                }
                            }
                            */

                            //Filtrering av högsta/lägsta värden
                           
                            rödapixlar.Sort();
                            
                            //Ta bort alla lägsta värden
                            for (int i = 0; i < rödapixlar.Count / 5; i++)
                            {
                                rödapixlar.RemoveAt(i);
                                }

                            //Ta bort alla högsta värden
                            for (int i = (rödapixlar.Count / 5) * 4; i < rödapixlar.Count; i++)
                            {
                                rödapixlar.RemoveAt(i);
                            }

                            //Medelvärde av de röda kanalerna i intressanta pixlar
                            double redcoloraverage = 0;

                            for (int i = 0; i < rödapixlar.Count; i++)
                            {

                                redcoloraverage += rödapixlar[i];
                            }

                            redcoloraverage = (redcoloraverage / rödapixlar.Count);

                            //Medelvärde av de gröna kanalerna i intressanta pixlar
                            /*
                            double greencoloraverage = 0;

                            for (int i = 0; i < grönapixlar.Count; i++)
                            {

                                greencoloraverage += grönapixlar[i];
                            }
                            */

                            //greencoloraverage = (greencoloraverage / grönapixlar.Count);
                           
                            //rött medel minus grönt medel
                            double coloraverage = redcoloraverage; // - greencoloraverage;

                            if (matlabPulsLista.Count >= 300)
                            {
                                matlabPulsLista.RemoveAt(0);
                                matlabPulsLista.Add(coloraverage);
                            }
                            else
                            {
                                matlabPulsLista.Add(coloraverage);
                            }

                            rödapixlar.Clear();
                            //grönapixlar.Clear();
                            
                            
                        }
                        catch
                        { }
                        
                        // Change to the directory  where the function is located 
                        matlab.Execute(@"cd " + path + @"\..\..\..");

                        // Define the output 
                        object result = null;

                        // Call the MATLAB function myfunc
                        if (matlabPulsLista.Count >= 300 )
                        //
                            try
                            {
                                matlab.Feval("myfunc1", 0, out result, matlabPulsLista.ToArray());
                                for (int i = 0; i < 30; ++i)
                                {
                                    matlabPulsLista.RemoveAt(i);
                                }
                            }
                            catch (System.Runtime.InteropServices.COMException)
                            {
                                Console.WriteLine("FEL MED MATLAB PANIK");
                            }
                        //}
                    }

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorSensing.getColorBitmap().Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorSensing.getColorBitmap().PixelWidth) && (colorFrameDescription.Height == this.colorSensing.getColorBitmap().PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorSensing.getColorBitmap().BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorSensing.getColorBitmap().AddDirtyRect(new Int32Rect(0, 0, this.colorSensing.getColorBitmap().PixelWidth, this.colorSensing.getColorBitmap().PixelHeight));
                        }

                        colorSensing.getColorBitmap().WritePixels(
                            new Int32Rect(0, 0, width, height),
                            pixels,
                            width * Bgr32BytesPerPixel,
                            0);

                        this.colorSensing.getColorBitmap().Unlock();
                    }
                }
            }
        }

        //-................-.-.--.-------------------.....................
        List<float> listDepth = new List<float>();
        float average = 0;
        //------------------------------------Andning, flera punkter-------------------------------------------------------
        private void breathingDepthAverage(object sender, DepthFrameArrivedEventArgs e)
        {
            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                ushort[] pixelData = new ushort[512 * 424];

                depthFrame.CopyFrameDataToArray(pixelData);

                    //Om midSpine-jointen hittas ska andningen beräknas
                    if (bodySensning.getSpineMidJoint().JointType == JointType.SpineMid)
                    {
                        try
                        {
                            DepthSpacePoint depthSpacePoint =
                                bodySensning.getCoordinateMapper().MapCameraPointToDepthSpace(bodySensning.getSpineMidJoint().Position);

                            List<float> pixelDepthList = new List<float>();

                            //for-loop för att hämta djupvärdet i punkter utgående från midSpine
                            for (int ix = Convert.ToInt32(Math.Round(depthSpacePoint.X) - 5);
                                    ix <= Convert.ToInt32(Math.Round(depthSpacePoint.X) + 5); ix++)
                            {
                                for (int iy = Convert.ToInt32(Math.Round(depthSpacePoint.Y) - 5);
                                    iy <= Convert.ToInt32(Math.Round(depthSpacePoint.Y) + 5); iy++)
                                {
                                    pixelDepthList.Add(pixelData[((iy - 1) * 512 + ix)]);
                                }
                            }

                            //for-loop följt av division för att ta fram medelvärdet över djupvärdena
                            for (int i = 0; i < pixelDepthList.Count; i++)
                            {
                                average += pixelDepthList[i];
                            }
                            average = average / pixelDepthList.Count;

                            //lägg till i listan med alla djupvärden
                            //skicka listan om den blivit tillräckligt stor
                            if (listDepth.Count >= 30)
                            {
                                //Console.WriteLine("Average breathing depth: " + average.ToString());
                                listDepth.Clear();
                                average = 0;
                            }
                            else
                            {
                                listDepth.Add(average);
                                average = 0;
                            }
                            //kör methlab-funktionen

                        }
                        catch
                        {
                            Console.WriteLine("Felhantering i breathingDepthAverage");
                        }
                    }
                }
            }
        }
        
//------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}