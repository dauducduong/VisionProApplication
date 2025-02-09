using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.QuickBuild.Implementation.Internal;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        private CogRecordDisplay CogResultDisplay { get; set; }
        private readonly Camera _Camera;
        //private readonly Utility _Utility;
        private VisionControl _VisionControl;
        private CogToolBlock Job;
        private string FileJob = "";
        //private string modelName = "";
        private readonly List<string> _FileNameList = new List<string>();
        private readonly CogToolBlockEditV2 _ctbEdit;
        private int _selectedIndex;
        private ImageManager imageManager = new ImageManager();
        private int _okCount;
        private int _ngCount;
        private int _totalCount;


        public MainWindow()
        {
            _Camera = new Camera();
            //_Utility = new Utility();
            _Camera.VisionImageAvailable += _Camera_VisionImageAvailable;
            
            CogDisplay = new CogRecordDisplay();
            CogResultDisplay = new CogRecordDisplay();
            _ctbEdit = new CogToolBlockEditV2();
            InitializeComponent();
            WPFCogDisplay.Child = CogDisplay;
            WPFCogTool.Child = _ctbEdit;
            WPFResultDisplay.Child = CogResultDisplay;
            btnLive.IsEnabled = false;
            btnTrigger.IsEnabled = false;
            btnRunOnce.IsEnabled = false;
            btnRunOncePB.IsEnabled = false;
            btnSaveJob.IsEnabled = false;
            btnNextImg.IsEnabled = false;
            btnPrevImg.IsEnabled = false;
            _okCount = 0;
            _ngCount = 0;
            _totalCount = 0;
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
                    _Camera.SetExposure(0, Convert.ToInt32(txtExposureNum.Text));
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
                    _VisionControl.VisionControlUserResultAvailable += _VisionControl_VisionControlUserResultAvailable;
                    _VisionControl.AttachToJobManager(true);
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
                if (imageManager.GetCount() > 0)
                {
                    btnRunOncePB.IsEnabled = true;
                }
                btnSaveJob.IsEnabled = true;
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

        private void btnRunOncePB_Click(object sender, RoutedEventArgs e)
        {
            if (imageManager.GetCurrentIndex() == imageManager.GetCount() - 1)
            {
                imageManager.ResetIndex();
            }
            else
            {
                imageManager.SetNextIndex();
            }
            _VisionControl.StartRunningOnce(imageManager.GetCurrentImage(), 0);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            if (tabControl != null)
            {
                _selectedIndex = tabControl.SelectedIndex;
            }
        }

        private void btnSaveJob_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                Filter = "JobFile |*.vpp"
            };
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    CogSerializer.SaveObjectToFile(FileJob, save.FileName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
        }

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    imageManager.LoadImagesToMemory(folderPath);
                    if (imageManager.GetCurrentIndex() != -1)
                    {
                        ShowImage();
                        btnNextImg.IsEnabled = true;
                        if (_VisionControl != null)
                        {
                            btnRunOncePB.IsEnabled = true;
                        }
                    }
                    
                }
            }
        }
        private void ShowImage()
        {
            CogDisplay.Image = imageManager.ConvertBitmapToCogImage(imageManager.GetCurrentImage());
            txtImageName.Text = imageManager.GetCurrentFileName();
            txtImageCount.Text = "File Name" + " (" + (imageManager.GetCurrentIndex() + 1).ToString() + "/" + imageManager.GetCount().ToString() + ")";
            if (imageManager.GetCurrentIndex() == imageManager.GetCount() - 1)
            {
                btnNextImg.IsEnabled = false;
            }
            else
            {
                btnNextImg.IsEnabled = true;
            }
            if (imageManager.GetCurrentIndex() == 0)
            {
                btnPrevImg.IsEnabled = false;
            }
            else
            {
                btnPrevImg.IsEnabled = true;
            }
        }

        private void btnPrevImg_Click(object sender, RoutedEventArgs e)
        {
            imageManager.SetPrevIndex();
            ShowImage();
        }

        private void btnNextImg_Click(object sender, RoutedEventArgs e)
        {
            imageManager.SetNextIndex();
            ShowImage();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _VisionControl.StartRunningOnce(imageManager.GetCurrentImage(), 0);
        }

        private void _VisionControl_VisionControlUserResultAvailable(object sender, VisionControl.VisionControlUserResultAvailableEventArgs e)
        {
            var result = e.Result;
            try
            {
                var jobPass = e.JobStatus.Result;
                if (result != null)
                {
                    //bool totalResult = (bool)result["TotalResult"].Value;
                    CogResultDisplay.Record = e.LastRunRecord.SubRecords[0];
                    //CogResultDisplay.AutoFit = true;
                    if (jobPass == CogToolResultConstants.Accept)
                    {
                        _okCount++;
                    }
                    else 
                    {
                        _ngCount++;
                    }
                    _totalCount = _okCount + _ngCount;
                    txtOkCount.Text = _okCount.ToString();
                    txtNgCount.Text = _ngCount.ToString();
                    txtTotalCount.Text = _totalCount.ToString();
                    
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        private void TabItem1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (imageManager.GetCount() > 0)
            {
                if (imageManager.GetCurrentFileName() != txtImageName.Text)
                {
                    ShowImage();
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _okCount = 0;
            _ngCount = 0;
            _totalCount = 0;
            txtOkCount.Text = "0";
            txtNgCount.Text = "0";
            txtTotalCount.Text = "0";
        }
    }
}
