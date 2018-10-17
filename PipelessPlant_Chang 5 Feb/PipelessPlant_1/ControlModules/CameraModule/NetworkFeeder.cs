using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MULTIFORM_PCS.ControlModules.CameraModule
{
    public class NetworkFeeder
    {
        public struct RobotPos
        {
            public int id;
            public double x;
            public double y;
            public double angle;
            public RobotPos(int id, double x, double y, double angle)
            {
                this.id = id;
                this.x = x;
                this.y = y;
                this.angle = angle;
            }
        }
        public const int port = 3001;
        public const string address = "192.168.1.100";
        private TcpClient client = new TcpClient();
        public void sendUpdatePos(RobotPos[] pos)
        {
            try
            {
                var writer = new BinaryWriter(client.GetStream());
                writer.Write((byte)((pos.Length * 25) + 3));
                writer.Write((byte)0);
                writer.Write((byte)pos.Length);
                foreach (var p in pos)
                {
                    writer.Write((byte)p.id);
                    writer.Write(p.x);
                    writer.Write(p.y);
                    writer.Write(p.angle);
                }
                writer.Flush();
            }
            catch (Exception e)
            {
                singleton = null;
                System.Windows.MessageBox.Show("Could not send packet to camera");
            }
        }
        public static NetworkFeeder singleton = null;
        public NetworkFeeder()
        {
            try
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(address), port));
                singleton = this;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Could not connect to camera reader!!!");
            }
        }
    }
}
