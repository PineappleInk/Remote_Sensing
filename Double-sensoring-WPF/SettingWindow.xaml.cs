using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private MainWindow mainWindow;

        public SettingWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            inputTextBreathing.Text = Convert.ToString(mainWindow.lowNumBreathing);
            inputTextPulse.Text = Convert.ToString(mainWindow.lowNumPulse);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.depthList.Clear();
            //mainWindow.getColorSensing().biglist.Clear();
        }

        private void Position_Low_Checked(object sender, RoutedEventArgs e)
        {
            Position_Normal.IsChecked = false;
            Position_High.IsChecked = false;
            mainWindow.setBellyJointYPosition(1);
            Console.WriteLine(mainWindow.bellyJointYPosition);
        }

        private void Position_Normal_Checked(object sender, RoutedEventArgs e)
        {
            if(Position_Low != null)
            {
                Position_Low.IsChecked = false;
            }           
            Position_High.IsChecked = false;
            mainWindow.setBellyJointYPosition(2/2.5);
            Console.WriteLine(mainWindow.bellyJointYPosition);
        }

        private void Position_High_Checked(object sender, RoutedEventArgs e)
        {
            Position_Low.IsChecked = false;
            Position_Normal.IsChecked = false;
            mainWindow.setBellyJointYPosition(2/2.7);
            Console.WriteLine(mainWindow.bellyJointYPosition);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            inputTextBreathing.Text = Convert.ToString(mainWindow.lowNumBreathing);
            inputTextPulse.Text = Convert.ToString(mainWindow.lowNumPulse);
            this.Hide();
        }

        public void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int inputnumber_b = Convert.ToInt32(inputTextBreathing.Text);
                if (inputnumber_b >= 2 && inputnumber_b <= 40)
                {
                    mainWindow.lowNumBreathing = inputnumber_b;
                }
                else
                {
                    inputTextBreathing.Text = Convert.ToString(mainWindow.lowNumBreathing);
                    System.Windows.MessageBox.Show("Invalid breathing alarm level! Choose a number between 2 and 40");
                }

                int inputnumber = Convert.ToInt32(inputTextPulse.Text);
                if (inputnumber >= 30 && inputnumber <= 200)
                {
                    mainWindow.lowNumPulse = inputnumber;
                }
                else
                {
                    inputTextPulse.Text = Convert.ToString(mainWindow.lowNumPulse);
                    System.Windows.MessageBox.Show("Invalid pulse alarm level ! Choose a number between 30 and 200");
                }

                if(inputnumber_b >= 2 && inputnumber_b <= 40 && inputnumber >= 30 && inputnumber <= 200)
                {
                    this.Hide();
                }
            }
            catch (System.FormatException)
            {
                inputTextBreathing.Text = Convert.ToString(mainWindow.lowNumBreathing);
                inputTextPulse.Text = Convert.ToString(mainWindow.lowNumPulse);
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.lowNumBreathing = 10;
            mainWindow.lowNumPulse = 30;
            inputTextBreathing.Text = Convert.ToString(mainWindow.lowNumBreathing);
            inputTextPulse.Text = Convert.ToString(mainWindow.lowNumPulse);
            checkBoxSound.IsChecked = false;
            Position_Low.IsChecked = false;
            Position_Normal.IsChecked = true;
            Position_High.IsChecked = false;
        }
    }

    //Funktionen ändrar gränsen för pulslarmet. Det finns ett satt tal från början som heter lowNumPulse.
    //Det är bara möjligt att ändra gränsen om den finns inom intervallet i if-satsen.

    //Funktionen ändrar gränsen för andningslarmet. Det finns ett satt tal från början som heter lowNumPulse.
    //Det är bara möjligt att ändra gränsen om den finns inom intervallet i if-satsen.

}
