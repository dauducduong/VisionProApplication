using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        private VisionControl _VisionControl;
        private CogToolBlock Job;
        private string FileJob = "";
        //private string modelName = "";
        private readonly List<string> _FileNameList = new List<string>();
        private readonly CogToolBlockEditV2 _ctbEdit;
        private int _selectedIndex;

        public MainWindow()
        {
            _Camera = new Camera();
            _Utility = new Utility();
            _Camera.VisionImageAvailable += _Camera_VisionImageAvailable;
            CogDisplay = new CogRecordDisplay();
            _ctbEdit = new CogToolBlockEditV2();
            InitializeComponent();
            WPFCogDisplay.Child = CogDisplay;
            WPFCogTool.Child = _ctbEdit;
            btnLive.IsEnabled = false;
            btnTrigger.IsEnabled = false;
            btnRunOnce.IsEnabled = false;
        }
        private void _Camera_VisionImageAvailable(object sender, Camera.VinsionImageAvailableEventArgs e)
        {
            if (_selectedIndex == 0)
            {
                CogDisplay.Image = e.Image;
            }
            else
            {
                _VisionControl.StartRunningOnce(e.Image.ToBitmap(), 0);
            }

        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.Content is string buttonText && buttonText == "🔗 CONNECT") 
            {
                _Camera.DeviceListAcq();
                if (_Camera.listCam.Count > 0)
                {
                    txtCamModel.Text = _Camera.listCam.First().Key;
                    btnConnect.Content = "⛓️‍💥 DISCONNECT";
                    btnTrigger.IsEnabled = true;
                    btnLive.IsEnabled = true;
                }
                if (_VisionControl != null)
                {
                    btnRunOnce.IsEnabled = true;
                }
            }
            else
            {
                if (_Camera != null)
                {
                    _Camera.DestroyCamera(0);
                    txtCamModel.Clear();
                    btnTrigger.IsEnabled = false;
                    btnLive.IsEnabled = false;
                    btnConnect.Content = "🔗 CONNECT";
                    CogDisplay = new CogRecordDisplay();
                    WPFCogDisplay.Child = CogDisplay;
                    btnRunOnce.IsEnabled = false;
                }
            }

            
        }

        private void btnLive_Click(object sender, RoutedEventArgs e)
        {
            if (btnLive.Background is SolidColorBrush brush && brush.Color == System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD))
            {
                CogFrameGrabbers mFrameGrabbers = new CogFrameGrabbers();
                if (mFrameGrabbers.Count > 0)
                {
                    _Camera.SetupRunContinuos(0);
                    CogDisplay.StartLiveDisplay(_Camera.mCamera[0].mAcqFifo);
                    btnConnect.IsEnabled = false;
                    btnTrigger.IsEnabled = false;
                    btnRunOnce.IsEnabled = false;
                    btnLive.Background = new SolidColorBrush(Colors.LimeGreen);
                }
                else
                {
                    System.Windows.MessageBox.Show("No camera found!");
                }
            }
            else
            {
                btnLive.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
                btnTrigger.IsEnabled = true;
                btnConnect.IsEnabled = true;
                btnRunOnce.IsEnabled = true;
                CogDisplay.StopLiveDisplay();
                //Reset CogDisplay
                //CogDisplay = new CogRecordDisplay();
                //WPFCogDisplay.Child = CogDisplay;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Camera.DestroyCamera(0);
        }

        private void btnTrigger_Click(object sender, RoutedEventArgs e)
        {
            _Camera.RunOnce(0);
        }

        private void btnLoadJob_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "JobFile |*.vpp"
            };
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileJob = open.FileName;
                //modelName = FileJob.Split("\\".ToCharArray())[FileJob.Split("\\".ToCharArray()).Length - 2];
                _FileNameList.Add(FileJob);
                try
                {
                    Job = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileJob);
                    List<CogToolBlock> _mtoolblockManager = new List<CogToolBlock>();
                    _mtoolblockManager.Add(Job);
                    _VisionControl = new VisionControl(ref _mtoolblockManager);
                    _ctbEdit.Subject = Job;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
                if (_Camera.listCam.Count > 0)
                {
                    btnRunOnce.IsEnabled = true;
                }
            }
        }


        private void TxtExposureNum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _Camera.SetExposure(0, Convert.ToInt32(txtExposureNum.Value));
        }

        private void btnRunOnce_Click(object sender, RoutedEventArgs e)
        {
            _Camera.RunOnce(0);
            //_VisionControl.StartRunningOnce(captureImage, 0);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            if (tabControl != null)
            {
                _selectedIndex = tabControl.SelectedIndex;
            }
        }
    }
}
