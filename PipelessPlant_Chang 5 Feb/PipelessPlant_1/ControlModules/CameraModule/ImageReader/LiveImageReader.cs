using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading.Tasks;

using MULTIFORM_PCS.ControlModules.CameraModule.ImageReader.uEyeCam;
using MULTIFORM_PCS.ControlModules.CameraModule.Algorithm;

namespace MULTIFORM_PCS.ControlModules.CameraModule.ImageReader
{
    public class LiveImageReader
    {
        //private int gamma = 160;

        private PatternDetectionAlgorithm dA;
        private System.Windows.Forms.PictureBox DisplayWindow;
        private List<Point> perceptrons;

        private uEye m_uEye;
        private bool m_bLive;
        private bool m_bDrawing;
        private int m_RenderMode;

        private int imageWidth;
        private int imageHeight;

        // uEye images
        //private const int IMAGE_COUNT = 4;
        private const int IMAGE_COUNT = 2;
        private struct UEYEIMAGE
        {
            public IntPtr pMemory;
            public int MemID;
            public int nSeqNum;
        }
        private UEYEIMAGE[] m_UeyeImages;
        private IntPtr m_pCurMem;
        private bool res800x600;

        public LiveImageReader(PatternDetectionAlgorithm dA, bool res800x600)
        {
            this.res800x600 = res800x600;
            this.dA = dA;
            this.DisplayWindow = new System.Windows.Forms.PictureBox();

            // init variables
            //m_bLive = false;

            m_bDrawing = false;
            //m_RenderMode = uEye.IS_RENDER_FIT_TO_WINDOW;
            m_RenderMode = uEye.IS_RENDER_NORMAL;

            // init our ueye object
            m_uEye = new uEye();
            // enable static messages ( no open camera is needed )	
            IntPtr test = new IntPtr();
            m_uEye.EnableMessage(uEye.IS_NEW_DEVICE, test.ToInt32());
            m_uEye.EnableMessage(uEye.IS_DEVICE_REMOVAL, test.ToInt32());

            // init our image struct and alloc marshall pointers for the uEye memory
            m_UeyeImages = new UEYEIMAGE[IMAGE_COUNT];
            int nLoop = 0;
            for (nLoop = 0; nLoop < IMAGE_COUNT; nLoop++)
            {
                m_UeyeImages[nLoop].pMemory = Marshal.AllocCoTaskMem(4);	// create marshal object pointers
                m_UeyeImages[nLoop].MemID = 0;
                m_UeyeImages[nLoop].nSeqNum = 0;
            }
        }

        public void setGamma(int gamma)
        {
            m_uEye.SetGamma(gamma);
        }
        public void setContrast(int contrast)
        {
            m_uEye.SetContrast(contrast);
        }
        public void setBrightness(int brightness)
        {
            double newValue = 0;
            m_uEye.SetExposureTime((double)(brightness / 1000), ref newValue); //SetBrightness(brightness);
            GUI.PCSMainWindow.getInstance().postStatusMessage("New Exposure Time: " + newValue);
        }



        // -----------------  DrawImage  -------------------------
        //
        public void readCurrentImage()
        {
            if (!m_bDrawing)
            {
                m_bDrawing = true;
                // draw current memory if a camera is opened
                if (m_uEye.IsOpen())
                {
                    int num = 0;
                    IntPtr pMem = new IntPtr();
                    IntPtr pLast = new IntPtr();
                    m_uEye.GetActSeqBuf(ref num, ref pMem, ref pLast);
                    if (pLast.ToInt32() == 0)
                    {
                        m_bDrawing = false;
                        return;
                    }

                    int nLastID = GetImageID(pLast);
                    int nLastNum = GetImageNum(pLast);
                    m_uEye.LockSeqBuf(nLastNum, pLast);

                    m_pCurMem = pLast;		// remember current buffer for our tootip ctrl

                    m_uEye.RenderBitmap(nLastID, DisplayWindow.Handle.ToInt32(), m_RenderMode);

                    m_uEye.UnlockSeqBuf(nLastNum, pLast);
                }
                m_bDrawing = false;
            }
            else
            {
            }
        }


