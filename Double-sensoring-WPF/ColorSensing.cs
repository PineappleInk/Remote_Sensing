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

        public List<List<double>> biglist = new List<List<double>>();
        
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

        public List<List<double>> createBigList(List<int> rödapixlar, List<int> grönapixlar, List<int> blåapixlar)
        {
            rödapixlar.Sort();
            grönapixlar.Sort();
            blåapixlar.Sort();

            //Ta bort alla lägsta värden
            for (int i = 0; i < rödapixlar.Count / 5; i++)
            {
                rödapixlar.RemoveAt(0);
                grönapixlar.RemoveAt(0);
                blåapixlar.RemoveAt(0);
            }

            //Ta bort alla högsta värden
            for (int i = (rödapixlar.Count / 5) * 4; i < rödapixlar.Count; i++)
            {
                rödapixlar.RemoveAt(rödapixlar.Count - 1);
                grönapixlar.RemoveAt(rödapixlar.Count - 1);
                blåapixlar.RemoveAt(rödapixlar.Count - 1);
            }

            //Medelvärde av de röda kanalerna i intressanta pixlar
            double redcoloraverage = 0;

            for (int i = 0; i < rödapixlar.Count; i++)
            {
                redcoloraverage += rödapixlar[i];
            }
            redcoloraverage = (redcoloraverage / rödapixlar.Count);

            //Medelvärde av de röda kanalerna i intressanta pixlar
            double greencoloraverage = 0;

            for (int i = 0; i < grönapixlar.Count; i++)
            {

                greencoloraverage += grönapixlar[i];
            }
            greencoloraverage = (greencoloraverage / grönapixlar.Count);

            //Medelvärde av de röda kanalerna i intressanta pixlar
            double bluecoloraverage = 0;

            for (int i = 0; i < blåapixlar.Count; i++)
            {

                bluecoloraverage += blåapixlar[i];
            }
            bluecoloraverage = (bluecoloraverage / blåapixlar.Count);

            if (biglist.Count == 0)
            {
                biglist.Add(new List<double>());
                biglist.Add(new List<double>());
                biglist.Add(new List<double>());
            }

            biglist[0].Add(redcoloraverage);
            biglist[1].Add(greencoloraverage);
            biglist[2].Add(bluecoloraverage);

            if (biglist[1].Count >= 900)
            {
                biglist[0].RemoveAt(0);
                biglist[1].RemoveAt(0);
                biglist[2].RemoveAt(0);
            }

            rödapixlar.Clear();
            grönapixlar.Clear();
            blåapixlar.Clear();

            //Rensa listan från de X äldsta värdena om listan är över en viss längd
            if (biglist[1].Count >= 900)
            {
                for (int i = 0; i < 30; ++i)
                {
                    biglist[0].RemoveAt(0);
                    biglist[1].RemoveAt(0);
                    biglist[2].RemoveAt(0);
                }
            }
            return biglist;
        }

        public List<List<double>> createBigList2(List<int> rödapixlar, List<int> grönapixlar, List<int> blåapixlar)
        {
            // Grön / (Röd + Blå)
            List<double> gDrPb = new List<double>();

            // Grön / Röd
            List<double> gDr = new List<double>();

            // Shradinkar
            List<double> shra = new List<double>();
            List<double> shraRödGrön = new List<double>();
            List<double> shraAlla = new List<double>();
            List<double> shraHB = new List<double>();

            for (int i = 0; i < rödapixlar.Count; i++)
            {
                gDrPb.Add((double)grönapixlar[i]/((double)rödapixlar[i]+(double)blåapixlar[i]));
                gDr.Add((double)grönapixlar[i] / (double)rödapixlar[i]);

                shraRödGrön.Add(rödapixlar[i] - grönapixlar[i]);
                shraAlla.Add(rödapixlar[i] + grönapixlar[i] - 2 * blåapixlar[i]);
            }
            //Console.WriteLine("röda: " + rödapixlar[20] + ", gröna: " + grönapixlar[20] + ", blåa: " + blåapixlar[20] + ", gDrPb: " + gDrPb[20] + ", gDr: " + gDr[20]);

            // SHRADINKAR BÖRJAR

            double shraRödGrönMedel = 0;
            double shraAllaMedel = 0;
            for (int i = 0; i < shraRödGrön.Count; i++)
            {
                shraRödGrönMedel += shraRödGrön[i];
                shraAllaMedel += shraAlla[i];
            }
            shraRödGrönMedel = shraRödGrönMedel / shraRödGrön.Count;
            shraAllaMedel = shraAllaMedel / shraAlla.Count;

            for (int i = 0; i < rödapixlar.Count; i++)
            {
                shraRödGrön[i] = shraRödGrön[i] - shraRödGrönMedel;
                shraAlla[i] = shraAlla[i] - shraAllaMedel;
            }

            double shraRödGrönStd = 0;
            double shraAllaStd = 0;
            for (int i = 0; i < shraRödGrön.Count; i++)
            {
                shraRödGrönStd += (shraRödGrön[i] - shraRödGrönMedel) * (shraRödGrön[i] - shraRödGrönMedel);
                shraAllaStd += (shraAlla[i] - shraAllaMedel) * (shraAlla[i] - shraAllaMedel);
            }
            shraRödGrönStd = Math.Sqrt(shraRödGrönStd / shraRödGrön.Count);
            shraAllaStd = Math.Sqrt(shraAllaStd / shraAlla.Count);

            for (int i = 0; i < shraAlla.Count; i++)
            {
                shraAlla[i] = shraRödGrönStd / shraAllaStd * shraAlla[i];
                shraHB.Add(shraRödGrön[i] - shraAlla[i]);
            }

            double shraHBMedel = 0;
            for (int i = 0; i < shraHB.Count; i++)
            {
                shraHBMedel += shraHB[i];
            }
            shraHBMedel = shraHBMedel / shraHB.Count;

            double shraHBStd = 0;
            for (int i = 0; i < shraHB.Count; i++)
            {
                shraHBStd += (shraHB[i] - shraHBMedel) * (shraHB[i] - shraHBMedel);
            }
            shraHBStd = Math.Sqrt(shraHBStd / shraHB.Count);

            for (int i = 0; i < shraHB.Count; i++)
            {
                shraHB[i] = shraHB[i] / shraHBStd;
            }

            // SHRADINKAR SLUT

            gDrPb.Sort();
            gDr.Sort();
            shraHB.Sort();

            gDrPb.RemoveRange(0, gDrPb.Count / 5);
            gDr.RemoveRange(0, gDr.Count / 5);
            shraHB.RemoveRange(0, shraHB.Count / 5);
            
            for (int i = (gDrPb.Count / 4) * 2; i < gDrPb.Count; i++)
            {
                gDrPb.RemoveAt(gDrPb.Count - 1);
                gDr.RemoveAt(gDr.Count - 1);
                shraHB.RemoveAt(shraHB.Count - 1);
            }

            double gDrPbAverage = 0;
            double gDrAverage = 0;
            double shraHBAverage = 0;

            for (int i = 0; i < gDrPb.Count; i++)
            {
                gDrPbAverage += gDrPb[i];
            }
            gDrPbAverage = gDrPbAverage / gDrPb.Count;
            
            for (int i = 0; i < gDr.Count; i++)
            {
                gDrAverage += gDr[i];
            }
            gDrAverage = gDrAverage / gDr.Count;

            for (int i = 0; i < shraHB.Count; i++)
            {
                shraHBAverage += shraHB[i];
            }
            shraHBAverage = shraHBAverage / shraHB.Count;

            if (biglist.Count == 0)
            {
                biglist.Add(new List<double>());
                biglist.Add(new List<double>());
                biglist.Add(new List<double>());
            }

            biglist[0].Add(gDrPbAverage);
            biglist[1].Add(gDrAverage);
            biglist[2].Add(shraHBAverage);
            
            rödapixlar.Clear();
            grönapixlar.Clear();
            blåapixlar.Clear();

            //Rensa listan från de X äldsta värdena om listan är över en viss längd
            if (biglist[1].Count >= 900)
            {
                biglist[0].RemoveRange(0, 30);
                biglist[1].RemoveRange(0, 30);
                biglist[2].RemoveRange(0, 30);
            }

            //Console.WriteLine("1: " + gDrPbAverage + ", 2: " + gDrAverage + ", 3: " + shraHBAverage);
            return biglist;
        }

    }
}
