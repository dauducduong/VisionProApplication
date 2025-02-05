using Cognex.VisionPro;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionProApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CogRecordDisplay CogDisplay { get; set; }
        private readonly Camera _Camera;
        private readonly Utility _Utility;
        private readonly VisionControl _VisionControl;

        public MainWindow()
        {
            _Camera = new Camera();
            _Utility = new Utility();
            _Camera.VisionImageAvailable += _Camera_VisionImageAvailable;
            CogDisplay = new CogRecordDisplay();
            InitializeComponent();
            WPFCogDisplay.Child = CogDisplay;
        }
        private void _Camera_VisionImageAvailable(object sender, Camera.VinsionImageAvailableEventArgs e)
        {
            CogDisplay.Image = e.Image;

        }
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            CogFrameGrabbers mFrameGrabbers = new CogFrameGrabbers();
            if (mFrameGrabbers.Count > 0)
        }

        private void exposureNum_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _Camera.SetExposure(0, (int)exposureNum.Value);
        }

        private void btnLive_Click(object sender, RoutedEventArgs e)
        {
            CogFrameGrabbers mFrameGrabbers = new CogFrameGrabbers();
            if (mFrameGrabbers.Count > 0)
            {
                _Camera.SetupRunContinuos(0);
                CogDisplay.StartLiveDisplay(_Camera.mCamera[0].mAcqFifo);
            }
            else
            {
                MessageBox.Show("No camera found!");
            }
        }
    }
}
