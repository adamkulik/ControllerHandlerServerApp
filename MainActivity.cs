using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using System.Net;
using System.Net.Sockets;
using Android.Support.V4.App;
using Android;
using Xamarin.Essentials;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace ControllerHandlerServerApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button startServerButton;
        ConnectionHandler connectionHandler;
        TextView serverPortText;
        private static float GetCenteredAxis(MotionEvent e,
       InputDevice device, Axis axis, int historyPos)
        {
            InputDevice.MotionRange range =
           device.GetMotionRange(axis, e.Source);

            // A joystick at rest does not always report an absolute position of
            // (0,0). Use the getFlat() method to determine the range of values
            // bounding the joystick axis center.
            if (range != null)
            {
                float flat = range.Flat;
                float value =
                historyPos < 0 ? e.GetAxisValue(axis) :
                e.GetHistoricalAxisValue(axis, historyPos);

                // Ignore axis values that are within the 'flat' region of the
                // joystick axis center.
                if (Math.Abs(value) > flat)
                {
                    return value;
                }
            }
            return 0;
        }
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
            int gasVal = 1500;
            if (e != null && e.IsFromSource(InputSourceType.Joystick))
            {
                if (e.GetAxisValue(Axis.Rtrigger) > 0)
                {
                    gasVal = 1500 + Convert.ToInt32(400 * GetCenteredAxis(e, e.Device, Axis.Rtrigger, i));
                }
                else if (e.GetAxisValue(Axis.Ltrigger) > 0)
                {
                    gasVal = 1500 - Convert.ToInt32(400 * GetCenteredAxis(e, e.Device, Axis.Ltrigger, i));
                }


            }

            if (gasVal < 1100 || gasVal > 1900) gasVal = 1500;
            int turnVal = 0;
            if (e != null && e.IsFromSource(InputSourceType.Joystick))
            {
                turnVal = 180 - Convert.ToInt32((90 * GetCenteredAxis(e, e.Device, Axis.X, i)) + 90);
            }

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
        }

        private void StartServerButton_Click(object sender, EventArgs e)
        {
            connectionHandler = new ConnectionHandler(0);
            connectionHandler.StartServer();
            serverPortText.Text = connectionHandler.PortNumber.ToString();
            
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
            View view = (View) sender;
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
