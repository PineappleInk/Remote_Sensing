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
    using System.Linq;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //Graf
        private ViewModel viewModel;
        private List<double> listTest;
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Paramteter for position of bellyJoint
        /// </summary>
        private double bellyJointYPosition = 2 / 3;
        private double bellyJointXPosition = 2 / 3;

        double heartrate = 0;
        double breathingrate = 0;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        //COLOR-instans
        private ColorSensing colorSensing;

        //DEPTH-instans
        private DepthSensing depthSensing;

        //BODY-instans
        private BodySensing bodySensning;

        ////----------------------
        ///Matlab-variabler
        /// Current directory
        string path = Path.Combine(Directory.GetCurrentDirectory());
        //MATLAB-instans 
        MLApp.MLApp matlab = new MLApp.MLApp();

        //Puls
        List<double> matlabPulsLista = new List<double>();

        //Andning
        List<double> listDepthMatlab = new List<double>();
        //----------------------

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

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

            //Graf
            viewModel = new ViewModel();
            listTest = new List<double> { 1, 2, 3 };

            // initialize the components (controls) of the window
            this.InitializeComponent();

            CompositionTarget.Rendering += CompositionTargetRendering;
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            Plot1.InvalidatePlot(true);
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

        public IList<DataPoint> Points
        {
            get
            {
                return viewModel.Points;
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
        }

        ///MATLAB-funktionen: Härifrån köra alla kommandon som har med matlab att göra.
        /// <param name="codeString">definierar detektion av puls ("pulse") eller andning ("breathing") som en sträng</param>
        /// <param name="measurements">innehåller all mätdata i form av en lista med floats</param>
        int antalFel = 0;
        private void matlabCommand(string codeString, List<double> measurements)
        {
            // Change to the directory  where the function is located 
            matlab.Execute(@"cd " + path + @"\..\..\..\matlab");
            //System.IO.File.WriteAllLines(@path + "data.text", measurements.ToString());

            // Define the output 
            object result = null;

            try
            {
                //Analys av puls i matlab
                if (codeString == "pulse")
                {
                    matlab.Feval("pulse_instant", 1, out result, measurements.ToArray());
                    object[] res = result as object[];
                    heartrate = Math.Round(Convert.ToDouble(res[0]));
                }
                //Analys av andning i matlab
                else if (codeString == "breathing")
                {
                    matlab.Feval("breathing_instant", 1, out result, measurements.ToArray());
                    object[] res = result as object[];
                    breathingrate = Math.Round(Convert.ToDouble(res[0]));
                }
                else
                {
                    Console.WriteLine("Matlabfunktionen kördes inte, kontrollera att codeString var korrekt");
                }

                //Skriver ut hjärtrytm och andningsrytm i programmet
                textBlock.Text = "Hjärtrytm: " + System.Environment.NewLine + heartrate + System.Environment.NewLine +
                    "Andningsrytm: " + System.Environment.NewLine + breathingrate;

            }
            catch
            {
                antalFel += 1;
                Console.WriteLine("antal kastade matlabfel: " + antalFel.ToString());
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


                            // Här tar vi ut alla röda värden i de intressanta pixlarna
                            List<int> rödapixlar = null;
                            rödapixlar = new List<int>();

                            // Rutan som följer HeadJoint
                            for (int i = (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) - 10);
                                i <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.X)) + 10); ++i)
                            {
                                for (int j = (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) - 10);
                                    j <= (Convert.ToInt32(Math.Round(colorSpaceHeadPoint.Y)) + 10); ++j)
                                {
                                    rödapixlar.Add(getcolorfrompixel(i, j, pixels, "green"));
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
                                rödapixlar.RemoveAt(0);
                            }

                            //Ta bort alla högsta värden
                            for (int i = (rödapixlar.Count / 5) * 4; i < rödapixlar.Count; i++)
                            {
                                rödapixlar.RemoveAt(rödapixlar.Count - 1);
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

                            if (matlabPulsLista.Count >= 900)
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

                            //Rensa listan från de X äldsta värdena om listan är över en viss längd
                            if (matlabPulsLista.Count >= 900)
                            {
                                for (int i = 0; i < 30; ++i)
                                {
                                    matlabPulsLista.RemoveAt(0);
                                }
                            }

                            // här ska methlab-funktionen köras--------------------^*************************^^,
                            //definiera hur ofta och hur stor listan är här innan.
                            if (matlabPulsLista.Count % 30 == 0)
                            {
                                matlabCommand("pulse", matlabPulsLista);
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
                            double average = 0;
                            DepthSpacePoint depthSpacePoint =
                                bodySensning.getCoordinateMapper().MapCameraPointToDepthSpace(bodySensning.getBellyJoint().Position);

                            //Jämför med en stationär Joint för att eliminera icke-andningesrelaterade rörelser
                            //OBS spineShoulder eller dylikt måste skapas
                            /*DepthSpacePoint depthSpacePointCompare = 
                                bodySensning.getCoordinateMapper().MapCameraPointToDepthSpace(bodySensning.getSpineShoulderJoint().Position);
                            double jointCompare = pixelData[Convert.ToInt32(Math.Round((depthSpacePointCompare.Y - 1) * 512 + depthSpacePointCompare.X))];*/

                            List<double> pixelDepthList = new List<double>();

                            //for-loop för att hämta djupvärdet i punkter utgående från midSpine
                            for (int ix = Convert.ToInt32(Math.Round(depthSpacePoint.X) - 5);
                                    ix <= Convert.ToInt32(Math.Round(depthSpacePoint.X) + 5); ix++)
                            {
                                for (int iy = Convert.ToInt32(Math.Round(depthSpacePoint.Y));
                                    iy <= Convert.ToInt32(Math.Round(depthSpacePoint.Y) + 10); iy++)
                                {
                                    pixelDepthList.Add(pixelData[((iy - 1) * 512 + ix)]);
                                }
                            }

                            //Filtrera pixelDepthList från extremvärden
                            for (int i = 0; i < pixelDepthList.Count / 5; i++)
                            {
                                pixelDepthList.RemoveAt(0);
                            }
                            for (int i = pixelDepthList.Count * (4/5); i <= pixelDepthList.Count; i++)
                            {
                                pixelDepthList.RemoveAt(pixelDepthList.Count - 1);
                            }


                            //for-loop följt av division för att ta fram medelvärdet över djupvärdena
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
                            //NYTT
                            if (listDepthMatlab.Count >= 900)
                            {
                                for (int i = 0; i < 30; ++i)
                                {
                                    listDepthMatlab.RemoveAt(0);
                                }
                            }

                            if (listDepthMatlab.Count % 30 == 0)
                            {
                                matlabCommand("breathing", listDepthMatlab);
                            }

                            for (int i = 0; i < listDepthMatlab.Count(); i++)
                            {
                                viewModel.AddDatapoint(i, listDepthMatlab[i]);
                            }
                            //INTE NYTT

                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            MessageBox.Show("Baby has escaped, he can't be far");
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
            matlabPulsLista.Clear();
        }
    }
}
