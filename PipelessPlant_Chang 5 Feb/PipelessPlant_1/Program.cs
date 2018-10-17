
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.IO;
using System.Threading;
using MULTIFORM_PCS.ControlModules.CameraModule.Algorithm;
using System.ComponentModel;
using MULTIFORM_PCS.Gateway;




namespace MULTIFORM_PCS
    
{
 
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]

        
        static void Main()
        {
            App PCS = new App();
            new MULTIFORM_PCS.ControlModules.CameraModule.NetworkFeeder();
            PCS.Run();


            /**Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PipelessPlantMainWindowForm());*/
        }
    }
}