using System;
using System.Windows.Forms;
using System.Resources;

using Cognex.VisionPro;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.QuickBuild;
using System.Reflection;
using System.Drawing;
using Cognex.VisionPro.ToolBlock;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace VisionProApplication
{
    public class Utility
    {
        static public ICogRecord TraverseSubRecords(ICogRecord r, string[] subs)
        {
            // Utility function to walk down to a specific subrecord
            if (r == null)
                return r;

            foreach (string s in subs)
            {
                if (r.SubRecords.ContainsKey(s))
                    r = r.SubRecords[s];
                else
                    return null;
            }

            return r;
        }
        private delegate void SetControlPropertyThreadSafeDelegate(
            System.Windows.Forms.Control control,
            string propertyName,
            object propertyValue);

        public void SetControlPropertyThreadSafe(
            System.Windows.Forms.Control control,
            string propertyName,
            object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate
                (SetControlPropertyThreadSafe),
                new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { propertyValue });
            }
        }

        public void SetControlPropertyRecord()
        {
        }
        public void FlushAllQueues(CogJobManager jm)
        {
            // Flush all queues
            if (jm == null)
                return;

            jm.UserQueueFlush();
            jm.FailureQueueFlush();
            for (int i = 0; i < jm.JobCount; i++)
            {
                jm.Job(i).OwnedIndependent.RealTimeQueueFlush();
                jm.Job(i).ImageQueueFlush();
            }
        }

        static public int GetJobIndexFromName(CogJobManager mgr, string name)
        {
            if (mgr != null)
            {
                for (int i = 0; i < mgr.JobCount; ++i)
                    if (mgr.Job(i).Name == name)
                        return i;
            }
            return -1;
        }

        public Type GetPropertyType(object obj, string path)
        {
            if (obj == null || path == "")
                return null;

            System.Reflection.MemberInfo[] infos = CogToolTerminals.
                ConvertPathToMemberInfos(obj, obj.GetType(), path);

            if (infos.Length == 0)
                return null;

            // Return the type of the last path element.
            return CogToolTerminals.GetReturnType(infos[infos.Length - 1]);
        }
        public void SetupPropertyProvider(CogToolPropertyProvider p, Control gui, string path)
        {
            p.SetPath(gui, path);
        }

        public bool AddRecordToDisplay(CogRecordsDisplay disp, ICogRecord r, string[] subs,
            bool pickBestImage)
        {
            // Utility function to put a specific subrecord into a display
            ICogRecord addrec = Utility.TraverseSubRecords(r, subs);
            if (addrec != null)
            {
                // if this is the first record in, then always select an image
                if (disp.Subject == null)
                    pickBestImage = true;

                disp.Subject = addrec;

                if (pickBestImage)
                {
                    // select first non-empty image record, to workaround the fact that the input image tool
                    // adds an empty subrecord to the LastRun record when it is disabled (when an image file
                    // tool is used, for example)
                    for (int i = 0; i < addrec.SubRecords.Count; ++i)
                    {
                        ICogImage img = addrec.SubRecords[i].Content as ICogImage;
                        if (img != null && img.Height != 0 && img.Width != 0)
                        {
                            disp.SelectedRecordKey = addrec.RecordKey + "." + addrec.SubRecords[i].RecordKey;
                            break;
                        }
                    }
                }

                return true;
            }
            return false;
        }
        public void SaveJobFile(string VppFileName, CogToolBlock job)
        {

            // save the QuickBuild project file
            try
            {
                CogSerializer.SaveObjectToFile(job, VppFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save job file: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SaveJobFileMinimum(string VppFileName, CogToolBlock job)
        {
            try
            {
                CogSerializer.SaveObjectToFile(job, VppFileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Cannot save job file!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //public static string GetThisExecutableDirectory()
        //{
        //  string loc = Application.ExecutablePath;
        //  loc = System.IO.Path.GetDirectoryName(loc) + "\\";
        //  return loc;
        //}
        //    public Bitmap getBitmapImage(IImage inputImage, IImage overlayImage)
        //    {
        //        Bitmap mergedBitmap = null;
        //        try
        //        {
        //            var bitmapLocated = inputImage.Bitmap;
        //            var bitmapLocatedOverlay = overlayImage.Bitmap;

        //            mergedBitmap = MergeBitmap(bitmapLocated, bitmapLocatedOverlay);
        //        }
        //        catch { }




        //        return mergedBitmap;
        //    }
        //    public Bitmap MergeBitmap(Bitmap inputBitmap, Bitmap mergeBitmap)
        //    {
        //        // The original bitmap with the wrong pixel format. 
        //        // You can check the pixel format with originalBmp.PixelFormat


        //        // Create a blank bitmap with the same dimensions
        //        Bitmap tempBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height);

        //        // From this bitmap, the graphics can be obtained, because it has the right PixelFormat
        //        using (Graphics g = Graphics.FromImage(tempBitmap))
        //        {
        //            // Draw the original bitmap onto the graphics of the new bitmap
        //            g.DrawImage(inputBitmap, 0, 0);
        //            // Use g to do whatever you like
        //            try
        //            {
        //                g.DrawImage(mergeBitmap, 0, 0);
        //            }
        //            catch { }


        //        }
        //        return tempBitmap;
        //    }

        //public Bitmap DrawRectangle(Bitmap inputBitmap, List<Dictionary<string, int>> Coor)
        //{
        //    // The original bitmap with the wrong pixel format. 
        //    // You can check the pixel format with originalBmp.PixelFormat
        //    // Create a blank bitmap with the same dimensions
        //    Bitmap tempBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height);

        //    // From this bitmap, the graphics can be obtained, because it has the right PixelFormat
        //    using (Graphics g = Graphics.FromImage(tempBitmap))
        //    {
        //        // Draw the original bitmap onto the graphics of the new bitmap
        //        g.DrawImage(inputBitmap, 0, 0);
        //        // Use g to do whatever you like
        //        try
        //        {
        //            for (int i = 0; i < Coor.Count; i++)
        //            {
        //                Color customColor;
        //                if (Coor[i]["passFail"] == 0)
        //                    customColor = Color.Red;
        //                else
        //                    customColor = Color.Green;
        //                Pen drawPen = new Pen(customColor, 10);
        //                g.DrawRectangle(drawPen, Coor[i]["PosX"], Coor[i]["PosY"], Coor[i]["Width"], Coor[i]["Height"]);
        //            }
        //        }
        //        catch { }
        //    }
        //    return tempBitmap;
        //}


        //#region Microsoft Excel

        //FileStream fsApp;
        //IWorkbook workbook;
        //ISheet sheet;

        //CheckPoint[] cp1Arr;
        //CheckPoint[] cp2Arr;

        //class ExcelInfors
        //{
        //    public string Path { get; set; }
        //    public string ModelName { get; set; }
        //    public string Barcode { get; set; }
        //}

        //ExcelInfors excelInfor = new ExcelInfors();

        //public void LogExcel(string excelPath, string modelName, string barcode, CheckPoint[] cp1Array, CheckPoint[] cp2Array)
        //{
        //    excelInfor.Path = excelPath;
        //    excelInfor.ModelName = modelName;
        //    excelInfor.Barcode = barcode;
        //    cp1Arr = cp1Array;
        //    cp2Arr = cp2Array;

        //    if (!File.Exists(excelInfor.Path))
        //    {
        //        NewExcel();
        //        EditExcel();
        //    }
        //    else EditExcel();
        //}

        //public void NewExcel()
        //{
        //    try
        //    {
        //        workbook = new XSSFWorkbook();
        //        sheet = workbook.CreateSheet("Sheet1");
        //        IRow row0 = sheet.CreateRow(0);

        //        row0.CreateCell(0).SetCellValue("DATE TIME");
        //        row0.CreateCell(1).SetCellValue("MODEL");
        //        row0.CreateCell(2).SetCellValue("BARCODE");
        //        row0.CreateCell(3).SetCellValue("CAMERA");
        //        row0.CreateCell(4).SetCellValue("CHECK POINT");
        //        row0.CreateCell(5).SetCellValue("TOOL");
        //        row0.CreateCell(6).SetCellValue("DEEP LEARNING");
        //        row0.CreateCell(7).SetCellValue("VISION PRO");
        //        row0.CreateCell(8).SetCellValue("TOTAL RESULT");

        //        FileStream create = File.Create(excelInfor.Path);
        //        create.Flush();
        //        workbook.Write(create, false);
        //        create.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}
        //int rowId = 0;
        //bool isWriteTotal = true; // avoid duplicated total result in the same check point
        //private void EditExcel()
        //{
        //    try
        //    {
        //        fsApp = new FileStream(excelInfor.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //        workbook = new XSSFWorkbook(fsApp);
        //        sheet = workbook.GetSheetAt(0);

        //        //if (sheet.GetRow(1).GetCell(1).StringCellValue.ToLower() != "")
        //        //    sheet.ShiftRows(1, sheet.LastRowNum, 1);

        //        int from = 0, to = 99;

        //        foreach (CheckPoint cp in cp1Arr)
        //        {
        //            if (cp != null)
        //            {
        //                if (cp.Side == "F")
        //                {
        //                    from = 1;
        //                    to = 49;
        //                }
        //                else if (cp.Side == "R")
        //                {
        //                    from = 50;
        //                    to = 99;
        //                }
        //            }
        //        }

        //        rowId = sheet.LastRowNum + 1;

        //        for (int i = from; i <= to; i++)
        //        {
        //            CheckPoint cp = cp1Arr[i];
        //            if (cp != null)
        //            {
        //                foreach(KeyValuePair<string, CheckPoint.Results> kvp in cp.ToolResulstVPDL)
        //                {
        //                    FillRow(cp, kvp.Key, kvp.Value.ToString());
        //                }
        //                isWriteTotal = true;
        //            }
        //            else FillEmptyRow("C1");
        //        }

        //        for (int i = from; i <= to; i++)
        //        {
        //            CheckPoint cp = cp2Arr[i];
        //            if (cp != null)
        //            {
        //                foreach (KeyValuePair<string, CheckPoint.Results> kvp in cp.ToolResulstVPDL)
        //                {
        //                    FillRow(cp, kvp.Key, kvp.Value.ToString());
        //                }
        //                isWriteTotal = true;
        //            }
        //            else FillEmptyRow("C2");
        //        }

        //        for(int i = 0; i <= 8; i++)
        //        {
        //            sheet.AutoSizeColumn(i);
        //        }

        //        FileStream fout = new FileStream(excelInfor.Path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
        //        fout.Flush();
        //        workbook.Write(fout, false);
        //        fout.Close();
        //    }
        //    catch(Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        //public void FillRow(CheckPoint cp, string toolName, string toolResult)
        //{
        //    try
        //    {
        //        rowId++;
        //        IRow row = sheet.CreateRow(rowId);

        //        if (rowId < 2)
        //        {
        //            row.CreateCell(0).SetCellValue(DateTime.Now.ToString("dd/MM/yyyy HH_mm_ss"));
        //            row.CreateCell(1).SetCellValue(excelInfor.ModelName);
        //            row.CreateCell(2).SetCellValue(excelInfor.Barcode);
        //        }
        //        else
        //        {
        //            row.CreateCell(0);
        //            row.CreateCell(1);
        //            row.CreateCell(2);
        //        }

        //        row.CreateCell(3).SetCellValue(cp.CamID);
        //        row.CreateCell(4).SetCellValue(cp.Name);
        //        row.CreateCell(5).SetCellValue(toolName);
        //        row.CreateCell(6).SetCellValue(toolResult);

        //        if (isWriteTotal)
        //        {
        //            row.CreateCell(7).SetCellValue(cp.ResultVPro.ToString());

        //            string totalResult = IsAllResultsReady(cp);

        //            if (totalResult == "OK")
        //            {
        //                row.CreateCell(8).SetCellValue(totalResult);
        //                row.CreateCell(9).SetCellValue("");
        //            }
        //            else if (totalResult == "NG")
        //            {
        //                row.CreateCell(8).SetCellValue("");
        //                row.CreateCell(9).SetCellValue(totalResult);
        //            }

        //            isWriteTotal = false;
        //        }
        //        else
        //        {
        //            row.CreateCell(7);
        //            row.CreateCell(8);
        //            row.CreateCell(9);
        //        }  
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        //public void FillEmptyRow(string camId)
        //{          
        //    try
        //    {
        //        rowId++;
        //        IRow row = sheet.CreateRow(rowId);

        //        if (rowId < 2)
        //        {
        //            row.CreateCell(0).SetCellValue(DateTime.Now.ToString("dd/MM/yyyy HH_mm_ss"));
        //            row.CreateCell(1).SetCellValue(excelInfor.ModelName);
        //            row.CreateCell(2).SetCellValue(excelInfor.Barcode);
        //        }

        //        row.CreateCell(3).SetCellValue(camId);
        //        row.CreateCell(4).SetCellValue("NULL");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        ////public string ChildPathDefine(string root, string modelName, bool excel)
        ////{
        ////    string path = root;

        ////    path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM"));
        ////    if (!Directory.Exists(path))
        ////        Directory.CreateDirectory(path);
        ////    path = Path.Combine(path, DateTime.Now.ToString("dd"));
        ////    if (!Directory.Exists(path))
        ////        Directory.CreateDirectory(path);
        ////    path = Path.Combine(path, modelName);
        ////    if (!Directory.Exists(path))
        ////        Directory.CreateDirectory(path);

        ////    if (excel)
        ////        path = Path.Combine(path, modelName + "_" + DateTime.Now.ToString("yyyy_MM_dd") + ".xlsx");
        ////    else
        ////    {
        ////        path = Path.Combine(path, "images");
        ////        if (!Directory.Exists(path))
        ////            Directory.CreateDirectory(path);
        ////    }

        ////    return path;
        ////}

        //#endregion


        //public string ImageNameDefine(string model, string side, string productID,
        //    string camID, string qc, string checkpointName, string barcode)
        //{
        //    string time = DateTime.Now.ToString("yy MM dd_HH mm ss");
        //    string timestamp = time.Replace(" ", "");
        //    string modelInfo = model + "_" + side + productID.PadLeft(3, '0');

        //    string camNo = camID;
        //    string cpName = checkpointName;// "P" + checkpointID.PadLeft(2, '0');
        //    string checkpointInfo = camNo + "_" + qc + "_" + cpName;

        //    return modelInfo + " " + checkpointInfo + " " + timestamp + " " + barcode + ".bmp";
        //}

        //public string IsAllResultsReady(CheckPoint cp)
        //{
        //    string totalResult;

        //    if ((cp.ResultVPro == CheckPoint.Results.OK || cp.ResultVPro == CheckPoint.Results.ROK)
        //            && (cp.ResultVPDL == CheckPoint.Results.OK || cp.ResultVPDL == CheckPoint.Results.ROK))
        //        totalResult = "OK";
        //    else if (cp.ResultVPro == CheckPoint.Results.NG || cp.ResultVPro == CheckPoint.Results.RNG
        //        || cp.ResultVPDL == CheckPoint.Results.NG || cp.ResultVPDL == CheckPoint.Results.RNG)
        //        totalResult = "NG";
        //    else totalResult = "NA";

        //    return totalResult;
        //}
    }


}
