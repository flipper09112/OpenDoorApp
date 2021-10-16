using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Autofac;
using OpenDoorApp.Services;
using OpenDoorApp.Services.Interfaces;
using OpenDoorApp.UI.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OpenDoorApp.UI.Fragments
{
    [Obsolete]
    public class HomepageFragment : Fragment
    {
        private IBluetoothService _bluetoothService;

        private Spinner _btDevicesSpinner;
        private Button _openBtn;
        private ImageView _connectedSignal;
        private HomeSpinnerAdapter _spinnerAdapter;

        public static string LastDeviceSelected = "LastDeviceSelectedKey";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.HomepageFragment, container, false);

            _bluetoothService = App.Container.Resolve<IBluetoothService>();

            _btDevicesSpinner = view.FindViewById<Spinner>(Resource.Id.btDevicesSpinner);
            _openBtn = view.FindViewById<Button>(Resource.Id.openBtn);
            _connectedSignal = view.FindViewById<ImageView>(Resource.Id.connectedSignal);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            SetupBindings();
            SetUI();
            StartBTConnection();
        }

        public override void OnPause()
        {
            base.OnPause();
            CleanBindings();

            _bluetoothService.Stop();
        }

        private void StartBTConnection()
        {
            Task.Run(() => { _bluetoothService.Ping(_spinnerAdapter[0], UpdateConnectedInfo, SomethingWrong, ShowDataReceived); });
        }

        private void SetupBindings()
        {
            _btDevicesSpinner.ItemSelected += BtDevicesSpinnerItemSelected;
            _openBtn.Click += OpenBtnClick;
        }
        private void CleanBindings()
        {
            _btDevicesSpinner.ItemSelected -= BtDevicesSpinnerItemSelected;
            _openBtn.Click -= OpenBtnClick;
        }

        private void UpdateConnectedInfo(bool connected)
        {
            if (connected == false)
            {
               // Activity.RunOnUiThread(() => { Toast.MakeText(Context, "try connect", ToastLength.Long).Show(); });
                _connectedSignal.SetBackgroundResource(Resource.Drawable.red_circle);
            }
            else
            {
               // Activity.RunOnUiThread(() => { Toast.MakeText(Context, "Connect", ToastLength.Long).Show(); });
                _connectedSignal.SetBackgroundResource(Resource.Drawable.green_circle);
            }
            Activity.RunOnUiThread(() => { _openBtn.Enabled = connected; });
        }

        private void SomethingWrong()
        {
            Activity.RunOnUiThread(() => { 
                Toast.MakeText(Context, "Erro", ToastLength.Long).Show();
                _connectedSignal.SetBackgroundResource(Resource.Drawable.red_circle);
                _openBtn.Enabled = false;
            });
        }

        private void ShowDataReceived(string data)
        {
            Activity.RunOnUiThread(() => { Toast.MakeText(Context, data, ToastLength.Long).Show(); });
        }

        private void BtDevicesSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Preferences.Set(LastDeviceSelected, _spinnerAdapter[e.Position]);
        }

        private void OpenBtnClick(object sender, EventArgs e)
        {
            _bluetoothService.SendCommand(_bluetoothService.OpenCommand);
        }

        private void SetUI()
        {
            List<string> pairedDevices = GetPairedDevices();
            _spinnerAdapter = new HomeSpinnerAdapter(pairedDevices);
            _btDevicesSpinner.Adapter = _spinnerAdapter;
        }

        private List<string> GetPairedDevices()
        {
            BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            var pairedDevices = mBluetoothAdapter.BondedDevices; 
            
            var s = new List<string>();
            foreach (var bt in pairedDevices)
                s.Add(bt.Name);

            if (s.Count == 0)
                s.Add("Sem Dispositivos");

            if(Preferences.Get(LastDeviceSelected, string.Empty) != string.Empty)
            {
                int pos = s.IndexOf(Preferences.Get(LastDeviceSelected, string.Empty));

                var first = s[0];
                s[0] = s[pos];
                s[pos] = first;
            }

            return s;
        }
    }
}