using Cognex.VisionPro.Exceptions;
using S7.Net;
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
using VisionProApplication.Tools;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace VisionProApplication
{
    /// <summary>
    /// Interaction logic for PlcWindow.xaml
    /// </summary>
    
    public partial class PlcWindow : Window
    {
        private PlcList _plcList;
        private PlcSiemensComm _plc;
        public bool IsConnected { get; private set; }
        public string PlcModel { get; private set; }


        public PlcWindow(PlcSiemensComm plc)
        {
            InitializeComponent();
            _plc = plc;
            LoadPLCList();
            IsConnected = false;
        }
        private void LoadPLCList()
        {
            _plcList = new PlcList();
            cbbPlcBrand.ItemsSource = _plcList.PlcBrands;
            cbbPlcBrand.SelectedIndex = 0;
            cbbPlcType.ItemsSource = _plcList.PlcBrands[0].Types;
            cbbPlcType.SelectedIndex = 0;
        }

        private void cbbPlcBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbPlcBrand.SelectedItem is PlcBrand selectedBrand)
            {
                cbbPlcType.ItemsSource = selectedBrand.Types; // Gán loại PLC tương ứng với hãng đã chọn
                cbbPlcType.SelectedIndex = 0; // Chọn loại đầu tiên mặc định
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = txtIpAddress.Text;
            short rack = Convert.ToInt16(txtRack.Text);
            short slot = Convert.ToInt16(txtSlot.Text);

            _plc.Connect(ipAddress, rack, slot);
            if (_plc.IsConnected)
            {
                PlcModel = cbbPlcType.SelectedItem as string;
                IsConnected = true;
                MessageBox.Show("Sucessfully connected to PLC", "Connect notification", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
