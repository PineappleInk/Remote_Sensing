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
    using System.Windows.Forms.DataVisualization.Charting;
    using Microsoft.Kinect;
    using System.Linq;
    using System.Drawing;
    using MathNet.Filtering;
    using System.Windows.Resources;
     
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Paramteter for position of bellyJoint
        /// </summary>
        private double bellyJointYPosition = 2 / 3;
        private double bellyJointXPosition = 2 / 3;
        int lowNumPulse = 30;
        int lowNumBreathing = 10;
        //double heartrate = 0;
        double average = 0;
        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        //COLOR-instans
        private ColorSensing colorSensing;

        //DEPTH-instans
        private DepthSensing depthSensing;

        //IR-instans
        //private IRSensing irSensing;

        //BODY-instans
        private BodySensing bodySensning;

        ////----------------------------------------Våra egna---------------------
        ///Matlab-variabler
        /// Current directory
        string path = Path.Combine(Directory.GetCurrentDirectory());

        //MATLAB-instans 
        //MLApp.MLApp matlab = new MLApp.MLApp();

        //Puls
        List<double> matlabPulsLista = new List<double>();

        //Andning
        /// <summary>
        /// listDepthMatlab - lista som skickas till matlab
        /// calculatedBreaths - lista med uträknade frekvenser från matlab, medelvärdesbildas och visas i användargränssnittet
        ///                     30 värden samlas in och medelvärdet visas för användaren (se matlabCommand)
        /// </summary>
        List<double> listDepthMatlab = new List<double>();
        List<double> calculatedBreaths = new List<double>();

        //TEST Intensity
        List<double> listIntensity = new List<double>();

        //Filter
        int orderOfFilter = 27;
        int timeOfMeasurement = 900;
        int runPlotModulu = 5;
        int fps = 30;
        OnlineFilter bpFiltBreath = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, 30, 6/60, 60/60, 27);
        OnlineFilter bpFiltPulse = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, 30, 40/60, 180/60, 27);
        //----------------------------------------------------------------------------------------

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        //PixelFormat format = PixelFormats.Bgr32; // BEHÖVS DENNA?!?!?!
        
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

            //Depth
            this.depthSensing = new DepthSensing(kinectSensor);

            //IR
            //this.irSensing = new IRSensing(kinectSensor);

            // initialize the components (controls) of the window
            this.InitializeComponent();
            
            //Om man vill rendera hela tiden!
            //CompositionTarget.Rendering += CompositionTargetRendering;

            Console.WriteLine("Här är vi! " + path);
        }

        //private void CompositionTargetRendering() //object sender, EventArgs e
        //{
            //BitmapImage _image = new BitmapImage();
            //_image.BeginInit();
            //_image.CacheOption = BitmapCacheOption.None;
            //_image.UriCachePolicy = new System.Net.Cache.RequestCachePolicy();
            //_image.CacheOption = BitmapCacheOption.OnLoad;
            //_image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            //_image.UriSource = new Uri(path + @"\..\..\..\matlab\pulseplot.png", UriKind.RelativeOrAbsolute);
            //_image.EndInit();
            //image1.Source = _image;
        //}
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

        //public ImageSource ImageSource3
        //{
        //    get
        //    {
        //        return irSensing.getInfraredBitmap();
        //    }
        //}


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

            //if (irSensing.getInfraredFrameReader() != null)
            //{
            //    irSensing.getInfraredFrameReader().FrameArrived += Reader_IR;
            //}
        }

        //Försök att få in infraröd sensor
        //private ImageSource ToBitmap(InfraredFrame frame)
        //{
        //    int width = frame.FrameDescription.Width;
        //    int height = frame.FrameDescription.Height;

        //    ushort[] infraredData = new ushort[width * height];
        //    byte[] pixels = new byte[width * height * Bgr32BytesPerPixel];
        //    byte[] pixelData = new byte[width * height * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8];

        //    frame.CopyFrameDataToArray(infraredData);

        //    int colorIndex = 0;
        //    for (int infraredIndex = 0; infraredIndex < infraredData.Length; ++infraredIndex)
        //    {
        //        ushort ir = infraredData[infraredIndex];
        //        byte intensity = (byte)(ir >> 8);

        //        pixelData[colorIndex++] = intensity; // Blue
        //        pixelData[colorIndex++] = intensity; // Green   
        //        pixelData[colorIndex++] = intensity; // Red

        //        ++colorIndex;
        //    }

        //    int stride = width * format.BitsPerPixel / 8;

        //    //Console.WriteLine(pixels[0] + pixelData[0] + " Nästa: " + pixels[1] + pixelData[1]);

        //    return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        //}

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

            if (this.depthSensing.getDepthFrameReader() != null)
            {
                // ColorFrameReder is IDisposable
                this.depthSensing.getDepthFrameReader().Dispose();
                this.depthSensing.setDepthFrameReader(null);
            }

            //if (this.irSensing.getInfraredFrameReader() != null)
            //{
            //    this.irSensing.getInfraredFrameReader().Dispose();
            //    this.irSensing.setInfraredFrameReader(null);
            //}

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        //Lokalisera toppen i lista för andning
        //Returvärdet är en lista med listor för [0] - positioner och [1] - värde i respektive position som innehåller toppar (alltså från tidsaxeln)
        private List<List<double>> locatePeaksBreath(List<double> measurements)
        {
            List<List<double>> topLocations = new List<List<double>>();
            topLocations.Add(new List<double>());
            topLocations.Add(new List<double>());
            int upCounter = 0;
            int downCounter = 0;

            for (int i = 0; i < measurements.Count - 4; i++)
            {
                //Påväg uppåt
                if (measurements[i] < measurements[i + 1])
                {
                    if (downCounter < 5)
                    {
                        upCounter += 1;
                        downCounter = 0;
                    }
                }
                //Vid topp
                else if (measurements[i] > (measurements[i + 1] + measurements[i + 2] + measurements[i + 3] + measurements[i + 4]) / 4)
                {
                    if (upCounter > 15)
                    {
                        topLocations[0].Add(Convert.ToDouble(i));
                        topLocations[1].Add(measurements[i]);
                        upCounter = 0;
                        downCounter = 1;
                    }
                }
                //Påväg nedåt
                else if (measurements[i] > measurements[i + 1])
                {
                    if (upCounter < 5)
                    {
                        downCounter += 1;
                        upCounter = 0;
                    }
                }
                //Vid dal
                else if (measurements[i] < (measurements[i + 1] + measurements[i + 2] + measurements[i + 3] + measurements[i + 4]) / 4)
                {
                    if (downCounter > 15)
                    {
                        upCounter = 0;
                        downCounter = 0;
                    }
                }
            }
            return topLocations;
        }

        //Lokalisera topparna i lista för puls
        // Returvärdet är en lista med listor för [0]=positioner och 
        // [1]=toppvärde till respektive position av topp (alltså från tidsaxeln)
        private List<List<double>> locatePeaksPulse(List<double> measurements)
        {
            List<List<double>> topLocations = new List<List<double>>();
            topLocations.Add(new List<double>());
            topLocations.Add(new List<double>());
            int upCounter = 0;
            int downCounter = 0;

            for (int i = 0; i < measurements.Count - 4; i++)
            {
                //Påväg uppåt
                if (measurements[i] < measurements[i + 1])
                {
                    if (downCounter < 4) //om det inte gått nedåt i max 0,1 sekunder kan det gå uppåt
                    {
                        upCounter += 1;
                        downCounter = 0;
                    }
                }
                //Vid topp
                else if (measurements[i] > (measurements[i + 1] + measurements[i + 2] + measurements[i + 3] + measurements[i + 4]) / 4)
                {
                    if (upCounter > 4)
                    {
                        topLocations[0].Add(Convert.ToDouble(i)); //Positionen läggs till i listan
                        topLocations[1].Add(measurements[i]); // Värdet på toppen läggs till i listan
                        upCounter = 0;
                        downCounter = 0;
                    }
                }
                //Påväg nedåt
                else if (measurements[i] > measurements[i + 1])
                {
                    if (upCounter < 4)
                    {
                        downCounter += 1;
                        upCounter = 0;
                    }
                }
                //Vid dal
                else if (measurements[i] < (measurements[i + 1] + measurements[i + 2] + measurements[i + 3] + measurements[i + 4]) / 4)
                {
                    if (downCounter > 4)
                    {
                        // Reset
                        upCounter = 0;
                        downCounter = 0;
                    }
                }
            }
            return topLocations;
        }





        /// Härifrån körs alla kommandon som har med signalbehandling och detektion av frekvenser att göra.
        /// <param name="codeString">definierar detektion av puls ("pulse") eller andning ("breathing") som en sträng</param>
        /// <param name="measurements">innehåller all mätdata i form av en lista med floats</param>
        int antalFel = 0;
        private void matlabCommand(string codeString, List<double> measurements = null, List<List<double>> rgbList = null)
        {
            // Define the output 
            object result = null;
            try
            {
                // Analys av puls
                if (codeString == "pulse")
                {
                    // Filtrering
                    double[] measurementsFilt = bpFiltPulse.ProcessSamples(rgbList[1].ToArray());
                    List<double> measurementsFiltList = measurementsFilt.ToList();

                    if (measurementsFiltList.Count > orderOfFilter)
                    {
                        measurementsFiltList.RemoveRange(0, orderOfFilter);
                    }

                    chartPulse.CheckAndAddSeriesToGraph("Pulse", "fps");
                    chartPulse.CheckAndAddSeriesToGraph("Pulsemarkers", "marker");
                    chartPulse.ClearCurveDataPointsFromGraph();

                    //Toppdetektering
                    if (measurementsFiltList.Count > 10)
                    {
                        List<List<double>> peaks = new List<List<double>>();
                        peaks = locatePeaksPulse(measurementsFiltList);
                        for (int i = 0; i < peaks[0].Count(); i++)
                        {
                            chartPulse.AddPointToLine("Pulsemarkers", peaks[1][i], peaks[0][i]);
                        }


                        //Average är antalet pulsslag under 60 sekunder
                        average = peaks[0].Count() * 60 * fps / timeOfMeasurement;

                        //Skriver ut pulspeakar i programmet
                        textBlock.Text = "Antal peaks i puls: " + System.Environment.NewLine + peaks[0].Count()
                            + System.Environment.NewLine + "Uppskattad BPM: " + average;

           
                        pulseAlarm(average, lowNumPulse);
                    }

                    for (int i = 0; i < measurementsFiltList.Count(); i++)
                    {
                        chartPulse.AddPointToLine("Pulse", measurementsFiltList[i], i);
                    }

                    if (rgbList[0].Count() >= timeOfMeasurement + orderOfFilter)
                    {
                        rgbList[0].RemoveRange(0, runPlotModulu);
                        rgbList[1].RemoveRange(0, runPlotModulu);
                        rgbList[2].RemoveRange(0, runPlotModulu);
                    }
                }

                //Analys av andning
                else if (codeString == "breathing")
                {
                    // Filtrering av djupvärden (andning)
                    double[] measurementsFilt = bpFiltBreath.ProcessSamples(measurements.ToArray());
                    List<double> measurementsFiltList = measurementsFilt.ToList();

                    if (measurementsFiltList.Count > orderOfFilter)
                    {
                        measurementsFiltList.RemoveRange(0, orderOfFilter);
                    }

                    chartBreath.CheckAndAddSeriesToGraph("Breath", "fps");
                    chartBreath.CheckAndAddSeriesToGraph("Breathmarkers", "marker");
                    chartBreath.ClearCurveDataPointsFromGraph();

                    // Toppdetektering
                    if (measurementsFiltList.Count > 10)
                    {
                        List<List<double>> peaks = new List<List<double>>();
                        peaks = locatePeaksBreath(measurementsFiltList);

                        // Rita ut peakar i andningen (= utandning)
                        for (int i = 0; i < peaks[0].Count(); i++)
                        {
                            chartBreath.AddPointToLine("Breathmarkers", peaks[1][i], peaks[0][i]);

                        }

                        // Average är antalet peakar * 3 (20 sek) till larmet.
                        average = peaks[0].Count() * fps * 60 / timeOfMeasurement;

                        // Ritar ut andningspeakar i programmet
                        averageBreathingTextBlock.Text = "Antal peaks i andning: " + System.Environment.NewLine + peaks[0].Count()
                               + Environment.NewLine + "Uppskattad BPM: " + average;

                        breathingAlarm(average, lowNumBreathing);

                    }

                    for (int i = 0; i < measurementsFiltList.Count(); i++)
                    {
                        chartBreath.AddPointToLine("Breath", measurementsFiltList[i], i);
                    }

                    if(measurements.Count() >= timeOfMeasurement + orderOfFilter)
                    {
                        listDepthMatlab.RemoveRange(0, runPlotModulu);
                    }

                }
                else if (codeString == "Intensity")
                {
                /*    //filtrering
                    double[] measurementsFilt = bpFiltPulse.ProcessSamples(measurements.ToArray());
                    List<double> measurementsFiltList = measurementsFilt.ToList();

                    measurementsFiltList.RemoveRange(0, 10);

                    //toppdetektering
                    if (measurementsFiltList.Count > 100)
                    {
                        List<List<double>> peaks = new List<List<double>>();
                        peaks = locatePeaksBreath(measurementsFiltList);

                      //Skriver ut andningspeakar i programmet
                        textBlock6.Text = "Antal peaks i intensitet: " + System.Environment.NewLine + peaks[0].Count()
                               + System.Environment.NewLine + "Uppskattad BPM: " + peaks[0].Count() * 3;

                    }

                    chartIntensity.CheckAndAddSeriesToGraph("Intensity", "fps");
                    chartIntensity.ClearCurveDataPointsFromGraph();

                    for (int i = 0; i < measurementsFiltList.Count(); i++)
                    {
                        chartIntensity.AddPointToLine("Intensity", measurementsFiltList[i], i);
                    }
                    if (measurements.Count() >= 610)
                    {
                        listIntensity.RemoveRange(0, 10);
                    }*/
                }
                else
                {
                    Console.WriteLine("Matlabfunktionen kördes inte, kontrollera att codeString var korrekt");
                }              

            }
            catch
            {
                antalFel += 1;
                Console.WriteLine("Antal kastade fel: " + antalFel.ToString());
            }

        }

        //Larm för andning
        private void breathingAlarm(double averageBreathing, int lowNum)
        {
            if (averageBreathing < lowNum)
            {
                if (!checkBoxSound.HasContent)
                {
                    Console.WriteLine("Det fanns inget värde i checkBoxSound");
                }
                if ((bool)checkBoxSound.IsChecked)
                {
                inputTextBreathing.Background = System.Windows.Media.Brushes.Red;
                string soundpath = Path.Combine(path + @"\..\..\..\beep-07.wav");
                System.Media.SoundPlayer beep = new System.Media.SoundPlayer();
                beep.SoundLocation = soundpath;
                beep.Play();
            }
                else
                {
                    inputTextBreathing.Background = System.Windows.Media.Brushes.Red;                    
                }

            }
            else inputTextBreathing.Background = System.Windows.Media.Brushes.White;
        }

        //Larm för pulsen
        private void pulseAlarm(double averagePulse, int lowNum)
        {
            if (averagePulse < lowNum)
            {
                if (!checkBoxSound.HasContent)
                {
                    Console.WriteLine("Det fanns inget värde i checkBoxSound");
                }
                if ((bool)checkBoxSound.IsChecked)
                {
                inputTextPulse.Background = System.Windows.Media.Brushes.Red;
                string soundpath = Path.Combine(path + @"\..\..\..\beep-07.wav");
                System.Media.SoundPlayer beep = new System.Media.SoundPlayer();
                beep.SoundLocation = soundpath;
                beep.Play();
            }
                else
                {
                    inputTextPulse.Background = System.Windows.Media.Brushes.Red;                    
                }
            }
            else inputTextPulse.Background = System.Windows.Media.Brushes.White;
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

                    byte[] pixels = new byte[width * height * Bgr32BytesPerPixel];

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
                            ColorSpacePoint colorSpaceHeadPoint = bodySensning.getCoordinateMapper().
                                MapCameraPointToColorSpace(bodySensning.getHeadJoint().Position);


                            textBlock2.Text = "Huvudet befinner sig vid pixel/punkt(?): " +
                                Math.Round(colorSpaceHeadPoint.X, 0).ToString() + ", " + Math.Round(colorSpaceHeadPoint.Y, 0).ToString();

                            // ----- Värden från gröna kanalerna, för tester
                            //Här tar vi ut alla gröna värden i de intressanta pixlarna

                            List<int> grönapixlar = null;
                            grönapixlar = new List<int>();

                            List<int> blåapixlar = null;
                            blåapixlar = new List<int>();

                            // Här tar vi ut alla röda värden i de intressanta pixlarna
                            List<int> rödapixlar = null;
                            rödapixlar = new List<int>();

                            // Rutan som följer HeadJoint
                            for (int i = (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) - 40);
                                i <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) + 40); ++i)
                            {
                                for (int j = (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) - 30);
                                    j <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) + 60); ++j)
                                {
                                    int r = getcolorfrompixel(i, j, pixels, "red");
                                    int g = getcolorfrompixel(i, j, pixels, "green");
                                    int b = getcolorfrompixel(i, j, pixels, "blue");

                                    if ((0 < r && r < 255) && (0 < g && g < 255) && (0 < b && b < 255))
                                    {
                                        rödapixlar.Add(r);
                                        grönapixlar.Add(g);
                                        blåapixlar.Add(b);
                                    }
                                    ChangePixelColor(i, j, pixels, "green");
                                }
                            }

                            List<List<double>> biglist = colorSensing.createBigList2(rödapixlar, grönapixlar, blåapixlar);
                            

                            // här ska methlab-funktionen köras--------------------^*************************^^,
                            //definiera hur ofta och hur stor listan är här innan.
                            if (biglist[0].Count % runPlotModulu == 0)
                            {
                                //Analys av puls i matlab
                                matlabCommand("pulse", listDepthMatlab, biglist);
                            }
                        }
                        catch
                        { }

                    }

                    if (bodySensning.getSpineMidJoint().JointType == JointType.SpineMid)
                    {
                        try
                        {
                            // Rutan som följer SpineMidJoint
                            bodySensning.setBellyJointPos(bodySensning.getSpineMidJoint().Position.X +
                                (bodySensning.getSpineBase().Position.X - bodySensning.getSpineMidJoint().Position.X) * (float)bellyJointXPosition,
                                bodySensning.getSpineMidJoint().Position.Y +
                                (bodySensning.getSpineBase().Position.Y - bodySensning.getSpineMidJoint().Position.Y) * (float)bellyJointYPosition,
                                bodySensning.getSpineMidJoint().Position.Z);

                            ColorSpacePoint colorSpaceSpinePoint = bodySensning.getCoordinateMapper().
                            MapCameraPointToColorSpace(bodySensning.getBellyJoint().Position);

                            if (colorSpaceSpinePoint.X > 0)
                            {
                                for (int i = (Convert.ToInt32(Math.Round(colorSpaceSpinePoint.X)) - 10);
                                 i <= (Convert.ToInt32(Math.Round(colorSpaceSpinePoint.X)) + 10); ++i)
                                {
                                    for (int j = (Convert.ToInt32(Math.Round(colorSpaceSpinePoint.Y)) - 10);
                                        j <= (Convert.ToInt32(Math.Round(colorSpaceSpinePoint.Y)) + 10); ++j)
                                    {
                                        ChangePixelColor(i, j, pixels, "red");
                                    }
                                }
                            }
                        }
                        catch
                        { }
                    }

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorSensing.getColorBitmap().Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorSensing.getColorBitmap().PixelWidth) &&
                            (colorFrameDescription.Height == this.colorSensing.getColorBitmap().PixelHeight))
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

        //------------------------------------IR, test--------------------------------------------------------------------
        //private void Reader_IR(object sender, InfraredFrameArrivedEventArgs e)
        //{
        //    using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
        //    {
        //        if (infraredFrame != null)
        //        {
        //            ushort[] pixelData = new ushort[infraredFrame.FrameDescription.Width * infraredFrame.FrameDescription.Height];

        //            infraredFrame.CopyFrameDataToArray(pixelData);
        //            //Console.WriteLine(pixelData[0] + " Nästa: " + pixelData[1] + " Ännu en nästa: " + pixelData[1] + " Längd: " + pixelData.Length);

        //            if (bodySensning.getHeadJoint().JointType == JointType.Head)
        //            {
        //                try
        //                {
        //                    listIntensity.Add(depthSensing.createDepthListAvarage(bodySensning.getCoordinateMapper(), bodySensning.getHeadJoint(), pixelData));
                            
        //                    if (listIntensity.Count % 10 == 0)
        //                    {
        //                        matlabCommand("Intensity", listIntensity);
        //                    }
        //                }

        //                catch (System.IndexOutOfRangeException)
        //                {
        //                    System.Windows.MessageBox.Show("Baby's head has escaped, baby can't be far");
        //                }
        //            }
        //        }
        //    }
        //}


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


                            //Jämför med en stationär Joint för att eliminera icke-andningesrelaterade rörelser
                            //OBS spineShoulder eller dylikt måste skapas
                            /*DepthSpacePoint depthSpacePointCompare = 
                                bodySensning.getCoordinateMapper().MapCameraPointToDepthSpace(bodySensning.getSpineShoulderJoint().Position);
                            double jointCompare = pixelData[Convert.ToInt32(Math.Round((depthSpacePointCompare.Y - 1) * 512 + depthSpacePointCompare.X))];*/

                            listDepthMatlab.Add(depthSensing.createDepthListAvarage(bodySensning.getCoordinateMapper(), bodySensning.getBellyJoint(), pixelData));

                            //NYTT
                            textBlock5.Text = "Element i andningslistan: " + listDepthMatlab.Count;

                            //lägg till average i listan med alla djupvärden
                            //skicka listan om den blivit tillräckligt stor
                            if (listDepthMatlab.Count % runPlotModulu == 0)
                            {
                                matlabCommand("breathing", listDepthMatlab);
                            }
                            //if (listDepthMatlab.Count >= 300)
                            //{
                            //    listDepthMatlab.RemoveRange(0, 30);
                            //}
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            System.Windows.MessageBox.Show("Baby has escaped, baby can't be far");
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
        //Slider som ändrar positionen på bellyJointen
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.bellyJointYPosition = Slider.Value;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            listDepthMatlab.Clear();
            colorSensing.biglist.Clear();
        }

        private void retrieveInputPulse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int inputnumber = Convert.ToInt32(inputTextPulse.Text);
                if (inputnumber >= 30 && inputnumber <= 200)
                {
                    lowNumPulse = inputnumber;
                }
                else inputTextPulse.Text = Convert.ToString(lowNumPulse);
            }
            catch (System.FormatException)
            {
                inputTextPulse.Text = Convert.ToString(lowNumPulse);
            }
        
        }

        private void retrieveInputBreathing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int inputnumber = Convert.ToInt32(inputTextBreathing.Text);
                if (inputnumber >= 2 && inputnumber <= 40)
                {
                    lowNumBreathing = inputnumber;
                }
                else inputTextBreathing.Text = Convert.ToString(lowNumBreathing);
            }
            catch (System.FormatException)
            {
                inputTextBreathing.Text = Convert.ToString(lowNumBreathing);
            }
        }
    }
}
