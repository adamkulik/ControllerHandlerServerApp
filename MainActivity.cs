using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace ControllerHandlerServerApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button startServerButton;
        ConnectionHandler connectionHandler;
        TextView serverPortText;
        EditText connectToIpText;
        private UdpClient client;
        private Timer receiveFrameTimer;
        private Timer emptyFrameBufferTimer;
        private List<byte[]> frameBuffer = new List<byte[]>();
        ImageView videoPreviewSurface;
        private bool connected = false;
        private VideoFrameReceiver receiver;
        private byte[] frame;

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (connectionHandler != null)
            {
                for (int i = 0; i < e.HistorySize; i++)
                {
                    ProcessJoystickInput(e, i);
                }
                ProcessJoystickInput(e, -1); // process current event as well
            }
            return true;
        }

        private void ProcessJoystickInput(MotionEvent e, int i)
        {
            int gasVal = Helpers.GetGasValue(e, i);
            int turnVal = Helpers.GetTurnValue(e, i);
            byte[] data = Encoding.ASCII.GetBytes(gasVal.ToString() + turnVal.ToString());
            connectionHandler.SendData(data);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
            startServerButton = FindViewById<Button>(Resource.Id.startServerButton);
            startServerButton.Click += StartServerButton_Click;
            serverPortText = FindViewById<TextView>(Resource.Id.serverPortText);
            connectToIpText = FindViewById<EditText>(Resource.Id.connectToIpText);
            videoPreviewSurface = FindViewById<ImageView>(Resource.Id.videoPreviewSurface);
            receiver = new VideoFrameReceiver(true);
            receiver.SetFrame += Receiver_SetFrame;
            receiveFrameTimer = new Timer();
            receiveFrameTimer.Interval = 4.0;
            receiveFrameTimer.Elapsed += ReceiveFrameTimer_Elapsed1;
            receiveFrameTimer.AutoReset = true;
            receiveFrameTimer.Start();


        }

        private void ReceiveFrameTimer_Elapsed1(object sender, ElapsedEventArgs e)
        {
            if (frame != null)
            {
                RunOnUiThread(() => {
                    Bitmap bmp = BitmapFactory.DecodeByteArray(frame, 0, frame.Length);
                    if(bmp != null)
                    videoPreviewSurface.SetImageBitmap(bmp);
                });

                    
            }
        }

        private void Receiver_SetFrame(object sender, EventArgs e)
        {
            frame = receiver.pushedFrame;
        }

        public void OnInputDeviceRemoved(int deviceId)
        {
            int gasVal = Helpers.GAS_MIDDLE_VALUE;
            int turnVal = Helpers.TURN_MIDDLE_VALUE;
            byte[] data = Encoding.ASCII.GetBytes(gasVal.ToString() + turnVal.ToString());
            connectionHandler.SendData(data);
        }

        private void EmptyFrameBufferTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(frameBuffer.Count > 0)
            {
                Bitmap bmp = BitmapFactory.DecodeByteArray(frameBuffer.Last(), 0, frameBuffer[0].Length);
                videoPreviewSurface.SetImageBitmap(bmp);
                frameBuffer.Clear();

            }
        }

        private async void ReceiveFrameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (connected)
            {
                var result = await client.ReceiveAsync();
                frameBuffer.Add(result.Buffer);
            }
        }

        private void StartServerButton_Click(object sender, EventArgs e)
        {
            serverPortText.Text += "Port: " + receiver.Port.ToString();
            connected = true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
