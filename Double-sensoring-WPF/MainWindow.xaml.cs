//------------------------------------------------------------------------------
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

    /// <summary>
    /// hej
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource1;

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

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;
            //BODY
            this.bodySensning = new BodySensing(kinectSensor);
            bodySensning.createBodySensor();
            // Create an image source that we can use in our image control
            this.imageSource1 = new DrawingImage(bodySensning.getDrawingGroup());

            // use the window object as the view model in this simple example
            this.DataContext = this;

            //COLOR
            this.colorSensing = new ColorSensing(kinectSensor);
            colorSensing.createColorSensor();

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
                return this.imageSource1;
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

            if (bodySensning.getBodyFrameReader() != null)
            {
                colorSensing.getColorFrameReader().FrameArrived += Reader_ColorFrameArrived;
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

       private List<string> getcolorfrompixel(int width, int heigth, byte[] array)
        {
            List<string> lista = null;
            lista = new List<string>();

            Console.WriteLine("Bredd: " + width);
            Console.WriteLine("Höjd: " + heigth);
            Console.WriteLine("Längd på array: " + array.Length);
            Console.WriteLine("Första värdet i arrayen: " + array.GetValue(0));

            if ((array.Length == 8294400) && (heigth > 0) && (width > 0))
            {
                int startposition = (((1920 * (heigth - 1)) + width) * 4) - 1;
                Console.WriteLine(startposition);
                lista.Add(array.GetValue(startposition).ToString());
                lista.Add(array.GetValue(startposition + 1).ToString());
                lista.Add(array.GetValue(startposition + 2).ToString());
                lista.Add(array.GetValue(startposition + 3).ToString());
            }
            else
            {
                lista.Add("0");
                lista.Add("0");
                lista.Add("0");
                lista.Add("0");
            }

            return lista;

        }

        private void ChangePixelColor(int x, int y, byte[] array, string color)
        {
            if ((array.Length == 8294400) && (y > 0) && (x > 0))
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


        List<double> list3 = new List<double>();

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

                    

                    if (bodySensning.headJoint.JointType == JointType.Head)
                    {
                        try
                        {
                            
                            ColorSpacePoint colorSpacePoint = bodySensning.getCoordinateMapper().MapCameraPointToColorSpace(bodySensning.headJoint.Position);
                            textBlock2.Text = "Huvudet befinner sig vid pixel/punkt(?): " + Math.Round(colorSpacePoint.X, 0).ToString() + ", " + Math.Round(colorSpacePoint.Y, 0).ToString();
                            
                            List<string> list2 = null;
                            list2 = new List<string>();

                            list2 = getcolorfrompixel(Convert.ToInt32(Math.Round(colorSpacePoint.X)), Convert.ToInt32(Math.Round(colorSpacePoint.Y)), pixels);

                            textBlock3.Text =
                                "Blå kanal: " + list2[1].ToString() +
                                System.Environment.NewLine + "Grön kanal: " + list2[2].ToString() +
                                System.Environment.NewLine + "Röd kanal: " + list2[3].ToString();
                            
                            if (list3.Count >= 300)
                            {
                                double x = Convert.ToDouble(list2[3]);
                                list3.RemoveAt(0);
                                list3.Add(x);
                            }
                            else
                            {
                                double x = Convert.ToDouble(list2[3]);
                                list3.Add(x);
                            }
                            
                            list2.Clear();
                            
                            ChangePixelColor(Convert.ToInt32(Math.Round(colorSpacePoint.X)), Convert.ToInt32(Math.Round(colorSpacePoint.Y)), pixels, "green");
                        }
                        catch
                        { }
                        
                        // Change to the directory  where the function is located 
                        matlab.Execute(@"cd " + path + @"\..\..\..");

                        // Define the output 
                        object result = null;

                        // Call the MATLAB function myfunc
                        if (list3 != null)
                        {
                            try
                            {
                                matlab.Feval("myfunc1", 0, out result, list3.ToArray());
                            }
                            catch (System.Runtime.InteropServices.COMException)
                            {
                            }
                        }
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
