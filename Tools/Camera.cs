using Cognex.VisionPro;
using Cognex.VisionPro.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace VisionProApplication
{
    public class Camera
    {

        //Public mFrameGrabber.Count
        public int camCount = 0;

        //public delegate void FormCamera();
        //public event FormCamera camCount;
        public struct CAMERA
        {
            public ICogAcqFifo mAcqFifo { get; set; }
            public bool isConnect { get; set; }

        }
        public Dictionary<string, bool> ISLIVE = new Dictionary<string, bool>();

        public Dictionary<string, int> numAcqs = new Dictionary<string, int>();
        //public ICogFrameGrabber mFrameGrabber = null;

        //public Dictionary<string, int> numAcqs = new Dictionary<string, int>();

        public class VinsionImageAvailableEventArgs
        {
            public string Index;
            public ICogImage Image;
            public bool ISLIVE;

            public VinsionImageAvailableEventArgs(string index, ICogImage image, bool isLive)
            {
                Index = index;
                Image = image;
                ISLIVE = isLive;
            }
        }

        public delegate void VisionImageAvailableEventHandler(object sender, VinsionImageAvailableEventArgs e);
        public event VisionImageAvailableEventHandler VisionImageAvailable = null;
        public CAMERA[] mCamera = new CAMERA[1];
        public Camera()
        {
            DeviceListAcq();
            //SetupOutput();
        }

        private void DeviceListAcq()
        {
            numAcqs.Clear();
            try
            {
                // Step 1 - Create the CogFrameGrabbers
                CogFrameGrabbers mFrameGrabbers = new CogFrameGrabbers();
                camCount = mFrameGrabbers.Count;
                if (mFrameGrabbers.Count < 1)
                    throw new CogAcqNoFrameGrabberException("No frame grabbers found");

                // Step 2 - Select the first frame grabber even if there is more than one.
                for (int i = 0; i < mFrameGrabbers.Count; i++)
                {
                    var mFrameGrabber = mFrameGrabbers[i];
                    var cameraName = mFrameGrabber.SerialNumber;
                    // Display the board type
                    //if (mFrameGrabber.SerialNumber.Contains(Properties.Settings.Default.SNTopCam))
                    //{
                    //     cameraName = "TopCam - " + mFrameGrabber.SerialNumber;
                    //}
                    //else if (mFrameGrabber.SerialNumber.Contains(Properties.Settings.Default.SNBotCam))
                    //{
                    //     cameraName = "BotCam - " + mFrameGrabber.SerialNumber;
                    //}
                    var cameraFormat = mFrameGrabber.AvailableVideoFormats[0];
                    mCamera[i].mAcqFifo = mFrameGrabber.CreateAcqFifo(cameraFormat,
                    CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);

                    //mCamera[i].mAcqFifo.OwnedExposureParams.Exposure =  5;
                    mCamera[i].isConnect = true;
                    numAcqs.Add(cameraName, 0);
                    ISLIVE.Add(cameraName, false);
                    mCamera[i].mAcqFifo.Complete += Camera_Complete;

                }

            }
            catch (CogAcqException)
            {
                mCamera = null;
            }

        }

        private void Camera_Complete(object sender, CogCompleteEventArgs e)
        {
            int numReadyVal, numPendingVal;
            bool busyVal;


            var AcqFifo = (ICogAcqFifo)sender;

            CogAcqInfo info = new CogAcqInfo();
            try
            {
                AcqFifo.GetFifoState(out numPendingVal, out numReadyVal, out busyVal);
                if (numReadyVal > 0)
                {
                    numAcqs[AcqFifo.FrameGrabber.SerialNumber] += 1;
                    var Image = AcqFifo.CompleteAcquireEx(info);
                    if (VisionImageAvailable != null)
                        VisionImageAvailable(this, new VinsionImageAvailableEventArgs(AcqFifo.FrameGrabber.SerialNumber, Image, ISLIVE[AcqFifo.FrameGrabber.SerialNumber]));
                }



                if (numAcqs[AcqFifo.FrameGrabber.SerialNumber] >= 1)
                {
                    GC.Collect();
                    numAcqs[AcqFifo.FrameGrabber.SerialNumber] = 0;

                }

            }
            catch (CogException)
            {

            }


        }

        public void SetupRunContinuos(int Index)
        {
            try
            {
                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Manual;
                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerEnabled = true;
                ISLIVE[mCamera[Index].mAcqFifo.FrameGrabber.SerialNumber] = true;

            }
            catch { }

        }
        public void SetupRunOnce(int Index)
        {
            try
            {
                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Manual;
                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerEnabled = true;
                //ISLIVE[mCamera[Index].mAcqFifo.FrameGrabber.SerialNumber] = false;

            }
            catch { }

        }
        public void SetExposure(int Index, int exp)
        {
            try
            {
                mCamera[Index].mAcqFifo.OwnedExposureParams.Exposure = exp;
            }
            catch { }

        }

        public void SetupRunExternalTrigger(int Index)
        {
            try
            {
                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Auto;
                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerLowToHigh = true;


                mCamera[Index].mAcqFifo.OwnedTriggerParams.TriggerEnabled = true;


            }
            catch { }

        }
        public void RunOnce(int Index)
        {
            if(mCamera !=null)
            {
                try
                {
                    mCamera[Index].mAcqFifo.StartAcquire();
                }
                catch (Exception)
                {
                    MessageBox.Show("Camera disconnection");
                }
            }
            else
            {
                MessageBox.Show("Camera disconnection");
            }

            

            //StopTriggerImage(Index);
        }


        public void DestroyCamera(int index)
        {
            mCamera[index].mAcqFifo.FrameGrabber.Disconnect(false);

        }

        public void SetupOutput()
        {
            for (int index = 0; index < mCamera.Length; index++)
            {
                if (mCamera[index].isConnect == true)
                {
                    mCamera[index].mAcqFifo.FrameGrabber.OwnedGigEAccess.SetFeature("LineSelector", "Out1");
                    mCamera[index].mAcqFifo.FrameGrabber.OwnedGigEAccess.SetFeature("LineSource", "UserOutput");
                    mCamera[index].mAcqFifo.FrameGrabber.OwnedGigEAccess.SetFeature("UserOutputSelector", "UserOutput1");
                }

            }

        }

        public void SetOutputHigh(int index)
        {
            mCamera[index].mAcqFifo.FrameGrabber.OwnedGigEAccess.SetFeature("UserOutputValue", "1");
        }

        public void SetOutputLow(int index)
        {
            mCamera[index].mAcqFifo.FrameGrabber.OwnedGigEAccess.SetFeature("UserOutputValue", "0");
        }

    }
}
