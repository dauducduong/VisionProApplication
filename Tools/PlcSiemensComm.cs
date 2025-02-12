using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisionProApplication.Tools
{
    public class PlcSiemensComm
    {
        private Plc plc;
        public bool Connected { get; private set; }

        // Kết nối PLC
        public void Connect(string cputype, string ipAddress, short rack, short slot)
        {
            string formattedCpuType = cputype.Replace("-", "");
            // Chuyển string thành enum
            if (!Enum.TryParse(formattedCpuType, out CpuType cpuModel))
            {
                MessageBox.Show("Invalid CPU type!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                plc = new Plc(cpuModel, ipAddress, rack, slot);
                plc.Open();
                Connected = plc.IsConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PLC Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Connected = false;
            }
        }

        // Ngắt kết nối PLC
        public void Disconnect()
        {
            if (plc != null && plc.IsConnected)
            {
                plc.Close();
                Connected = false;
            }
        }

        // Đọc dữ liệu từ PLC
        public string ReadData(string address)
        {
            try
            {
                if (Connected)
                {
                    var result = plc.Read(address).ToString();
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    throw new InvalidOperationException("PLC is not connected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading PLC data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        // Ghi dữ liệu vào PLC
        public void WriteData(string address, object value)
        {
            try
            {
                if (Connected)
                {
                    plc.Write(address, value);
                }
                else
                {
                    throw new InvalidOperationException("PLC is not connected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Writing PLC data Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

