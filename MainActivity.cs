using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Text;

namespace ControllerHandlerServerApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button startServerButton;
        ConnectionHandler connectionHandler;
        TextView serverPortText;
        EditText connectToIpText;

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
        }

        private void StartServerButton_Click(object sender, EventArgs e)
        {
            string ipString = connectToIpText.Text.Split(':')[0];
            string portString = connectToIpText.Text.Split(':')[1];
            connectionHandler = new ConnectionHandler(ipString,Int32.Parse(portString));
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
