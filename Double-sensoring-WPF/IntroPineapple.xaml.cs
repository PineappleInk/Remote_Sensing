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

namespace Microsoft.Samples.Kinect.BodyBasics
{
    /// <summary>
    /// Interaction logic for IntroPineapple.xaml
    /// </summary>

    public partial class IntroPineapple : Window
    {
        //räknare till 360
        int introCounter = 0;

        public IntroPineapple(string str)
        {
            /*System.Media.SoundPlayer sound = new System.Media.SoundPlayer();
            sound.SoundLocation = str;
            sound.Play();*/
            InitializeComponent();
        }

        //Funktion för att ananasen ska snurra
        public int spinPineapple()
        {
            if (introCounter < 360)
            {
                pineappleImage.Height -= 1;
                pineappleImage.Width -= 1;
                RotateTransform rotateTransform = new RotateTransform(introCounter*2);
                pineappleImage.RenderTransform = rotateTransform;
                introCounter += 1;
            }
            return introCounter;
        }
    }
}
