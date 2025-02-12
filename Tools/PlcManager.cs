using S7.Net;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

namespace VisionProApplication
{



    public class PlcBrand
    {
        public string Name { get; set; }
        public List<string> Types { get; set; }
    }

    public class PlcList
    {
        public List<PlcBrand> PlcBrands { get; set; }

        public PlcList()
        {
            try
            {
                string solutionPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName)?.FullName)?.FullName;
                LoadDataFromJson(Path.Combine(solutionPath, "JSON Config\\PlcList.json"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void LoadDataFromJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                PlcBrands = JsonConvert.DeserializeObject<List<PlcBrand>>(json); // Deserialize thành List<PlcBrand>
            }
            else
            {
                PlcBrands = new List<PlcBrand>();
                Console.WriteLine("⚠ File JSON không tồn tại.");
            }
        }
    }
}


    
