using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Communication.Plc.Mitsubishi;
using Communication.Plc.Siemens;

namespace VisionProApplication
{
    public class PlcMitsubishiComm
    {
        private Enet_iQRCpu plc;
        public bool IsConnected { get; private set; }
        public string PlcName { get; private set; }
        public void Connect(string ipAddress, int port)
        {
            try
            {
                plc = new Enet_iQRCpu(PlcTypes.QCpu, PlcProtocol.Tcp);
                plc.Connect(ipAddress, port);
                IsConnected = plc.IsConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PLC Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsConnected = false;
            }

        }

        public void Disconnect()
        {
            if (plc != null && plc.IsConnected)
            {
                plc.Close();
                IsConnected = false;
            }

        }

        public void WriteData(MitsubishiMemory memory, ushort address, ushort value)
        {
            try
            {
                if (IsConnected)
                {
                    plc.MemoryAreaWrite(MitsubishiMemory.D, address, value);
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

        public uint ReadData(MitsubishiMemory memory, UInt16 address)
        {
            try
            {
                if (IsConnected)
                {
                    uint value;
                    plc.MemoryAreaRead(memory, address, out value);
                    return value;
                }
                else
                {
                    throw new InvalidOperationException("PLC is not connected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading PLC data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
    }
}

        //public void WriteStringData(MitsubishiMemory memory, UInt16 add, string value)
        //{
        //    enet_IQRCpu.MemoryAreaWrite(MitsubishiMemory.D, add, value);

        //    //enet_IQRCpu.MemoryAreaWrite("192.168.3.39", 8000, MitsubishiMemory.D, add, value);
        //}

        //public uint ReadStringData(MitsubishiMemory memory, ushort add)
        //{
        //    uint dataIn;
        //    enet_IQRCpu.MemoryAreaRead( MitsubishiMemory.D, add, out dataIn);
        //    return dataIn;
        //}

        //public void WriteMulData(MitsubishiMemory memory,ushort[] address,ushort[] val)
        //{
        //    enet_IQRCpu.MultipleMemoryAreaWrite(memory, address, val);
        //}
        

