using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VisionProApplication.Tools;

namespace VisionProApplication
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private int _savingOption;
        private string _savingDir;
        private string solutionPath;
        public SettingWindow()
        {
            InitializeComponent();
            _savingOption = 0;
            _savingDir = "";
            btnChooseDir.IsEnabled = false;
            solutionPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName)?.FullName)?.FullName;
            txtCurrentDir.Text = "Current saving directory: " + solutionPath;
        }

        public event Action<SettingData> SavingOptionChanged;
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Confirm to apply these setting?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SettingData data = new SettingData(_savingOption, _savingDir);
                SavingOptionChanged?.Invoke(data);
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbbSaveOption_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cbbSaveOption = sender as System.Windows.Controls.ComboBox;
            if (cbbSaveOption.SelectedItem is ComboBoxItem selectedItem)
            {
                _savingOption = Convert.ToInt32(selectedItem.Tag.ToString());
                if (_savingOption != 0)
                {
                    btnChooseDir.IsEnabled = true;
                    string folderPath = "";
                    switch (_savingOption)
                    {
                        case 1: 
                            folderPath = "Run\\All Images";
                            break;
                        case 2:
                            folderPath = "Run\\OK Images";
                            break;
                        case 3:
                            folderPath = "Run\\NG Images";
                            break;
                    }
                    _savingDir = Path.Combine(solutionPath, folderPath);
                    txtCurrentDir.Text = "Current saving directory: " + _savingDir;
                }
                else
                {
                    if (btnChooseDir != null)
                    {
                        btnChooseDir.IsEnabled = false;
                        _savingDir = Path.Combine(solutionPath, "Run");
                        txtCurrentDir.Text = "Current saving directory: " + _savingDir;
                    }
                }
            }

        }

        private void btnChooseDir_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chọn thư mục để lưu file";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _savingDir = dialog.SelectedPath;
                    txtCurrentDir.Text = "Current saving directory: " + _savingDir;
                }
            }
        }

    }
}
