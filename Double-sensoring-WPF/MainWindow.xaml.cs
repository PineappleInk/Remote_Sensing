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
    using System.Windows.Markup;
    using System.Windows.Data;
    using System.Windows.Input;

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
        public double bellyJointYPosition = 1 / 2.1; //Närmare 1 flyttar punkten nedåt
        private double bellyJointXPosition = 1;
        //double heartrate = 0;
        //double average = 0;
        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        //COLOR-instans
        private ColorSensing colorSensing;

        //DEPTH-instans
        private DepthSensing depthSensing;

        //IR-instans
        private IRSensing irSensing;

        //BODY-instans
        private BodySensing bodySensning;

        //SettingWindow-instans
        public SettingWindow settingWindow;

        //IntroPineapple-instans
        private IntroPineapple introPineapple;

        ////----------------------------------------Pinapple Inc: kod ---------------------
        //Bakgrund
        private System.Windows.Media.Brush bgBrush;

        ////----------------------------------------Våra egna---------------------
        /// Current directory
        string path = Path.Combine(Directory.GetCurrentDirectory());

        //Andning
        /// <summary>
        /// depthList - Lista som innehåller djupvärdena
        /// </summary>
        public List<double> depthList = new List<double>(); //OBS gör privat
        List<double> calculatedBreaths = new List<double>();

        /*Globala variabler*/

        // Excel-flagga för heart-rate variability
        bool heartRateVariabilityFlag = false;

        // Standardavvikelse höjd peakar, medel över 5 max minuter
        List<double> stdMeanLst = new List<double>();
        double stdMean = 0;
        // Standardavvikelse höjd peakar, senaste 10 sekunder
        double stdH10 = 0;

        // Info om mätdata
        static int secondsOfMeasurement = 60;          //Anger över hur många sekunder vi ska mäta
        static int fps = 30;                           //Frames Per Second (Antalet bilder/sekund)
        static int samplesOfMeasurement =
            secondsOfMeasurement * fps;                //Över hur många bilder vi ska mäta (sekunder * fps)
        static int runPlotModulo = 5;                  //Hur ofta plottarna ska köras (anges som antalet bilder som ska gå emellan plottningen)
        static int plotOverSeconds = 10;               //Anger över hur många sekunder plottarna ska visas

        // Alarmparametrar
        public int lowNumPulse = 30; //OBS gör privata
        public int lowNumBreathing = 10;

        //Listor för beräkningar för larm
        static int breathingWarningInSeconds = 40;     //Anger över hur många sekunder som beräkningen av andningen ska ske
        static int pulseWarningInSeconds = 10;         //Anger över hur många sekunder som beräkningen av pulsen ska ske
        static int startBreathingAfterSeconds = 40;    //Anger efter hur många sekunder som beräkningar och plottning av andning ska ske
        static int startPulseAfterSeconds = 20;        //Anger efter hur många sekunder som beräkningar och plottning av pulsen ska ske

        static double minimiDepthBreath = 3;         //Anger det minsta djup som andningen måste variera för att upptäckas av peakdetektionen

        static int dotSize = 20;                       //Anger ramstorleken på kvadraterna vid mage och huvud

        //Filter
        static int orderOfFilter = 27;
        OnlineFilter bpFiltBreath = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, 30, 6 / 60, 60 / 60, orderOfFilter);
        OnlineFilter bpFiltPulse = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, 30, 40 / 60, 180 / 60, orderOfFilter);

        //Timer
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        bool heartDecreasing = true;

        System.Windows.Threading.DispatcherTimer lungTimer = new System.Windows.Threading.DispatcherTimer();
        bool lungDecreasing = true;

        double heartPulse = 60;
        double breathRate = 30;

        //Ljud
        string nightSoundPath;
        System.Media.SoundPlayer nightSound = new System.Media.SoundPlayer();

        string daySoundPath;
        System.Media.SoundPlayer daySound = new System.Media.SoundPlayer();


        //----------------------------------------------------------------------------------------

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            //Programmet börjar med en intro-ananas
            this.Hide();
            this.introPineapple = new IntroPineapple(Path.Combine(path + @"\..\..\..\pineapple.wav"));
            this.introPineapple.Show();

            //Timer start - använda dispathertimer till introt
            dispatcherTimer.Tick += introPineappleSpin;
            dispatcherTimer.Interval = new TimeSpan(10000);
            dispatcherTimer.Start();

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

            //COLOR
            this.colorSensing = new ColorSensing(kinectSensor);

            //Depth
            this.depthSensing = new DepthSensing(kinectSensor);

            //IR
            this.irSensing = new IRSensing(kinectSensor, this);

            // initialize the components (controls) of the window
            this.InitializeComponent();

            //SetingWindow
            this.settingWindow = new SettingWindow(this);

            //Bakgrund
            this.bgBrush = this.Background;

            //Ljud
            nightSoundPath = Path.Combine(path + @"\..\..\..\nighttime.wav");
            nightSound.SoundLocation = nightSoundPath;

            daySoundPath = Path.Combine(path + @"\..\..\..\daytime.wav");
            daySound.SoundLocation = daySoundPath;
        }

        // -------------------------------- Pineapple Inc: kod ----------------------------------------------------
        //Intro-ananas ska snurra 720 grader
        private void introPineappleSpin(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            int angle = this.introPineapple.spinPineapple();

            if (angle >= 720)
            {
                //Visa programmet istället för intro-ananasen
                this.introPineapple.Close();
                this.Show();

                //Välkomnande röst
                string greetingsPath = Path.Combine(path + @"\..\..\..\welcome.wav");
                System.Media.SoundPlayer greeting = new System.Media.SoundPlayer();
                greeting.SoundLocation = greetingsPath;
                greeting.Play();

                //Timer start
                dispatcherTimer.Tick -= introPineappleSpin;
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(500000);
                dispatcherTimer.Start();

                //Timer start
                lungTimer.Tick += lungTimer_Tick;
                lungTimer.Interval = new TimeSpan(500000);
                lungTimer.Start();
            }
            else
            {
                dispatcherTimer.Start();
            }
        }

        public void setBellyJointYPosition(double v)
        {
            this.bellyJointYPosition = v;
        }
        // -----------------------------------------------------------------------------------------------------------

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

        public ImageSource ImageSource3
        {
            get
            {
                return irSensing.ImageSource;
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

            if (irSensing.getInfraredFrameReader() != null)
            {
                irSensing.getInfraredFrameReader().FrameArrived += irSensing.Reader_InfraredFrameArrived;
            }

            chartPulse.Visibility = Visibility.Hidden;
            chartBreath.Visibility = Visibility.Hidden;
            heart2.Visibility = Visibility.Hidden;
            lung3.Visibility = Visibility.Hidden;
            heartrateTextBlock.Visibility = Visibility.Hidden;
            breathrateTextBlock.Visibility = Visibility.Hidden;
            textBlock1.Visibility = Visibility.Hidden;
            textBlock1_Copy.Visibility = Visibility.Hidden;
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

            if (this.depthSensing.getDepthFrameReader() != null)
            {
                // ColorFrameReder is IDisposable
                this.depthSensing.getDepthFrameReader().Dispose();
                this.depthSensing.setDepthFrameReader(null);
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            System.Environment.Exit(1);
        }

        // --------------------------------- Pineapple Inc: kod ----------------------------------------------------

        // Lokalisera toppen i lista för andning
        // Returvärdet är en lista med listor för [0] - positioner och 
        // [1] - värde i respektive position som innehåller toppar (alltså från tidsaxeln)
        private List<List<double>> locatePeaksBreath(List<double> measurements)
        {
            int upCounter = 0;
            int downCounter = 0;

            // Lista för peakar
            List<List<double>> topLocations = new List<List<double>>();
            topLocations.Add(new List<double>()); //[0] Topparnas position
            topLocations.Add(new List<double>()); //[1] Topparnas värden

            for (int i = 0; i < measurements.Count - 4; i++)
            {
                //Påväg uppåt
                if (measurements[i] < measurements[i + 1])
                {
                    if (downCounter < 4)
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
                        topLocations[0].Add(Convert.ToDouble(i));
                        topLocations[1].Add(measurements[i]);
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
            }
            return topLocations;
        }

        // Lokalisera dalar i lista för andning
        // Returvärdet är en lista med listor för [0] - positioner och 
        // [1] - värde i respektive position som innehåller dalar (alltså från tidsaxeln)
        private List<List<double>> locateValleysBreath(List<double> measurements)
        {
            int upCounter = 0;
            int downCounter = 0;
            // Lista för dalar
            List<List<double>> valleyLocations = new List<List<double>>();
            valleyLocations.Add(new List<double>()); //[0] Dalarnas position(x-värden)
            valleyLocations.Add(new List<double>()); //[1] Dalarnas värden  (y-värden)

            for (int i = 0; i < measurements.Count - 4; i++)
            {
                //Påväg nedåt
                if (measurements[i] > measurements[i + 1])
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
                        upCounter = 0;
                        downCounter = 0;
                        // Lägger endast till dalar i listan                      
                        valleyLocations[0].Add(Convert.ToDouble(i));
                        valleyLocations[1].Add(measurements[i]);
                    }
                }

                //Påväg uppåt
                else if (measurements[i] < measurements[i + 1])
                {
                    if (downCounter < 4)
                    {
                        upCounter += 1;
                        downCounter = 0;
                    }
                }
            }
            return valleyLocations;
        }

        // Returnerar double-lista med tider mellan alla närliggande toppar.
        private List<double> timeBetweenAllPeaks(List<List<double>> correctPeaksPulse)
        {
            // Toppar i andningsdjupet (korrekta)
            List<List<double>> peaks = correctPeaksPulse; // [0]=xPos, [1]=yPos

            // Antal peakar
            int numOfPeaks = peaks[0].Count;

            // Tiderna mellan topparna - Ny lista
            List<double> timeBwPeaks = new List<double>();

            for (int i = 0; i < numOfPeaks - 1; ++i)
            {
                double timeBwTwoPeaks = (peaks[0][i + 1] - peaks[0][i]) / fps;
                timeBwPeaks.Add(timeBwTwoPeaks);
            }

            // Tiden mellan alla toppar returneras i lista.
            return timeBwPeaks;
        }

        // Returnerar peakar och dalar som är tillräckligt höga.
        private List<List<double>> sortPeaksAndValleys(List<List<double>> peaks, List<List<double>> valleys)
        {
            List<List<double>> sortedPeaksAndValleys = new List<List<double>>();
            sortedPeaksAndValleys.Add(new List<double>()); // x Peak
            sortedPeaksAndValleys.Add(new List<double>()); // y Peak
            sortedPeaksAndValleys.Add(new List<double>()); // x Valley
            sortedPeaksAndValleys.Add(new List<double>()); // y Valley

            List<List<double>> peaksAndValleys = new List<List<double>>();
            peaksAndValleys.Add(new List<double>());
            peaksAndValleys.Add(new List<double>());
            peaksAndValleys.Add(new List<double>()); // 0 = dal, 1 = topp

            for (int i = 0, j = 0; i < peaks[0].Count || j < valleys[0].Count;)
            {
                if (i == peaks[0].Count)
                {
                    peaksAndValleys[0].Add(valleys[0][j]);
                    peaksAndValleys[1].Add(valleys[1][j]);
                    peaksAndValleys[2].Add(0);
                    j++;
                }
                else if (j == valleys[0].Count)
                {
                    peaksAndValleys[0].Add(peaks[0][i]);
                    peaksAndValleys[1].Add(peaks[1][i]);
                    peaksAndValleys[2].Add(1);
                    i++;
                }
                else if (peaks[0][i] < valleys[0][j])
                {
                    peaksAndValleys[0].Add(peaks[0][i]);
                    peaksAndValleys[1].Add(peaks[1][i]);
                    peaksAndValleys[2].Add(1);
                    i++;
                }
                else
                {
                    peaksAndValleys[0].Add(valleys[0][j]);
                    peaksAndValleys[1].Add(valleys[1][j]);
                    peaksAndValleys[2].Add(0);
                    j++;
                }
            }

            // HÄR BÖR peaksAndValleys vara en komplett sorterad lista.
            // Nästa steg blir att se över så att det kommer varannan topp, varannan dal.
            // Även para ihop så att det finns topp och dal

            if (peaksAndValleys[2].Count > 1 && peaksAndValleys[2][0] == 1) // Om första elementet är en topp
            {
                if (peaksAndValleys[2][1] == 0) // Och det andra värdet är en dal
                {
                    sortedPeaksAndValleys[0].Add(peaksAndValleys[0][0]);
                    sortedPeaksAndValleys[1].Add(peaksAndValleys[1][0]);
                    sortedPeaksAndValleys[2].Add(peaksAndValleys[0][1]);
                    sortedPeaksAndValleys[3].Add(peaksAndValleys[1][1]);
                }
            }

            for (int i = 0; i < peaksAndValleys[0].Count - 1; ++i)
            {
                if (peaksAndValleys[2][i] == 0) // Elementet är en dal
                {
                    if (peaksAndValleys[2][i + 1] == 1) // Elementet direkt efter är en topp
                    {
                        sortedPeaksAndValleys[0].Add(peaksAndValleys[0][i + 1]);
                        sortedPeaksAndValleys[1].Add(peaksAndValleys[1][i + 1]);
                        sortedPeaksAndValleys[2].Add(peaksAndValleys[0][i]);
                        sortedPeaksAndValleys[3].Add(peaksAndValleys[1][i]);
                        ++i;
                    }
                }
            }
            int numOfElements = peaksAndValleys[0].Count;
            return sortedPeaksAndValleys;
        }

        /* Dubbelamplitud koll
          Kollar hur hög amplituden är i förhållande till vänster och höger sida,
          och bildar medelvärde av dessa. Utifrån det sker sortering */

        private List<List<double>> doubleAmplitudePeaks(List<List<double>> peaks, List<List<double>> valleys)
        {
            List<List<double>> doubleAmplitudePeaks = new List<List<double>>();
            doubleAmplitudePeaks.Add(new List<double>());
            doubleAmplitudePeaks.Add(new List<double>());

            // Här ska topparnas positioner x [0], höjd y [1] och medelamplitud [2] lagras
            List<List<double>> ampPeaks = new List<List<double>>();
            ampPeaks.Add(new List<double>());
            ampPeaks.Add(new List<double>());
            ampPeaks.Add(new List<double>());

            List<List<double>> peaksAndValleys = new List<List<double>>();
            peaksAndValleys.Add(new List<double>());
            peaksAndValleys.Add(new List<double>());
            peaksAndValleys.Add(new List<double>()); // 0 = dal, 1 = topp

            for (int i = 0, j = 0; i < peaks[0].Count || j < valleys[0].Count;)
            {
                if (i == peaks[0].Count)
                {
                    peaksAndValleys[0].Add(valleys[0][j]);
                    peaksAndValleys[1].Add(valleys[1][j]);
                    peaksAndValleys[2].Add(0);
                    j++;
                }
                else if (j == valleys[0].Count)
                {
                    peaksAndValleys[0].Add(peaks[0][i]);
                    peaksAndValleys[1].Add(peaks[1][i]);
                    peaksAndValleys[2].Add(1);
                    i++;
                }
                else if (peaks[0][i] < valleys[0][j])
                {
                    peaksAndValleys[0].Add(peaks[0][i]);
                    peaksAndValleys[1].Add(peaks[1][i]);
                    peaksAndValleys[2].Add(1);
                    i++;
                }
                else
                {
                    peaksAndValleys[0].Add(valleys[0][j]);
                    peaksAndValleys[1].Add(valleys[1][j]);
                    peaksAndValleys[2].Add(0);
                    j++;
                }
            }

            // HÄR BÖR peaksAndValleys vara en komplett sorterad lista. Nästa steg blir att se över dess amplitud.

            double mean = 0;

            for (int i = 0; i < peaksAndValleys[0].Count - 2; ++i)
            {
                if (peaksAndValleys[2][i] == 0 && peaksAndValleys[2][i + 1] == 1)
                {
                    if (peaksAndValleys[2][i + 2] == 0)
                    {
                        ampPeaks[0].Add(peaksAndValleys[0][i + 1]);
                        ampPeaks[1].Add(peaksAndValleys[1][i + 1]);
                        ampPeaks[2].Add(2 * peaksAndValleys[1][i + 1] - peaksAndValleys[1][i] - peaksAndValleys[1][i + 2]);
                        mean += ampPeaks[2][ampPeaks[2].Count - 1];
                        ++i;
                    }
                    else if (i < peaksAndValleys[0].Count - 3 && peaksAndValleys[2][i + 3] == 0)
                    {
                        ampPeaks[0].Add(peaksAndValleys[0][i + 2]);
                        ampPeaks[1].Add(peaksAndValleys[1][i + 2]);
                        ampPeaks[2].Add(2 * peaksAndValleys[1][i + 2] - peaksAndValleys[1][i] - peaksAndValleys[1][i + 3]);
                        mean += ampPeaks[2][ampPeaks[2].Count - 1];
                        ++i;
                    }
                }
            }

            if (ampPeaks[2].Count != 0)
            {
                mean = mean / ampPeaks[2].Count;
            }

            for (int i = 0; i < ampPeaks[2].Count; ++i)
            {
                if (ampPeaks[2][i] > mean * 0.4 && ampPeaks[2][i] < mean * 2.5) //UNDERSÖK MER HÄR!!! :-) VERKAR VARA BRA NU! :-)
                {
                    doubleAmplitudePeaks[0].Add(ampPeaks[0][i]);
                    doubleAmplitudePeaks[1].Add(ampPeaks[1][i]);
                }
            }

            return doubleAmplitudePeaks;
        }

        // Returnerar "korrekta peakar" för andning. Krav; ska vara tillräckligt höga.
        private List<List<double>> correctPeaks(List<List<double>> peaks, List<List<double>> valleys, double minimiDepth)
        {
            List<List<double>> correctPeaks = new List<List<double>>();
            correctPeaks.Add(new List<double>());
            correctPeaks.Add(new List<double>());

            List<List<double>> peaksAndValleys = new List<List<double>>();
            peaksAndValleys.Add(new List<double>());
            peaksAndValleys.Add(new List<double>());
            peaksAndValleys.Add(new List<double>()); // 0 = dal, 1 = topp

            for (int i = 0, j = 0; i < peaks[0].Count || j < valleys[0].Count;)
            {
                if (i == peaks[0].Count)
                {
                    peaksAndValleys[0].Add(valleys[0][j]);
                    peaksAndValleys[1].Add(valleys[1][j]);
                    peaksAndValleys[2].Add(0);
                    j++;
                }
                else if (j == valleys[0].Count)
                {
                    peaksAndValleys[0].Add(peaks[0][i]);
                    peaksAndValleys[1].Add(peaks[1][i]);
                    peaksAndValleys[2].Add(1);
                    i++;
                }
                else if (peaks[0][i] < valleys[0][j])
                {
                    peaksAndValleys[0].Add(peaks[0][i]);
                    peaksAndValleys[1].Add(peaks[1][i]);
                    peaksAndValleys[2].Add(1);
                    i++;
                }
                else
                {
                    peaksAndValleys[0].Add(valleys[0][j]);
                    peaksAndValleys[1].Add(valleys[1][j]);
                    peaksAndValleys[2].Add(0);
                    j++;
                }
            }

            // HÄR peaksAndValleys vara en komplett sorterad lista. Nästa steg blir att se över dess amplitud :-)

            if (peaksAndValleys[2].Count > 2 && peaksAndValleys[2][0] == 1) // Om första värdet i listan är en topp 
            {
                if (peaksAndValleys[2][1] == 0) // Och det andra värdet i listan är en dal
                {
                    if (peaksAndValleys[1][0] - peaksAndValleys[1][1] > minimiDepth)
                    {
                        correctPeaks[0].Add(peaksAndValleys[0][0]);
                        correctPeaks[1].Add(peaksAndValleys[1][0]);
                    }
                }
            }

            for (int i = 0; i < peaksAndValleys[0].Count - 1; ++i)
            {
                if (peaksAndValleys[2][i] == 0)
                {
                    if (peaksAndValleys[2][i + 1] == 1)
                    {
                        if (peaksAndValleys[1][i + 1] - peaksAndValleys[1][i] > minimiDepth)
                        {
                            correctPeaks[0].Add(peaksAndValleys[0][i + 1]);
                            correctPeaks[1].Add(peaksAndValleys[1][i + 1]);
                            ++i;
                        }
                    }
                }
            }
            int num = peaksAndValleys[0].Count;

            return correctPeaks;
        }

        //Lokalisera topparna i lista för puls
        // Returvärdet är en lista med listor för [0] = positioner (x) och 
        // [1]=toppvärde till respektive position av topp (y) (alltså från tidsaxeln)
        private List<List<double>> locatePeaksPulse(List<double> measurements)
        {
            List<List<double>> topLocations = new List<List<double>>();
            topLocations.Add(new List<double>());
            topLocations.Add(new List<double>());
            int upCounter = 0;
            int downCounter = 0;

            for (int i = 0; i < measurements.Count - 5; i++)
            {
                //Påväg uppåt
                if (measurements[i] < measurements[i + 1])
                {
                    if (downCounter < 4) //om det gått nedåt i max 0,1 sekunder, eller inte alls, kan det gå uppåt
                    {
                        upCounter += 1;
                        downCounter = 0;
                    }
                }
                //Vid topp
                else if (measurements[i] > (measurements[i + 1] + measurements[i + 2] + measurements[i + 3] + measurements[i + 4] + measurements[i + 5]) / 5)
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
            }
            return topLocations;
        }

        private List<List<double>> locateValleysPulse(List<double> measurements)
        {
            List<List<double>> valleyLocations = new List<List<double>>();
            valleyLocations.Add(new List<double>());
            valleyLocations.Add(new List<double>());
            int upCounter = 0;
            int downCounter = 0;

            for (int i = 0; i < measurements.Count - 5; i++)
            {
                //Påväg nedåt
                if (measurements[i] > measurements[i + 1])
                {
                    if (upCounter < 4)
                    {
                        downCounter += 1;
                        upCounter = 0;
                    }
                }

                //Vid dal
                else if (measurements[i] < (measurements[i + 1] + measurements[i + 2] + measurements[i + 3] + measurements[i + 4] + measurements[i + 5]) / 5)
                {
                    if (downCounter > 4)
                    {
                        valleyLocations[0].Add(Convert.ToDouble(i)); // Positionen på dalen läggs till i listan
                        valleyLocations[1].Add(measurements[i]); // Värdet på dalen läggs till i listan
                        upCounter = 0;
                        downCounter = 0;
                    }
                }

                //Påväg uppåt
                else if (measurements[i] < measurements[i + 1])
                {
                    if (downCounter < 4) //om det gått nedåt i max 0,1 sekunder, eller inte alls, kan det gå uppåt
                    {
                        upCounter += 1;
                        downCounter = 0;
                    }
                }

            }
            return valleyLocations;
        }

        /* Kollar om pekarna och dalarna ligger inom tillåtet intervall.
           Hur mågna std-avvikelser höjderna får avvika upp/ned anges genom konstanten k nedan i denna funktion.*/
        private List<List<double>> checkHeights(List<List<double>> sortedPeaksAndValleys, double meanH, double sigmaH)
        {
            // Ta fram info ur sortedPeaksAndValleys 
            // [0] = x-peak, [1] = y-peak, [2] = x-dal, [3] = y-dal
            List<double> xPeaks = sortedPeaksAndValleys[0];
            List<double> yPeaks = sortedPeaksAndValleys[1];
            List<double> xValleys = sortedPeaksAndValleys[2];
            List<double> yValleys = sortedPeaksAndValleys[3];

            List<List<double>> highEnoughPeaks = new List<List<double>>();
            highEnoughPeaks.Add(new List<double>());
            highEnoughPeaks.Add(new List<double>());
            highEnoughPeaks.Add(new List<double>());
            highEnoughPeaks.Add(new List<double>());

            int N = xPeaks.Count;
            // k är en konstant som avgör hur många std-avvikelser 
            // från medel som är tillåtna för att uppfylla höjdvillkor.
            double k1 = 0.8;
            double k2 = 1.5;

            for (int i = 0; i < N; ++i)
            {
                double height = yPeaks[i] - yValleys[i];
                if (height < meanH + k2 * sigmaH && height > meanH - k1 * sigmaH)
                {
                    // [0] = x-peak, [1] = y-peak, [2] = x-dal, [3] = y-dal
                    highEnoughPeaks[0].Add(xPeaks[i]);
                    highEnoughPeaks[1].Add(yPeaks[i]);
                    highEnoughPeaks[2].Add(xValleys[i]);
                    highEnoughPeaks[3].Add(yValleys[i]);
                }
            }

            // Testa rensa listorna
            xPeaks.Clear();
            yPeaks.Clear();
            xValleys.Clear();
            yValleys.Clear();

            // Returnera
            return highEnoughPeaks;
        }
        /* SLUT checkHeights*/

        /* Returnerar lista med: [0] = x-peak, [1] = y-peak, [2] = x-dal, [3] = y-dal 
           där toppar och dalar från brus ska ha reducerats */
        private List<List<double>> sortByHeight(List<List<double>> peaksPulse, List<List<double>> valleysPulse, int lastSample)
        {
            // Peakar och dalar sorterade så att dem ligger i ordning; varannan peak, varannan dal.
            // [0] = x-peak, [1] = y-peak, [2] = x-dal, [3] = y-dal
            List<List<double>> sortedPeaksAndValleys = new List<List<double>>();
            sortedPeaksAndValleys.Add(new List<double>()); // x Peak
            sortedPeaksAndValleys.Add(new List<double>()); // y Peak
            sortedPeaksAndValleys.Add(new List<double>()); // x Valley
            sortedPeaksAndValleys.Add(new List<double>()); // y Valley

            sortedPeaksAndValleys = sortPeaksAndValleys(peaksPulse, valleysPulse);

            // Värden på peakar och dalar
            List<double> xPeaks = sortedPeaksAndValleys[0];
            List<double> xValleys = sortedPeaksAndValleys[2];

            List<double> yPeaks = sortedPeaksAndValleys[1];
            List<double> yValleys = sortedPeaksAndValleys[3];

            /* Sök medelvärde och standardavvikelse för höjderna (topp-till-dal) senaste 10 s */
            // Tar fram medelhöjd meanH (x-streck)
            double meanH10 = 0;
            double M = 0;

            for (int i = 0; i < xPeaks.Count; ++i)
            {
                if (xPeaks[i] > (lastSample - 1 - fps * 10) && xPeaks[i] < (lastSample - 1))
                {
                    meanH10 += (yPeaks[i] - yValleys[i]);
                    M += 1;
                }
            }
            meanH10 = meanH10 / M;

            // Tar fram summa av höjden
            double sum10 = 0;
            for (int i = 0; i < xPeaks.Count; ++i)
            {
                if (xPeaks[i] > (lastSample - 1 - fps * 10) && xPeaks[i] < (lastSample - 1))
                {
                    double xi10 = yPeaks[i] - yValleys[i];
                    sum10 += (xi10 - meanH10) * (xi10 - meanH10);
                }
            }

            // Tar fram std-avvikelsen sigmaH10
            double sigmaH10 = Math.Sqrt((1 / M) * sum10);
            stdH10 = sigmaH10; // stdH10 är global variabel
            /* Slut */

            /* Sök medelvärde och standardavvikelse för höjderna (topp-till-dal) */
            // Tar fram medelhöjd meanH (x-streck)
            double meanH = 0;
            double n = yPeaks.Count;
            double m = yValleys.Count;
            double N = Math.Min(n, m); // Anm: Ska ej behövas nu. n = m ska gälla.

            for (int i = 0; i < N; ++i)
            {
                meanH += (1 / N) * (yPeaks[i] - yValleys[i]);
            }

            // Tar fram summa av höjden
            double sum = 0;
            for (int i = 0; i < N; ++i)
            {
                double xi = yPeaks[i] - yValleys[i];
                sum += (xi - meanH) * (xi - meanH);
            }

            // Tar fram std-avvikelsen sigmaH
            double sigmaH = Math.Sqrt((1 / N) * sum);

            // Korrigera std listan
            double lengthLst = stdMeanLst.Count;

            // Lägg till värden i listan för max senaste 5 minuterna
            // 1800 värden / (6 ggr per sekund * 60 sekunder) = 5 minuter
            if (lengthLst < 1800)
            {
                stdMeanLst.Add(sigmaH);
            }
            else
            {
                stdMeanLst.RemoveAt(0);
                stdMeanLst.Add(sigmaH);
            }
            //Uppdatera till ny medel-std
            stdMean = stdMeanLst.Average(); // Global variabel, std-värden över max senaste 5 min

            /* Slut medel och Std*/

            // Sortera ut värden  
            List<List<double>> filteredByH = new List<List<double>>();
            filteredByH.Add(new List<double>());
            filteredByH.Add(new List<double>());
            filteredByH.Add(new List<double>());
            filteredByH.Add(new List<double>());

            // Sorterar ut beroende på höjdskillnader, min och max besäknas från medel och std-avvikelse,
            // genom att kalla på underfunktion.
            filteredByH = checkHeights(sortedPeaksAndValleys, meanH, sigmaH);

            return filteredByH;
        }
        /* SLUT: Sortera bort en del pulsvärden på bas av höjd */

        //Utsållning med avseende till tiden
        // Returnerar double-lista med peakar där peakar som ligger för nära varandra sållats bort
        // [0] = Peakarnas x-position, [1] = Peakarnas y-position
        //Utsållning med avseende till tiden
        // Returnerar double-lista med peakar där peakar som ligger för nära varandra sållats bort
        // [0] = Peakarnas x-position, [1] = Peakarnas y-position, [2] = Tider mellan två (godkända) peakar
        private List<List<double>> removeByTime(List<List<double>> peakList)
        {
            // Lista med alla tider mellan alla toppar
            List<double> timeBetweenHeartBeats = new List<double>();
            timeBetweenHeartBeats = timeBetweenAllPeaks(peakList);

            // Lista med peakar sorterade på tid.
            List<List<double>> sortedByTime = new List<List<double>>();
            sortedByTime.Add(new List<double>());
            sortedByTime.Add(new List<double>());
            sortedByTime.Add(new List<double>());

            double average = 0;

            // Tar fram sammanlagda totala tiden mellan alla hjärtslag, d.v.s. t1 + t2 +...+ tn
            for (int i = 0; i < timeBetweenHeartBeats.Count; ++i)
            {
                average += timeBetweenHeartBeats[i];
            }

            // MEDELtiden mellan 2 st hjärtslag, beräknat över mättiden (bör vara 10 sekunder).
            // Medeltiden = average = t_tot / antal intervall
            if (timeBetweenHeartBeats.Count != 0)
            {
                average = average / timeBetweenHeartBeats.Count;
            }

            // Hjärtslaget ska avvika max 30% nedåt och max 40% uppåt från medeltiden, för att vara "korrekt".
            for (int i = 0; i < timeBetweenHeartBeats.Count; ++i)
            {
                if (timeBetweenHeartBeats[i] > average * 0.7 && timeBetweenHeartBeats[i] <= average * 1.4)
                {
                    sortedByTime[0].Add(peakList[0][i + 1]); // x-pos = tiden
                    sortedByTime[1].Add(peakList[1][i + 1]); // y-pos = Amplituden
                    sortedByTime[2].Add(timeBetweenHeartBeats[i]); // Tiden mellan topparna
                }
                else if (i != timeBetweenHeartBeats.Count - 1 && (timeBetweenHeartBeats[i] + timeBetweenHeartBeats[i + 1]) > average * 0.7 &&
                    timeBetweenHeartBeats[i] + timeBetweenHeartBeats[i + 1] <= 1.4)
                {
                    sortedByTime[0].Add(peakList[0][i + 1]); // x-pos = Tiden
                    sortedByTime[1].Add(peakList[1][i + 1]); // y-pos = Amplituden
                    sortedByTime[2].Add(timeBetweenHeartBeats[i] + timeBetweenHeartBeats[i + 1]); // Tiden mellan topp1 och topp3 (där topp2 förbises)
                    i++;
                }
            }

            return sortedByTime;
        }


        /// Härifrån körs alla kommandon som har med signalbehandling och detektion av frekvenser att göra.
        /// <param name="codeString">definierar detektion av puls ("pulse") eller andning ("breathing") som en sträng</param>
        /// <param name="measurements">innehåller all mätdata i form av en lista med floats</param>

        int antalFel = 0; // DENNA SKA VÄL TAS BORT TILL SLUTPRODUKTEN?!?!?!

        private void plottingAndCalculations(string codeString, List<double> breathingList = null, List<double> rgbList = null)
        {
            try
            {
                // Analys av puls
                if (codeString == "pulse" && nighttime.IsChecked == false)
                {
                    if (rgbList.Count >= startPulseAfterSeconds * fps + fps)
                    {
                        double pulsWarningOverSamples = pulseWarningInSeconds * fps;

                        // Filtrering
                        double[] rgbListFilt = bpFiltPulse.ProcessSamples(rgbList.ToArray());
                        List<double> rgbFiltList = rgbListFilt.ToList();
                        rgbFiltList.RemoveRange(0, fps);
                        // Slut filtrering

                        // Initialisering av markeringar i pulsplot
                        chartPulse.CheckAndAddSeriesToGraph("Puls", "hjärtslag/minut");
                        //chartPulse.CheckAndAddSeriesToGraph("TopLines", "fps2");
                        chartPulse.CheckAndAddSeriesToGraph("Toppmarkeringar", "topp");
                        //chartPulse.CheckAndAddSeriesToGraph("HeightMarkers", "heightMarker");
                        //chartPulse.CheckAndAddSeriesToGraph("ValleyMarkers", "valleyMarker");
                        //chartPulse.CheckAndAddSeriesToGraph("TimeAndHeightMarker", "TimeAndHeightMarker");
                        //chartPulse.CheckAndAddSeriesToGraph("CcccomboMarkers", "comboMarker");
                        chartPulse.ClearCurveDataPointsFromGraph();

                        double momentaryPulse = 0;

                        /* Toppdetektering. Toppar urskiljs även från brus inom bandbredden. */

                        // Toppdetektering. Steg (1)
                        List<List<double>> peaksPulse = new List<List<double>>();
                        peaksPulse = locatePeaksPulse(rgbFiltList); // Testas nu
                        // Daldetektering. Steg (1)
                        List<List<double>> valleysPulse = new List<List<double>>();
                        valleysPulse = locateValleysPulse(rgbFiltList);

                        // Sortera toppar och dalar baserat på höjd. Steg (2)
                        int lastSample = rgbFiltList.Count;
                        List<List<double>> peaksAndValleysByHeight = new List<List<double>>();
                        peaksAndValleysByHeight = doubleAmplitudePeaks(peaksPulse, valleysPulse);

                        // Sortera toppar baserat på tiden 
                        List<List<double>> peaksByTimeAndAmplitude = new List<List<double>>();
                        peaksByTimeAndAmplitude = removeByTime(peaksAndValleysByHeight);
                        /* SLUT toppdetektering */

                        ////Beräkning av hjärtfrekvens
                        double heartrate = 0;
                        //double periods = 0; //DENNA OCH 2 NEDANSTÅENDE KAN BORTKOMMENTERAS!!!
                        //List<double> timeBetweenHeartBeats = new List<double>();
                        //timeBetweenHeartBeats = timeBetweenAllPeaks(peaksByTimeAndAmplitude); //SE TILL SÅ DETTA ÄR RÄTT LISTA!!! O.o :-O
                        double xStart = rgbFiltList.Count - (pulseWarningInSeconds * fps);

                        //HÄR ÄR ALL BORTSORTERING AV FELAKTIGA PEAK:AR KLAR!!!

                        //// DETTA KAN KOMMENTERAS BORT, HÄRIFRÅN OCH NER ...
                        //List<double> excelList = new List<double>();

                        ////Ser till att beräkningarna endast sker över valda sekunder
                        //for (int i = 0; i < peaksByTimeAndAmplitude[0].Count - 1; ++i)
                        //{
                        //    if (peaksByTimeAndAmplitude[0][i] >= xStart)
                        //    {
                        //        heartrate += 60 / timeBetweenHeartBeats[i]; // Beräknar den momentana pulsen för varje topp och medelvärdesberäknar därefter alla
                        //        periods += 1;
                        //        excelList.Add(timeBetweenHeartBeats[i]);

                        //        if (i == peaksByTimeAndAmplitude[0].Count - 2)
                        //        {
                        //            momentaryPulse = 60 / timeBetweenHeartBeats[i];
                        //            momentaryPulse = Math.Round(momentaryPulse);
                        //        }
                        //    }
                        //}

                        ////Skriver ut heartPulse på skärmen
                        //if (periods == 0)
                        //{
                        //    heartPulse = 0;
                        //}
                        //else
                        //{
                        //    heartPulse = Math.Round(heartrate / periods);
                        //}

                        //// Skickar heart-rate variability-värden till Excel då knapp i interface klickas på.
                        //if (heartRateVariabilityFlag == true)
                        //{
                        //    for (int i = 0; i < excelList.Count; ++i)
                        //    {
                        //        //oSheet.Cells[(i+1).ToString(), "A"].Value2 = (Math.Round(excelList[i], 4)).ToString();
                        //        Console.WriteLine(excelList[i]);
                        //    }

                        //    //oXL.Visible = false;
                        //    //oXL.UserControl = false;
                        //    //oWB.SaveAs(path + @"\..\..\..\HRV-excel\" + excelnamn.Text + ".xls", Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                        //    //    false, false, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                        //    heartRateVariabilityFlag = false;
                        //}
                        //// ... HIT

                        // DETTA KAN ANVÄNDAS ISTÄLLET FÖR OVANSTÅENDE KOD (OM DEN BORTKOMMENTERAS!)
                        //Ser till att beräkningarna endast sker över valda sekunder
                        int counter = 0;

                        for (int i = 0; i < peaksByTimeAndAmplitude[0].Count - 1; ++i)
                        {
                            if (peaksByTimeAndAmplitude[0][i] >= xStart)
                            {
                                heartrate += 60 / peaksByTimeAndAmplitude[2][i]; // Beräknar den momentana pulsen för varje topp och medelvärdesberäknar därefter alla
                                counter++;
                                if (i == peaksByTimeAndAmplitude[2].Count - 1)
                                {
                                    momentaryPulse = 60 / peaksByTimeAndAmplitude[2][i];
                                    momentaryPulse = Math.Round(momentaryPulse);
                                }
                            }
                        }

                        //Skriver ut heartPulse på skärmen
                        if (counter == 0)
                        {
                            heartPulse = 0;
                        }
                        else
                        {
                            heartPulse = Math.Round(heartrate / counter);
                        }

                        // Skriver ut heart-rate variability-värden till konsollen då knapp i interface klickas på.
                        if (heartRateVariabilityFlag == true)
                        {
                            for (int i = 0; i < peaksByTimeAndAmplitude[2].Count; ++i)
                            {
                                Console.WriteLine(peaksByTimeAndAmplitude[2][i]);
                            }

                            heartRateVariabilityFlag = false;
                        }

                        /* Plottning av pulskurva (färgvärde median(röd/grön) över tid), samt toppdetektering. */
                        int j = 0;
                        if (rgbFiltList.Count - plotOverSeconds * fps >= 0)
                        {
                            j = rgbFiltList.Count - plotOverSeconds * fps;
                        }

                        /*
                        for (int i = 0; i < peaksPulse[0].Count(); i++)
                        {
                            if (peaksPulse[0][i] >= j)
                            {
                                chartPulse.AddPointToLine("Pulsemarkers", peaksPulse[1][i], peaksPulse[0][i] - j);
                            }
                        }

                        for (int i = 0; i < valleysPulse[0].Count(); i++)
                        {
                            if (valleysPulse[0][i] >= j)
                            {
                                chartPulse.AddPointToLine("ValleyMarkers", valleysPulse[1][i], valleysPulse[0][i] - j);
                            }
                        }*/

                        //for (int i = 0; i < peaksAndValleysByHeight[0].Count(); i++)
                        //{
                        //    if (peaksAndValleysByHeight[0][i] >= j)
                        //    {
                        //        chartPulse.AddPointToLine("Toppmarkeringar", peaksAndValleysByHeight[1][i], peaksAndValleysByHeight[0][i] - j);
                        //    }
                        //}

                        for (int i = 0; i < peaksByTimeAndAmplitude[0].Count(); i++)
                        {
                            if (peaksByTimeAndAmplitude[0][i] >= j)
                            {
                                chartPulse.AddPointToLine("Toppmarkeringar", peaksByTimeAndAmplitude[1][i] * 1.001, peaksByTimeAndAmplitude[0][i] - j);
                            }
                        }
                        /* Slut plottning puls */

                        // Beräknar ut pulsen över den valda beräkningstiden
                        int samplesForPulseAlarm = pulseWarningInSeconds * fps;

                        // Plockar ut värden över de senaste 10 sekundrarna
                        while (peaksPulse[0].Count > 0 && peaksPulse[0][0] < rgbFiltList.Count - samplesForPulseAlarm)
                        {
                            peaksPulse[0].RemoveAt(0);
                            peaksPulse[1].RemoveAt(0);
                        }

                        /* Tar in larmgränsen och jämför med personens uppskattade puls. */
                        pulseAlarm(heartPulse, lowNumPulse, lastSample);
                        /* Slut larm */

                        for (int k = j; k < rgbFiltList.Count(); k++)
                        {
                            chartPulse.AddPointToLine("Puls", rgbFiltList[k], k - j);
                        }
                        /*
                        for (int k = 0; k < heartRateList[0].Count(); k++)
                        {
                            chartPulse.AddPointToLine("TopLines", heartRateList[1][k], heartRateList[0][k] - j);
                        }
                        */
                        // Justus hade rgbFiltList innan.
                        if (rgbList.Count() >= samplesOfMeasurement)
                        {
                            rgbList.RemoveRange(0, runPlotModulo);
                        }
                    }
                }

                //Analys av andning
                else if (codeString == "breathing")
                {
                    if (breathingList.Count >= startBreathingAfterSeconds * fps + fps)
                    {
                        double breathingWarningOverSamples = breathingWarningInSeconds * fps;

                        // Filtrering av djupvärden (andning)
                        double[] breathingFilt = bpFiltBreath.ProcessSamples(breathingList.ToArray());
                        List<double> breathingFiltList = breathingFilt.ToList();

                        breathingFiltList.RemoveRange(0, fps);

                        chartBreath.CheckAndAddSeriesToGraph("Andning", "andetag/minut");
                        chartBreath.CheckAndAddSeriesToGraph("Toppmarkeringar", "topp");
                        //chartBreath.CheckAndAddSeriesToGraph("Valleymarkers", "valleyMarker");
                        chartBreath.ClearCurveDataPointsFromGraph();

                        // Toppdetektering

                        List<List<double>> peaks = new List<List<double>>();
                        List<List<double>> valleys = new List<List<double>>();
                        peaks = locatePeaksBreath(breathingFiltList);
                        valleys = locateValleysBreath(breathingFiltList);

                        // Korrekta toppar
                        List<List<double>> breathPeaksFilt = new List<List<double>>();
                        breathPeaksFilt = correctPeaks(peaks, valleys, minimiDepthBreath);

                        int j = 0;
                        if (breathingFiltList.Count - plotOverSeconds * fps >= 0)
                        {
                            j = breathingFiltList.Count - plotOverSeconds * fps;
                        }

                        // Rita ut peakar i andningen (= utandning)
                        for (int i = 0; i < breathPeaksFilt[0].Count; i++)
                        {
                            if (breathPeaksFilt[0][i] >= j)
                            {
                                chartBreath.AddPointToLine("Toppmarkeringar", breathPeaksFilt[1][i] * 1.001, breathPeaksFilt[0][i] - j);
                            }
                        }

                        // Beräknar ut andningsfrekvensen över den valda beräkningstiden
                        int samplesForBreathAlarm = breathingWarningInSeconds * fps;

                        while (breathPeaksFilt[0].Count > 0 && breathPeaksFilt[0][0] < breathingFiltList.Count - samplesForBreathAlarm)
                        {
                            breathPeaksFilt[0].RemoveAt(0);
                            breathPeaksFilt[1].RemoveAt(0);
                        }

                        ////Beräkning av hjärtfrekvens
                        double breathingRate = 0;
                        double average = 0;
                        List<double> timeBetweenBreaths = new List<double>();
                        timeBetweenBreaths = timeBetweenAllPeaks(breathPeaksFilt);

                        for (int i = 0; i < timeBetweenBreaths.Count; ++i)
                        {
                            breathingRate += timeBetweenBreaths[i];
                        }

                        if (timeBetweenBreaths.Count != 0)
                        {
                            breathingRate = breathingRate / timeBetweenBreaths.Count;
                            breathingRate = Math.Round(60 / breathingRate);
                        }

                        // Average är antalet peakar i andningen under 60 sekunder.
                        average = breathPeaksFilt[0].Count() * 60 / breathingWarningInSeconds;
                        //Sparar andningsfrekvensen i den globala variabeln
                        breathRate = breathingRate;

                        //Skickar alarmgränsen till larmfunktionen för att testa ifall ett larm ska ges.
                        breathingAlarm(breathRate, lowNumBreathing);

                        //Plottar andningen i andningsgrafen
                        for (int k = j; k < breathingFiltList.Count; k++)
                        {
                            chartBreath.AddPointToLine("Andning", breathingFiltList[k], k - j);
                        }

                        //Begränsar listan från att bli för stor
                        if (breathingFiltList.Count >= samplesOfMeasurement && depthList.Count >= runPlotModulo)
                        {
                            depthList.RemoveRange(0, runPlotModulo);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(" Varken puls- eller andning-funktion kördes, kontrollera att codeString var korrekt");
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
                clearGraphs();
                kinectSensor.Close();
                Alarm breathAlarm = new Alarm(this, kinectSensor, path, "breath");
                this.Hide();
                breathAlarm.Show();
                breathRate = 12;
            }
        }

        // Larm för pulsen
        private void pulseAlarm(double averagePulse, int lowNum, int lastSample)
        {
            if (averagePulse < lowNum || ((lastSample >= fps * startPulseAfterSeconds) && stdH10 < (stdMean * 1 / 8)))
            {
                clearGraphs();
                kinectSensor.Close();
                Alarm pulseAlarm = new Alarm(this, kinectSensor, path, "pulse");
                this.Hide();
                pulseAlarm.Show();
                heartPulse = 60;
            }
        }

        // Funktion som tar ut färgvärdena för en pixel
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

        //Byter färg på en pixel till den angivna färgen
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
                else if (color == "transparantBlue")
                {
                    array[startposition + 1] += 30;
                }
                else if (color == "transparantGreen")
                {
                    array[startposition + 2] += 30;
                }
                else if (color == "transparantRed")
                {
                    array[startposition + 3] += 30;
                }
            }
        }

        //Kontrollerar ifall någon kropp detekteras och följs av kameran
        bool IsBodyTracked()
        {
            bool bodytracked = false;
            for (int i = 0; i < 5; ++i)
            {
                if (bodySensning.bodies[i].IsTracked)
                    bodytracked = true;
            }
            return bodytracked;
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
                    if (bodySensning.getHeadJoint().JointType == JointType.Head && IsBodyTracked())
                    //if (bodySensning.getRightHandJoint().JointType == JointType.HandRight)
                    {
                        try
                        {
                            ColorSpacePoint colorSpaceHeadPoint = bodySensning.getCoordinateMapper().
                                MapCameraPointToColorSpace(bodySensning.getHeadJoint().Position);

                            // Här tar vi ut alla röda värden i de intressanta pixlarna
                            List<int> rödapixlar = null;
                            rödapixlar = new List<int>();

                            //Här tar vi ut alla gröna värden i de intressanta pixlarna
                            List<int> grönapixlar = null;
                            grönapixlar = new List<int>();

                            // Rutan som följer HeadJoint
                            for (int i = (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) - dotSize);
                                i <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) + dotSize); ++i)
                            {
                                for (int j = (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) - dotSize);
                                    j <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) + dotSize); ++j)
                                {
                                    int r = getcolorfrompixel(i, j, pixels, "red");
                                    int g = getcolorfrompixel(i, j, pixels, "green");

                                    if ((0 < r && r < 255) && (0 < g && g < 255))
                                    {
                                        rödapixlar.Add(r);
                                        grönapixlar.Add(g);
                                    }
                                    if (i <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) - dotSize * 0.9) || i >= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) + dotSize * 0.9)
                                        || j <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) - dotSize * 0.9) || j >= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) + dotSize * 0.9))
                                    {
                                        ChangePixelColor(i, j, pixels, "red");
                                    }
                                }
                            }

                            List<double> pulseList = colorSensing.createPulseList(rödapixlar, grönapixlar);

                            //Laddar hjärt-grafen
                            if (Math.Round((double)pulseList.Count / (double)(startPulseAfterSeconds * fps + fps) * 100) <= 100)
                            {
                                if (nighttime.IsChecked == false)
                                {
                                    double procent = Math.Round((double)pulseList.Count / (double)(startPulseAfterSeconds * fps + fps) * 100);
                                    TextBlock.Text = procent.ToString() + "%";
                                    TextBlock.Visibility = Visibility.Visible;
                                    chartPulse.Visibility = Visibility.Hidden;
                                    heartrateTextBlock.Visibility = Visibility.Hidden;
                                    textBlock1_Copy.Visibility = Visibility.Hidden;
                                    heart2.Visibility = Visibility.Visible;
                                    heart2.Width = heart2.MaxWidth * procent / 100;
                                    heart2.Height = heart2.MaxHeight * procent / 100;
                                }
                                else
                                {
                                    heart2.Visibility = Visibility.Hidden;
                                    TextBlock.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (Math.Round((double)pulseList.Count / (double)(startPulseAfterSeconds * fps + fps) * 100) == 101)
                            {
                                if (nighttime.IsChecked == false)
                                {
                                    TextBlock.Text = "";
                                    chartPulse.Visibility = Visibility.Visible;
                                    heartrateTextBlock.Visibility = Visibility.Visible;
                                    textBlock1_Copy.Visibility = Visibility.Visible;
                                    heart2.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    heart2.Visibility = Visibility.Hidden;
                                    TextBlock.Visibility = Visibility.Hidden;
                                }
                            }

                            //definiera hur ofta och hur stor listan är här innan.
                            if (pulseList.Count % runPlotModulo == 0)
                            {
                                //Analys av puls i matlab
                                plottingAndCalculations("pulse", pulseList, pulseList);
                            }
                        }
                        catch
                        { }

                    }

                    if (bodySensning.getSpineMidJoint().JointType == JointType.SpineMid && IsBodyTracked())
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
                                        ChangePixelColor(i, j, pixels, "blue");
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
                    if (bodySensning.getSpineMidJoint().JointType == JointType.SpineMid && IsBodyTracked())
                    {
                        try
                        {
                            depthList.Add(depthSensing.createDepthListAvarage(bodySensning.getCoordinateMapper(), bodySensning.getBellyJoint(), pixelData));

                            //lägg till average i listan med alla djupvärden
                            //skicka listan om den blivit tillräckligt stor
                            if (depthList.Count % runPlotModulo == 0)
                            {
                                plottingAndCalculations("breathing", depthList);
                            }

                            //Laddar lung-grafen
                            if (Math.Round((double)depthList.Count / (double)(startBreathingAfterSeconds * fps + fps) * 100) <= 100)
                            {
                                double procent = Math.Round((double)depthList.Count / (double)(startBreathingAfterSeconds * fps + fps) * 100);
                                TextLungLoad.Text = procent.ToString() + "%";
                                chartBreath.Visibility = Visibility.Hidden;
                                breathrateTextBlock.Visibility = Visibility.Hidden;
                                textBlock1.Visibility = Visibility.Hidden;
                                lung3.Visibility = Visibility.Visible;
                                lung3.Width = lung3.MaxWidth * procent / 100;
                                lung3.Height = lung3.MaxHeight * procent / 100;
                            }
                            else if (Math.Round((double)depthList.Count / (double)(startBreathingAfterSeconds * fps + fps) * 100) == 101)
                            {
                                TextLungLoad.Text = "";
                                chartBreath.Visibility = Visibility.Visible;
                                breathrateTextBlock.Visibility = Visibility.Visible;
                                textBlock1.Visibility = Visibility.Visible;
                                lung3.Visibility = Visibility.Hidden;
                            }
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            //System.Windows.MessageBox.Show("Baby has escaped, baby can't be far");
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            settingWindow.Show();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            clearGraphs();

        }

        private void clearGraphs()
        {
            depthList.Clear();
            colorSensing.gDrList.Clear();
            chartPulse.ClearCurveDataPointsFromGraph();
            chartBreath.ClearCurveDataPointsFromGraph();
            chartPulse.Visibility = Visibility.Hidden;
            chartBreath.Visibility = Visibility.Hidden;
            TextBlock.Text = "0 %";
            TextLungLoad.Text = "0 %";

            heartrateTextBlock.Visibility = Visibility.Hidden;
            breathrateTextBlock.Visibility = Visibility.Hidden;
            textBlock1.Visibility = Visibility.Hidden;
            textBlock1_Copy.Visibility = Visibility.Hidden;
            heartPulse = 60;
            breathRate = 30;

            //Rensa data för standardavvikelsenn
            stdMeanLst.Clear();
            stdMean = 0;
            stdH10 = 0;

        }

        //Timer-funktionen
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            if (heartDecreasing)
            {
                heart.Opacity -= 0.1;
                heart.Width -= 5;
                heart.Height -= 5;
            }
            else
            {
                heart.Opacity += 0.1;
                heart.Width += 5;
                heart.Height += 5;
            }
            if (heart.Opacity <= 0.4)
            {
                heartDecreasing = false;
            }
            if (heart.Opacity == 1)
            {
                heartDecreasing = true;
            }
            //Skriv ut hjärtfrekvensen
            heartrateTextBlock.Text = heartPulse.ToString();
            XMLsave XMLsave = new XMLsave();
            XMLsave.saveToXML(heartPulse.ToString(), "pulse");

            //Sätt timertiden till att matcha hjärtfrekvensen
            if (heartPulse != 0)
            {
                dispatcherTimer.Interval = new TimeSpan(60 / (long)heartPulse * 10000000 / 14);
                dispatcherTimer.Start();
            }
            else
            {
                dispatcherTimer.Interval = new TimeSpan(10000000 / 14);
                dispatcherTimer.Start();
            }
        }

        //Timer-funktionen
        private void lungTimer_Tick(object sender, EventArgs e)
        {
            if (lungDecreasing)
            {
                lung2.Opacity -= 0.05;
                lung2.Width -= 1.2;
                lung2.Height -= 0.3;
            }
            else
            {
                lung2.Opacity += 0.05;
                lung2.Width += 1.2;
                lung2.Height += 0.3;
            }
            if (lung2.Opacity <= 0.4)
            {
                lungDecreasing = false;
            }
            if (lung2.Opacity == 1)
            {
                lungDecreasing = true;
            }
            //Skriver ut andningsfrekvens
            breathrateTextBlock.Text = breathRate.ToString();

            if (breathRate != 0)
            {
                lungTimer.Interval = new TimeSpan(60 / (long)breathRate * 10000000 / 28);
                lungTimer.Start();
            }
            else
            {
                lungTimer.Interval = new TimeSpan(60 / 20 * 10000000 / 28);
                lungTimer.Start();
            }
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void DotSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dotSize = (int)DotSizeSlider.Value;
        }

        //___________________TANGENTER__________________________
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                settingWindow.Save_Click(sender, e);
            }
        }

        private void nighttime_Checked(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            this.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF000000");
            movieFrame.Source = ImageSource3;
            nightSound.Play();
            chartPulse.Visibility = Visibility.Hidden;
            heartrateTextBlock.Visibility = Visibility.Hidden;
            textBlock1_Copy.Visibility = Visibility.Hidden;
            heart.Visibility = Visibility.Hidden;
            heart2.Visibility = Visibility.Hidden;
        }

        private void nighttime_Unchecked(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            this.Background = bgBrush;
            movieFrame.Source = ImageSource2;
            daySound.Play();
            //chartPulse.Visibility = Visibility.Visible;
            heartrateTextBlock.Visibility = Visibility.Visible;
            textBlock1_Copy.Visibility = Visibility.Visible;
            heart.Visibility = Visibility.Visible;

            // Rensa data för standardavvikelsen
            stdMeanLst.Clear();
            stdMean = 0;
            stdH10 = 0;

            // Rensa pulsdata och grafer
            colorSensing.gDrList.Clear();
            chartPulse.ClearCurveDataPointsFromGraph();
            textBlock1_Copy.Visibility = Visibility.Hidden; // Pulsens enhet (slag / minut).
        }


    }
}

namespace MyApp.Tools
{

    [System.Windows.Data.ValueConversion(typeof(string), typeof(string))]
    public class RatioConverter : System.Windows.Markup.MarkupExtension, System.Windows.Data.IValueConverter
    {
        private static RatioConverter _instance;

        public RatioConverter() { }

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { // do not let the culture default to local to prevent variable outcome re decimal syntax
            double size = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, System.Globalization.CultureInfo.InvariantCulture);
            return size.ToString("G0", System.Globalization.CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { // read only converter...
            throw new System.NotImplementedException();
        }

        public override object ProvideValue(System.IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RatioConverter());
        }

    }
}
