using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ControllerHandlerServerApp
{
    
    public class VideoFrameReceiver
    {
        private UdpClient client;
        private Timer receiveDataTimer;
        private Timer receiveJumboDataTimer;
        public int Port { get; set; }
        private byte[][][] assembledFrames;
        int[] framePacketsCount;
        private int lastPushedFrame;
        private const int FRAME_THRESHOLD = 50;
        private const int MTU = 1300;
        public event EventHandler SetFrame;
        public byte[] pushedFrame { get; private set; }

        public VideoFrameReceiver(bool jumbo)
        {
            client = new UdpClient(0);
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            if (!jumbo)
            {
                receiveDataTimer = new Timer();
                receiveDataTimer.Elapsed += ReceiveDataTimer_Elapsed;
                receiveDataTimer.Interval = 0.5;
                receiveDataTimer.AutoReset = true;
                receiveDataTimer.Start();
            }
            else
            {
                receiveJumboDataTimer = new Timer();
                receiveJumboDataTimer.Elapsed += ReceiveJumboDataTimer_Elapsed;
                receiveJumboDataTimer.Interval = 2.0;
                receiveJumboDataTimer.AutoReset = true;
                receiveJumboDataTimer.Start();
            }
            assembledFrames = new byte[256][][];
            framePacketsCount = new int[256];
            lastPushedFrame = 0;
        }

        private async void ReceiveJumboDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var data = await client.ReceiveAsync();
            pushedFrame = data.Buffer;
            SetFrame?.Invoke(this, null);
        }

        private async void ReceiveDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var data = await client.ReceiveAsync();
            byte[] dataBuffer = data.Buffer.Take(MTU).ToArray();
            byte[] counterHeader = { 0xFF, 0xFF, 0xFF, 0xFF }; // frame meta-packet: first 4 values are 0xFF, then comes frame number (mod 255), then comes packet count
            //frame packet: frame number, packet number, packet data
            if (dataBuffer.Take(4).SequenceEqual(counterHeader))
            {
                if (dataBuffer[5] == 0)
                {
                    assembledFrames[dataBuffer[4]] = new byte[1][];
                    assembledFrames[dataBuffer[4]][0] = new byte[MTU];
                }
                assembledFrames[dataBuffer[4]] = new byte[dataBuffer[5]][];
                for(int i = 0; i<dataBuffer[5]; i++)
                {
                    assembledFrames[dataBuffer[4]][i] = new byte[MTU];
                }
            }
            else
            {
              if(assembledFrames[dataBuffer[0]] != null && assembledFrames[dataBuffer[0]][dataBuffer[1]] != null && framePacketsCount[dataBuffer[0]] > 0)
                assembledFrames[dataBuffer[0]][dataBuffer[1]] = dataBuffer.TakeLast(dataBuffer.Count() - 2).OrderBy(x => x == 0).ToArray();
            }
            framePacketsCount[dataBuffer[0]]++;
            if(assembledFrames[dataBuffer[0]] != null && framePacketsCount[dataBuffer[0]] == assembledFrames[dataBuffer[0]].Count() + 1)
            {
                framePacketsCount[dataBuffer[0]] = 0;
                PushAssembledFrame(assembledFrames[dataBuffer[0]], dataBuffer[0]);
            }

            
        }

        private void PushAssembledFrame(byte[][] vs, int frameNo)
        {
            if(frameNo < lastPushedFrame && lastPushedFrame - frameNo < FRAME_THRESHOLD)
            {
                return;
            }
            byte[] frame = vs.SelectMany(x => x).ToArray();
            pushedFrame = frame;
            lastPushedFrame = frameNo;
            SetFrame?.Invoke(this, null);




        }
    }
}