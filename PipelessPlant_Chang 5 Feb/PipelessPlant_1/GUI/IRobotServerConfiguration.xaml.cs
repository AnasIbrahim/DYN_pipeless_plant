using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MULTIFORM_PCS.GUI
{
    /// <summary>
    /// Interaktionslogik für IRobotServerConfiguration.xaml
    /// </summary>
    public partial class IRobotServerConfiguration : Window
    {
        public IRobotServerConfiguration()
        {
            InitializeComponent();
        }

        private void Button_SaveAndStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().stopServer();
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().TcpIP = textBoxTCPIP.Text;
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().TcpPort = int.Parse(textBoxTCPPort.Text);
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().UdpIP = textBoxUDPIP.Text;
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().UpdPort = int.Parse(textBoxUDPPort.Text);
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().startServer();
                this.Close();

                PCSMainWindow.getInstance().border40.Background = Brushes.White;
                PCSMainWindow.getInstance().border40.BorderBrush = Brushes.White;
            }
            catch (Exception)
            {
                PCSMainWindow.getInstance().postStatusMessage("Wrong ip and port configuration. Restart of iRobotServer failed.");
            }
        }

        private void Button_Default_Click(object sender, RoutedEventArgs e)
        {
            textBoxTCPIP.Text = "192.168.1.200";
            textBoxTCPPort.Text = "2001";
            textBoxUDPIP.Text = "192.168.1.200";
            textBoxUDPPort.Text = "2000";
        }
    }
}
