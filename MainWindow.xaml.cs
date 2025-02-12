#region Namespace declaration
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using VisionProApplication.Tools;
using Cognex.VisionPro.Display;
#endregion

namespace VisionProApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variable declaration
        private CogRecordDisplay _CogDisplay { get; set; }
        private CogRecordDisplay _CogResultDisplay { get; set; }
        private readonly Camera _Camera;
        private VisionControl _VisionControl;
        private CogToolBlock Job;
        private string FileJob = "";
        private readonly List<string> _FileNameList = new List<string>();
        private readonly CogToolBlockEditV2 _CogToolBlockDisplay;
        private int _selectedIndex;
        private ImageManager imageManager = new ImageManager();
        private int _okCount;
        private int _ngCount;
        private int _totalCount;
        private bool _isCameraOpened;
        private bool _isPlaybackOpened;
        bool _isRunning;
        bool _isJobLoaded;
        int _savingOption;
        string _savingDir;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            #region Variable assignment
            _Camera = new Camera();
            _Camera.VisionImageAvailable += Camera_VisionImageAvailable;
            _CogDisplay = new CogRecordDisplay();
            _CogResultDisplay = new CogRecordDisplay();
            _CogToolBlockDisplay = new CogToolBlockEditV2();
            WPFCogDisplay.Child = _CogDisplay;
            WPFCogTool.Child = _CogToolBlockDisplay;
            WPFResultDisplay.Child = _CogResultDisplay;
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
            _savingOption = 0;
            _savingDir = "";
            _isCameraOpened = false;
            _isPlaybackOpened = false;
            _isRunning = false;
            _isJobLoaded = false;
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnSetting.IsEnabled = false;
            #endregion
        }

        #region Camera method
        private void Camera_VisionImageAvailable(object sender, Camera.VinsionImageAvailableEventArgs e)
        {
            //Khi camera trigger và chụp được 1 ảnh mới e.Image
            if (!_isRunning)  //Nếu không ở chế độ 3. Run
            {
                if (_selectedIndex == 0) //Đang trigger cam để lấy ảnh test ở 1. Connect
                {
                    _CogDisplay.Image = e.Image;
                }
                else
                {
                    if (_selectedIndex == 1) //Thử chạy tool block với 1 ảnh trigger từ cam ở 2. Program
                    {
                        _VisionControl.StartRunningOnce(e.Image.ToBitmap(), 0);
                    }
                }
            }
            else //Ở chế độ Run
            {
                _VisionControl.StartRunningOnce(e.Image.ToBitmap(), 0);
            }

        }

        private void VisionControl_VisionControlUserResultAvailable(object sender, VisionControl.VisionControlUserResultAvailableEventArgs e)
        {
            //Khi ToolBlock chạy xong, hàm này sẽ được chạy
            if (_isRunning) //Nếu đang ở chế độ RUN
            {
                var result = e.Result;
                try
                {
                    var jobPass = e.JobStatus.Result;
                    if (result != null)
                    {
                        //Hiển thị ảnh kết quả record
                        _CogResultDisplay.Record = e.LastRunRecord.SubRecords[0];
                        //Xử lý bộ đếm OK, NG
                        if (jobPass == CogToolResultConstants.Accept)
                        {
                            _okCount++;
                        }
                        else
                        {
                            _ngCount++;
                        }
                        _totalCount = _okCount + _ngCount;
                        //Save file
                        if (_savingOption != 0)
                        {
                            switch (_savingOption)
                            {
                                case 1:
                                    SaveResultImage();
                                    break;
                                case 2 when jobPass == CogToolResultConstants.Accept:
                                case 3 when jobPass == CogToolResultConstants.Reject:
                                    SaveResultImage();
                                    break;
                            }
                        }
                        // Cập nhật UI trên UI thread
                        Dispatcher.Invoke(() =>
                        {
                            txtOkCount.Text = _okCount.ToString() + "(" + Math.Round(((_okCount / (double)_totalCount) * 100), 2).ToString() + "%)";
                            txtNgCount.Text = _ngCount.ToString() + "(" + (100 - Math.Round(((_okCount / (double)_totalCount) * 100), 2)).ToString() + "%)";
                            txtTotalCount.Text = _totalCount.ToString();
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
        }

        private void TxtExposureNum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_Camera != null)
            {
                _Camera.SetExposure(0, Convert.ToInt32(txtExposureNum.Value));
            }
        }

        private void ShowImage()
        {
            //Hiển thị ảnh, tên file, thứ tự trong list (Chỉ dùng cho PLAYBACK)
            _CogDisplay.Image = imageManager.ConvertBitmapToCogImage(imageManager.GetCurrentImage());
            txtImageName.Text = imageManager.GetCurrentFileName();
            txtImageCount.Text = "File Name" + " (" + (imageManager.GetCurrentIndex() + 1).ToString() + "/" + imageManager.GetCount().ToString() + ")";
            //Nếu đang là ảnh cuối của list thì disable nút next ảnh, còn ảnh đầu thì disable nút back ảnh
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

        private void SaveResultImage()
        {
            if (!Directory.Exists(_savingDir))
            {
                Directory.CreateDirectory(_savingDir);
            }

            if (_CogResultDisplay == null) throw new ArgumentNullException(nameof(_CogResultDisplay));
            try
            {
                // Chụp ảnh từ Display (bao gồm cả đồ họa vẽ)
                Bitmap bitmap = (Bitmap)_CogResultDisplay.CreateContentBitmap(CogDisplayContentBitmapConstants.Image);
                // Lưu ảnh chụp lại
                int count = Directory.GetFiles(_savingDir, "image*.bmp").Length + 1;
                string filePath = System.IO.Path.Combine(_savingDir, $"image{count}.bmp");
                bitmap.Save(filePath, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Window method
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_Camera.camCount > 0)
            {
                _Camera.DestroyCamera(0);
            }
            System.Windows.Application.Current.Shutdown();
            
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            if (tabControl != null)
            {
                _selectedIndex = tabControl.SelectedIndex;
            }
        }

        private void TabItem1_GotFocus(object sender, RoutedEventArgs e)
        {
            //Khi chọn vào tab 1 thì cập nhật lại ảnh hiển thị playback nếu đã ấn RunOncePlayBack ở tab 2
            if (imageManager.GetCount() > 0)
            {
                if (imageManager.GetCurrentFileName() != txtImageName.Text)
                {
                    ShowImage();
                }
            }
        }

        private void OnSettingChanged(SettingData data)
        {
            _savingOption = data.SavingOption;
            _savingDir = data.SavingDir;
        }
        #endregion

        #region Button method
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.Content is string buttonText && buttonText == "🔗 CONNECT") //Ấn CONNECT
            {
                _Camera.DeviceListAcq(); //Kết nối camera
                if (_Camera.listCam.Count > 0)
                {
                    txtCamModel.Text = _Camera.listCam.First().Key;
                    _Camera.SetExposure(0, Convert.ToInt32(txtExposureNum.Text));
                    btnConnect.Content = "⛓️‍💥 DISCONNECT";
                    btnTrigger.IsEnabled = true;
                    btnLive.IsEnabled = true;
                    _isCameraOpened = true;
                    //Nếu đã load tool block, enable các nút Run, Start
                    if (_isJobLoaded) 
                    {
                        btnRunOnce.IsEnabled = true;
                        btnStart.IsEnabled = true;
                    }
                }
            }
            else
            {
                if (_Camera != null) //Ấn DISCONNECT
                {
                    _Camera.DestroyCamera(0);
                    txtCamModel.Clear();
                    btnTrigger.IsEnabled = false;
                    btnLive.IsEnabled = false;
                    btnConnect.Content = "🔗 CONNECT";
                    _CogDisplay = new CogRecordDisplay();
                    WPFCogDisplay.Child = _CogDisplay;
                    btnRunOnce.IsEnabled = false;
                    _isCameraOpened = false;
                    //Nếu chưa load ảnh thì disable nút Start
                    if (!_isPlaybackOpened) 
                    {
                        btnStart.IsEnabled = false;
                    }
                }
            }


        }

        private void btnLive_Click(object sender, RoutedEventArgs e)
        {
            //Bật LIVE
            if (btnLive.Background is SolidColorBrush brush && brush.Color == System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD))
            {
                if (_isCameraOpened)
                {
                    _Camera.SetupRunContinuos(0);
                    _CogDisplay.StartLiveDisplay(_Camera.mCamera[0].mAcqFifo);
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
            //Tắt LIVE
            else
            {
                btnLive.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
                btnTrigger.IsEnabled = true;
                btnConnect.IsEnabled = true;
                btnRunOnce.IsEnabled = true;
                _CogDisplay.StopLiveDisplay();
                //Reset CogDisplay
                //CogDisplay = new CogRecordDisplay();
                //WPFCogDisplay.Child = CogDisplay;
            }
        }

        private void btnTrigger_Click(object sender, RoutedEventArgs e)
        {
            _Camera.RunOnce(0);
        }

        private void btnLoadJob_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "JobFile |*.vpp" //Chỉ load file lưu CogToolBlock
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
                    _VisionControl.VisionControlUserResultAvailable += VisionControl_VisionControlUserResultAvailable;
                    _VisionControl.AttachToJobManager(true);
                    _CogToolBlockDisplay.Subject = Job;
                    _isJobLoaded = true;
                    if (_isCameraOpened) //Nếu đã kết nối cam
                    {
                        btnRunOnce.IsEnabled = true;
                        btnStart.IsEnabled = true;
                    }
                    if (_isPlaybackOpened) //Nếu đã load ảnh
                    {
                        btnRunOncePB.IsEnabled = true;
                        btnStart.IsEnabled = true;
                    }
                    btnSaveJob.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                } 
            }
        }

        private void btnRunOnce_Click(object sender, RoutedEventArgs e)
        {
            _Camera.RunOnce(0);
            //Chụp thành công sẽ kích hoạt hàm _Camera_VisionImageAvailable
        }

        private void btnRunOncePB_Click(object sender, RoutedEventArgs e)
        {
            //Chạy toolblock 1 lần với ảnh playback. Ảnh dùng để chạy là ảnh tiếp theo so với ảnh đang chọn ở 1. Connect
            //Nếu ảnh hiện tại là ảnh cuối của list thì quay về ảnh đầu
            if (imageManager.GetCurrentIndex() == imageManager.GetCount() - 1)
            {
                imageManager.ResetIndex();
            }
            //Nếu không phải ảnh cuối thì nhảy đến ảnh kế tiếp
            else
            {
                imageManager.SetNextIndex();
            }
            //Chạy toolblock
            _VisionControl.StartRunningOnce(imageManager.GetCurrentImage(), 0);
        }

        private void btnSaveJob_Click(object sender, RoutedEventArgs e)
        {
            //Lưu toolblock
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
                        _isPlaybackOpened = true;
                        if (_isJobLoaded)   //Nếu đã load toolblock
                        {
                            btnRunOncePB.IsEnabled = true;
                            btnStart.IsEnabled = true;
                        }
                    }
                    
                }
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

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnReset.IsEnabled = true;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = true;
            btnStop.IsEnabled = true;
            btnStart.IsEnabled = false;
            btnReset.IsEnabled = false;
            //Chương trình
            await Task.Run(() =>
            {
                while (_isRunning)
                {
                    //Nếu cam đang kết nối thì chạy chương trình với cam, còn không thì chạy với ảnh PLAYBACK
                    if (_isCameraOpened)
                    {
                        //Không cần làm gì cả vì khi có ảnh mới được chụp, hàm _Camera_VisionImageAvailable sẽ tự động được chạy.
                    }
                    else
                    {
                        //Nếu không có cam kết nối
                        if (_isPlaybackOpened) 
                        {
                            foreach (ImageItem item in imageManager.GetImageItemList())
                            {
                                _VisionControl.StartRunningOnce(item.Image, 0);
                                Task.Delay(200).Wait();
                            }
                        }
                    }
                    
                }
            });
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

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (btnLogin.Content.ToString() == "🔑 LOG IN")
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.ShowDialog();
                if (loginWindow.IsAuthenticated)
                {
                    btnSetting.IsEnabled = true;
                    btnLogin.Content = "🔒 LOG OUT";
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to log out?", "Log Out Confirmation",
                                             MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    btnSetting.IsEnabled = false;
                    btnLogin.Content = "🔑 LOG IN";
                }
                
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.SavingOptionChanged += OnSettingChanged; //Khi nào ấn Apply ở cửa sổ Setting thì hàm OnSettingChanged được kích hoạt
            settingWindow.ShowDialog();
        }
        #endregion
    }
}