        public int[] getCurrentImage()
        {
            readCurrentImage();
            // if there is a open camera, show tooltip ctrl
            if (m_uEye.IsOpen())
            {
                perceptrons = new List<Point>();
                int width = 0, height = 0, bitspp = 0, pitch = 0, bytespp = 0;
                m_uEye.InquireImageMem(m_pCurMem, GetImageID(m_pCurMem), ref width, ref height, ref bitspp, ref pitch);
                bytespp = (bitspp + 1) / 8;
                int[] image = new int[width * height];
                imageHeight = height;
                imageWidth = width;

               /** if (res800x600)
                {
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX >= 800)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX = 799;
                  }
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX >= 800)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX = 799;
                  }
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY >= 600)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY = 599;
                  }
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY >= 600)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY = 599;
                  }
                }*/
                Parallel.For(0, width, delegate(int i)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int pos = pitch * j + bytespp * i;
                        if (pos < pitch * height && i < width)
                        {
                            int b = Marshal.ReadByte(m_pCurMem, pos);
                            int g = Marshal.ReadByte(m_pCurMem, pos + 1);
                            int r = Marshal.ReadByte(m_pCurMem, pos + 2);
                            System.Drawing.Color pixelColor = System.Drawing.Color.FromArgb(r, g, b);
                            image[j * width + i] = pixelColor.ToArgb();
                            if (i >= Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX 
                                && i <= Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX
                                && j >= Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY 
                                && j < Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY)
                            {
                                if (pixelColor.R >= dA.redChanMin && pixelColor.R <= dA.redChanMax &&
                                    pixelColor.B >= dA.blueChanMin  && pixelColor.B <= dA.blueChanMax &&
                                    pixelColor.G >= dA.greenChanMin && pixelColor.G <= dA.greenChanMax)
                              {

                                lock (perceptrons)
                                {
                                  perceptrons.Add(new Point((j * width + i) / width, (j * width + i) % width));
                                }
                              }
                            }
                        }
                    }
                });
                return image;
            }
            return null;
        }

        public List<Point> getPerceptrons()
        {
            return perceptrons;
        }

        // ------------------------  GetImageID -------------------------------
        //
        private int GetImageID(IntPtr pBuffer)
        {
            // get image id for a given memory
            if (!m_uEye.IsOpen())
                return 0;

            int i = 0;
            for (i = 0; i < IMAGE_COUNT; i++)
                if (m_UeyeImages[i].pMemory == pBuffer)
                    return m_UeyeImages[i].MemID;
            return 0;
        }


        // ------------------------  GetImageNum -------------------------------
        //
        private int GetImageNum(IntPtr pBuffer)
        {
            // get number of sequence for a given memory
            if (!m_uEye.IsOpen())
                return 0;

            int i = 0;
            for (i = 0; i < IMAGE_COUNT; i++)
                if (m_UeyeImages[i].pMemory == pBuffer)
                    return m_UeyeImages[i].nSeqNum;

            return 0;
        }

        // ------------------------  btnInit_Click -------------------------------
		//
		public void startCam()
		{
			// if opened before, close now
			if (m_uEye.IsOpen())
			{
				m_uEye.ExitCamera ();
			}

			// open a camera
            int nRet = m_uEye.InitCamera(0, DisplayWindow.Handle.ToInt32());
			if (nRet == uEye.IS_STARTER_FW_UPLOAD_NEEDED)
			{
				/************************************************************************************************/
				/*                                                                                              */
				/*  If the camera returns with "IS_STARTER_FW_UPLOAD_NEEDED", an upload of a new firmware       */
				/*  is necessary. This upload can take several seconds. We recommend to check the required      */
				/*  time with the function is_GetDuration().                                                    */
				/*                                                                                              */
				/*  In this case, the camera can only be opened if the flag "IS_ALLOW_STARTER_FW_UPLOAD"        */ 
				/*  is "OR"-ed to m_hCam. This flag allows an automatic upload of the firmware.                 */
				/*                                                                                              */                        
				/************************************************************************************************/
				
				uint nUploadTime = 25000;
				m_uEye.GetDuration (uEye.IS_STARTER_FW_UPLOAD, ref nUploadTime);

				String Str;
				Str = "This camera requires a new firmware. The upload will take about " + nUploadTime / 1000 + " seconds. Please wait ...";
				Console.WriteLine(Str, "uEye");

                nRet = m_uEye.InitCamera(0 | uEye.IS_ALLOW_STARTER_FW_UPLOAD, DisplayWindow.Handle.ToInt32());
			}

			if (nRet != uEye.IS_SUCCESS)
			{
                Console.WriteLine("Init failed", "uEye Simple HDR Demo");
				return;
			}

            uEye.SENSORINFO sensorInfo = new uEye.SENSORINFO();
            m_uEye.GetSensorInfo(ref sensorInfo);

            if(res800x600)
            {
                int mode1 = uEye.IS_BINNING_2X_HORIZONTAL;
                int mode2 = uEye.IS_BINNING_2X_VERTICAL;
                m_uEye.SetBinning(mode1 ^ mode2);
            }

            // Set the image size
            int x = 0;
            int y = 0;
            unsafe
            {
                int nAOISupported = -1;
                IntPtr pnAOISupported = (IntPtr)((uint*)&nAOISupported);
                bool bAOISupported = true;

                // check if an arbitrary AOI is supported
                if (m_uEye.ImageFormat(uEye.IMGFRMT_CMD_GET_ARBITRARY_AOI_SUPPORTED, pnAOISupported, 4) == uEye.IS_SUCCESS)
                {
                    bAOISupported = (nAOISupported != 0);
                }

                // If an arbitrary AOI is supported -> take maximum sensor size
                if (bAOISupported)
                {
                    if (res800x600)
                    {
                        x = 800;
                        y = 600;
                    }
                    else
                    {
                        x = sensorInfo.nMaxWidth;
                        y = sensorInfo.nMaxHeight;
                    }
                }
                // Take the image size of the current image format
                else
                {
                    x = m_uEye.SetImageSize(uEye.IS_GET_IMAGE_SIZE_X, 0);
                    y = m_uEye.SetImageSize(uEye.IS_GET_IMAGE_SIZE_Y, 0);
                }

                m_uEye.SetImageSize(x, y);
            }


            // alloc images
            m_uEye.ClearSequence();
            int nLoop = 0;
            for (nLoop = 0; nLoop < IMAGE_COUNT; nLoop++)
            {
              // alloc memory
              m_uEye.AllocImageMem(x, y, 32, ref m_UeyeImages[nLoop].pMemory, ref m_UeyeImages[nLoop].MemID);
              // add our memory to the sequence
              m_uEye.AddToSequence(m_UeyeImages[nLoop].pMemory, m_UeyeImages[nLoop].MemID);
              // set sequence number
              m_UeyeImages[nLoop].nSeqNum = nLoop + 1;
            }

            m_uEye.SetColorMode(uEye.IS_SET_CM_RGB32);
            IntPtr test = new IntPtr();
            m_uEye.EnableMessage(uEye.IS_FRAME, test.ToInt32());

            // capture a single image
            //m_uEye.FreezeVideo(uEye.IS_WAIT);

            if (m_uEye.CaptureVideo(uEye.IS_WAIT) == uEye.IS_SUCCESS)
              m_bLive = true;
            else
              Console.WriteLine("Capture Video failed!");


 
            //m_uEye.AOI(m_RenderMode, );


            //double enable = 1;
            //m_uEye.SetAutoParameter(
            //    uEye.IS_SET_ENABLE_AUTO_SHUTTER, enable, 0);
            //double brightness = 50;
            //m_uEye.SetAutoParameter(
            //    uEye.IS_SET_AUTO_REFERENCE, brightness, 0);



            //Console.WriteLine("!" + nPosX + nPosY + nWidth+ nHeight );
    }


        public unsafe int GetAoiSize(ref int nPosX, ref int nPosY, ref int nWidth, ref int nHeight)
        {
            uEye.IS_RECT rectAOI;
            uEye.IS_RECT* pRect = &rectAOI;
            IntPtr pTemp = (IntPtr)pRect;
            int nRet = m_uEye.AOI(uEye.IS_AOI_IMAGE_GET_AOI, pTemp, (uint)Marshal.SizeOf(rectAOI));
            if (nRet == uEye.IS_SUCCESS)
            {
                // save the return values
                nPosX = rectAOI.s32X;
                nPosY = rectAOI.s32Y;
                nWidth = rectAOI.s32Width;
                nHeight = rectAOI.s32Height;
            }

            return nRet;
        }

        public unsafe int SetAoiSize(ref int nPosX, ref int nPosY, ref int nWidth, ref int nHeight)
        {
            uEye.IS_RECT rectAOI;
            uEye.IS_RECT* pRect = &rectAOI;
            IntPtr pTemp = (IntPtr)pRect;

            // set the aoi size
            rectAOI.s32X = nPosX;
            rectAOI.s32Y = nPosY;
            rectAOI.s32Width = nWidth;
            rectAOI.s32Height = nHeight;

            int nRet = m_uEye.AOI(uEye.IS_AOI_IMAGE_SET_AOI, pTemp, (uint)Marshal.SizeOf(rectAOI));

            return nRet;
        }

        public void stopCam()
        {
            // release marshal object pointers
            int nLoop = 0;
            for (nLoop = 0; nLoop < IMAGE_COUNT; nLoop++)
                Marshal.FreeCoTaskMem(m_UeyeImages[nLoop].pMemory);

            m_uEye.ExitCamera();
        }

        public int getHeight()
        {
            return imageHeight;
        }

        public int getWidth()
        {
            return imageWidth;
        }
    }
}
