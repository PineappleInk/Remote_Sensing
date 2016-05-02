using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    /// <summary>
    /// Interaction logic for Alarm.xaml
    /// </summary>
    public partial class Alarm : Window
    {
        MainWindow parent;
        KinectSensor kinect;
        System.Media.SoundPlayer beep;
        string soundpath;
        System.Windows.Threading.DispatcherTimer alarmTimer = new System.Windows.Threading.DispatcherTimer();

        public Alarm(MainWindow mw, KinectSensor ks, string p)
        {
            parent = mw;
            kinect = ks;
            soundpath = System.IO.Path.Combine(p + @"\..\..\..\beep-07.wav");
            beep = new System.Media.SoundPlayer();
            beep.SoundLocation = soundpath;

            //Timer start
            alarmTimer.Tick += soundAlarm;
            alarmTimer.Interval = new TimeSpan(1500000);
            alarmTimer.Start();

            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            parent.Show();
            kinect.Open();
            this.Close();
            alarmTimer.Stop();
        }

        private void soundAlarm(object sender, EventArgs e)
        {
            alarmTimer.Stop();
            if (parent.settingWindow.checkBoxSound.IsChecked == true)
            {
                beep.Play();
            }
            colorAlarm(sender, e);
            alarmTimer.Start();
        }

        private void colorAlarm(object sender, EventArgs e)
        {
            if (this.Background == Brushes.WhiteSmoke)
            {
                Background = Brushes.Red;
            }
            else
            {
                Background = Brushes.WhiteSmoke;
            }

            //Timern startas om i soundAlarm() iställer för här!
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            okButton_Click(sender, e);
        }
    }
}
