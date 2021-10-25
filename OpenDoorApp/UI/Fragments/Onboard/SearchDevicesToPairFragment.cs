using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Com.Airbnb.Lottie;
using OpenDoorApp.Helpers;
using OpenDoorApp.Services.Natives;
using OpenDoorApp.UI.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OpenDoorApp.UI.Fragments.Onboard
{
    [Obsolete]
    public class SearchDevicesToPairFragment : Fragment
    {
        private MainActivity _mainActivity;
        private ConstraintLayout _container;
        private RecyclerView _devicesList;
        private BroadcastReceiver _receiver;
        private AppCompatButton _nextBtn;
        private TextView _refresh;
        private LottieAnimationView _lottie;
        private List<string> _pairedDevices;
        private SelectDoorDeviceAdapter _adapter;

        public int SelectedDevicePosition { get; private set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.SearchDevicesToPairFragment, container, false);

            _mainActivity = (MainActivity)Activity;

            _container = view.FindViewById<ConstraintLayout>(Resource.Id.container);
            _devicesList = view.FindViewById<RecyclerView>(Resource.Id.devicesList);
            _nextBtn = view.FindViewById<AppCompatButton>(Resource.Id.turnOnBtn);
            _refresh = view.FindViewById<TextView>(Resource.Id.dontFindDevice);
            _lottie = view.FindViewById<LottieAnimationView>(Resource.Id.imageView);

            _devicesList.SetLayoutManager(new LinearLayoutManager(Context));

            _pairedDevices = new List<string>();

            SelectedDevicePosition = -1;

            return view;
        }

        public override void OnPause()
        {
            base.OnPause();

            CleanBindings();

            Activity.UnregisterReceiver(_receiver);
        }

        public override void OnResume()
        {
            base.OnResume();

            _receiver = _receiver ?? new GetBluetoothDevicesReceiver(AddDevice, StartSearch, EndSearch);

            SetUI();
            SetupBindings();

            // Register for broadcasts when a device is discovered.
            IntentFilter filter = new IntentFilter();

            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);

            Activity.RegisterReceiver(_receiver, filter);

            StartScan();
        }

        public async Task StartScan()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            if (ContextCompat.CheckSelfPermission(Context, Manifest.Permission.AccessFineLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Activity, 
                                                  new string[] { Manifest.Permission.AccessFineLocation },
                                                  MainActivity.EnableLocationRequestCode);
            }
            else
            {
                adapter.StartDiscovery();
            }
        }

        private void SetUI()
        {
            SetRecyclerView();
            _container.SetBackgroundDrawable(GradientHelper.GetBackground());
        }

        private void SetRecyclerView()
        {
            _adapter = new SelectDoorDeviceAdapter(_pairedDevices, SelectDevicePosition, GetSelectDevicePosition);
            _devicesList.SetAdapter(_adapter);
        }

        private void SetupBindings()
        {
            _nextBtn.Click += NextBtnClick;
            _refresh.Click += RefreshClick;
        }

        private void CleanBindings()
        {
            _nextBtn.Click -= NextBtnClick;
            _refresh.Click -= RefreshClick;
        }
        private void RefreshClick(object sender, EventArgs e)
        {
            SelectedDevicePosition = -1;
            StartScan();
        }

        private void NextBtnClick(object sender, EventArgs e)
        {
            Preferences.Set(HomepageFragment.LastDeviceSelected, _pairedDevices[SelectedDevicePosition]);
            FragmentsHelper.ShowFragment(Activity, new HomepageFragment());
        }

        public void AddDevice(BluetoothDevice bluetoothDevice)
        {
            _pairedDevices.Add(bluetoothDevice.Name ?? bluetoothDevice.Address);
            _adapter.PairedDevices = _pairedDevices;
            _adapter.NotifyItemInserted(_pairedDevices.Count - 1);
        }

        private void StartSearch()
        {
            _pairedDevices.Clear();
            _adapter.PairedDevices = _pairedDevices;

            _adapter = new SelectDoorDeviceAdapter(_pairedDevices, SelectDevicePosition, GetSelectDevicePosition);
            _devicesList.SetAdapter(_adapter);

            _lottie.SetAnimation("Lotties/bluetooth_search.json");
            _lottie.PlayAnimation();
            _refresh.Visibility = ViewStates.Invisible;
            _refresh.Enabled = false;
        }

        private void EndSearch()
        {
            _refresh.Enabled = true;
            _refresh.Visibility = ViewStates.Visible;
            _lottie.SetAnimation("Lotties/select.json");
            _lottie.PlayAnimation();
        }

        public void SelectDevicePosition(int pos)
        {
            SelectedDevicePosition = pos;
        }

        public int GetSelectDevicePosition()
        {
            return SelectedDevicePosition;
        }
    }
}