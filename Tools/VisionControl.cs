using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.QuickBuild;
using System.Drawing;
using System.Windows;

namespace VisionProApplication
{
    public class VisionControl
    {
        private bool mAttached = false;
        public List<CogToolBlock> _cogToolBlockManager = new List<CogToolBlock>();

        public enum RunState { Stopped, RunningContinuous, RunningOnce, RunningLive };
        public RunState mCurrentRunState = RunState.Stopped;

        #region UserResultAvailable event support
        public class VisionControlUserResultAvailableEventArgs
        {
            public string JobName;
            public CogToolBlockTerminalCollection Result;
            public ICogRunStatus JobStatus;
            public ICogRecord LastRunRecord;

            public VisionControlUserResultAvailableEventArgs(string jobName, CogToolBlockTerminalCollection result, ICogRunStatus jobStatus, ICogRecord lastRunRecord)
            {
                JobName = jobName;
                Result = result;
                JobStatus = jobStatus;
                LastRunRecord = lastRunRecord;
            }
        }
        public delegate void VisionControlUserResultAvailableEventHandler(object sender, VisionControlUserResultAvailableEventArgs e);
        public event VisionControlUserResultAvailableEventHandler VisionControlUserResultAvailable = null;
        #endregion

        public VisionControl(ref List<CogToolBlock> toolBlocks)
        {
            _cogToolBlockManager = toolBlocks;
        }
        public void AttachToJobManager(bool attach)
        {
            // attach and detach our event handlers/etc
            if (attach)
            {
                if (mAttached)
                    return;
                mAttached = true;
                for (int i = 0; i < _cogToolBlockManager.Count; i++)
                {
                    _cogToolBlockManager[i].Ran += _cogJobManager_Ran;
                }
            }
            else
            {
                if (!mAttached)
                    return;


                mAttached = false;
                for (int i = 0; i < _cogToolBlockManager.Count; i++)
                {
                    _cogToolBlockManager[i].Ran -= _cogJobManager_Ran;
                }

            }
        }
        private void _cogJobManager_Ran(object sender, EventArgs e)
        {
            var toolBlockName = (CogToolBlock)sender;
            var result = toolBlockName.Outputs;
            var resultStatus = toolBlockName.RunStatus;
            var Record = toolBlockName.CreateLastRunRecord();
            if (result != null)
            {
                if (VisionControlUserResultAvailable != null)
                    VisionControlUserResultAvailable(this, new VisionControlUserResultAvailableEventArgs(toolBlockName.Name, result, resultStatus, Record));
            }
            else
            {
                if (VisionControlUserResultAvailable != null)
                    VisionControlUserResultAvailable(this, new VisionControlUserResultAvailableEventArgs(toolBlockName.Name, null,null,null));
            }
        }
        public void StartRunningOnce(Bitmap inputBitmap, int index)
        {
            if (inputBitmap != null)
            {
                CogImage8Grey inputImage = new CogImage8Grey(inputBitmap);
                try
                {
                    _cogToolBlockManager[index].Inputs["OutputImage"].Value = inputImage;
                    _cogToolBlockManager[index].Run();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
