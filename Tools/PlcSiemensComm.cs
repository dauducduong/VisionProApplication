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
        public bool IsConnected { get; private set; }

        // Kết nối PLC
        public void Connect(string ipAddress, short rack, short slot)
        {
            /* 
             Nếu truyền thêm cpuType
            string formattedCpuType = cputype.Replace("-", "");
            // Chuyển string thành enum
            if (!Enum.TryParse(formattedCpuType, out CpuType cpuModel))
            {
                MessageBox.Show("Invalid CPU type!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            */
            try
            {
                plc = new Plc(CpuType.S71200, ipAddress, rack, slot);
                plc.Open();
                IsConnected = plc.IsConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PLC Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsConnected = false;
            }
        }

        // Ngắt kết nối PLC
        public void Disconnect()
        {
            if (plc != null && plc.IsConnected)
            {
                plc.Close();
                IsConnected = false;
            }
        }

        // Đọc dữ liệu từ PLC
        public string ReadData(string address)
        {
            try
            {
                if (IsConnected)
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
                if (IsConnected)
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

