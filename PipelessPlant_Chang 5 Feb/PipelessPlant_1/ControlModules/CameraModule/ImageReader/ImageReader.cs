using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MULTIFORM_PCS.ControlModules.CameraModule.ImageReader
{
    static class ImageReader
    {
        public static BitmapImage readJPEGFile(string fullPath)
        {
            BitmapImage rtnVal = new BitmapImage(new Uri("file://"+fullPath));
            return rtnVal;
        }
    }
}
