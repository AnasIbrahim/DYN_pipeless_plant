
using System;
using System.Net;
using System.Net.Sockets;


// static values used in the protocol 

// type of the transmission
public enum Type
{
    fromNetToUart = 0,
    fromUartToTcp = 1,
    fromUartToUdp = 2,
}

// packet ids: from the pc to the uc
enum PTU {
	ptuAddSegments = 0,
	ptuDock,
	ptuUndock,
	ptuInterrupt,
	ptuDirectDrive,
	ptuDockDemo
};

// packet ids: from uc to pc
enum FromUcToPc : int
{
    assert = 0,
    debug,
    segmentDone,
    sensorUpdate,
    segmentUpdate,
}

// device able to handel one robot
class Device 
{
    // the port we are listening on
    public const int listenPort = 2001;
    // the listener itself
    private TcpListener listener = new TcpListener(System.Net.IPAddress.Any,listenPort);
    private TcpClient client;
    public Device()
    {
        // start listening
        listener.Start();
        // wait for an incoming connection and accepts it
        client = listener.AcceptTcpClient();
        // starts a backround thread for asynchrone recveing
        // will call recv on new data (from onother thread)
        client.GetStream().BeginRead(recvBuffer, recved, recvBuffer.Length - recved, recv, null);
    }
    // send direct drive command
    public void sendDirectDrive(Int16 left, Int16 right)
    {
        byte[] buffer = new byte[256];
        int length = 0;
        buffer[length++] = (byte)Type.fromNetToUart;
        buffer[length++] = 5;
        buffer[length++] = (byte)PTU.ptuDirectDrive;
        buffer[length++] = (byte)(left >> 8);
        buffer[length++] = (byte)(left & 0xff);
        buffer[length++] = (byte)(right >> 8);
        buffer[length++] = (byte)(right & 0xff);
        client.GetStream().Write(buffer, 0, length);
        client.GetStream().Flush();
    }
    // buffer for recving
    private byte[] recvBuffer = new byte[1024];
    // how much we habe recvived
    private int recved = 0;
    // handler will be called on a debug packet recved
    public event Action<string, Int32> debugRecved = null;
    // handler for begin recved 
    public void recv(IAsyncResult res)
    {

        // ----------
        // recving
        // ----------

        // end the recve process
        // read tells us how many bytes where written to the buffer
        int read = client.GetStream().EndRead(res);
        // so the total bytes recved are added
        recved += read;

        // ----------
        // parsing
        // ----------
        // offset of the current parsed packet in the buffer
        int offset = 0;
        while (recved > 3)
        {
            // the length of the packets user data (containing the id)
            int packetLength = recvBuffer[offset + 1];
            // if we hve not recved a whole packet
            if (recved < packetLength)
                return;

            // get the packet id from the buffer
            int id = recvBuffer[offset + 2];
            if (id == ((int)FromUcToPc.debug))
            {
                // read (nulltermiting string) msg
                string msg = "";
                int i;
                for (i = offset + 3; recvBuffer[i] != 0; ++i) {
                    msg += (char)recvBuffer[i];
                }
                ++i;
                // read 32 bit value
                Int32 val = 0;
                val |= ((Int32)recvBuffer[i++]) << 24;
                val |= ((Int32)recvBuffer[i++]) << 16;
                val |= ((Int32)recvBuffer[i++]) << 8;
                val |= ((Int32)recvBuffer[i++]) << 0;
                if (debugRecved != null)
                    debugRecved(msg, val);
            }
            // we processed the packet, so "delete" it from the buffer
            offset += packetLength + 2;
            recved -= packetLength + 2;
        }
        // copy the begin of the next packet to the front of the buffer
        Array.Copy(recvBuffer, offset, recvBuffer, 0, recved);

        // start reading again
        client.GetStream().BeginRead(recvBuffer, recved, recvBuffer.Length - recved, recv, null);
    }
}
