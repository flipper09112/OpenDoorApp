using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.RecyclerView.Widget;
using OpenDoorApp.Helpers;
using OpenDoorApp.UI.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace OpenDoorApp.UI.Fragments
{
    [Obsolete]
    public class SelectDoorDeviceFragment : Fragment
    {
        private MainActivity _mainActivity;
        private ConstraintLayout _container;
        private RecyclerView _devicesList;
        private AppCompatButton _nextBtn;
        private TextView _dontFindDevice;
        private SelectDoorDeviceAdapter _adapter;

        public int SelectedDevicePosition { get; private set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SelectedDevicePosition = -1;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.SelectDoorDeviceFragment, container, false);

            _mainActivity = (MainActivity)Activity;

            _container = view.FindViewById<ConstraintLayout>(Resource.Id.container);
            _devicesList = view.FindViewById<RecyclerView>(Resource.Id.devicesList);
            _nextBtn = view.FindViewById<AppCompatButton>(Resource.Id.turnOnBtn);
            _dontFindDevice = view.FindViewById<TextView>(Resource.Id.dontFindDevice);

            _devicesList.SetLayoutManager(new LinearLayoutManager(Context));

            return view;
        }

        public override void OnPause()
        {
            base.OnPause();

            _mainActivity?.ShowToolbar();
            CleanBindings();
        }

        public override void OnResume()
        {
            base.OnResume();

            _mainActivity?.HideToolbar();

            SetUI();
            SetupBindings();
        }

        private void SetupBindings()
        {
            _dontFindDevice.Click += DontFindDeviceClick;
            _nextBtn.Click += NextBtnClick;
        }

        private void CleanBindings()
        {
            _dontFindDevice.Click -= DontFindDeviceClick;
            _nextBtn.Click -= NextBtnClick;
        }

        private void DontFindDeviceClick(object sender, EventArgs e)
        {
            //TODO: search new devices
        }

        private void NextBtnClick(object sender, EventArgs e)
        {
            Preferences.Set(HomepageFragment.LastDeviceSelected, GetPairedDevices()[SelectedDevicePosition]);
            FragmentsHelper.ShowFragment(Activity, new HomepageFragment());
        }

        private void SetUI()
        {
            SetRecyclerView();
            _container.SetBackgroundDrawable(GradientHelper.GetBackground());
            SetButtonLayout();
        }

        private void SetButtonLayout()
        {
            Activity.RunOnUiThread(() => {
                _nextBtn.Enabled = SelectedDevicePosition != -1;
            });
        }

        private void SetRecyclerView()
        {
            List<string> pairedDevices = GetPairedDevices();
            _adapter = new SelectDoorDeviceAdapter(pairedDevices, SelectDevicePosition, this);
            _devicesList.SetAdapter(_adapter);
        }

        private List<string> GetPairedDevices()
        {
            BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            var pairedDevices = mBluetoothAdapter.BondedDevices;

            var s = new List<string>();
            foreach (var bt in pairedDevices)
                s.Add(bt.Name);

            return s;
        }

        public void SelectDevicePosition(int pos)
        {
            SelectedDevicePosition = pos;
            SetButtonLayout();
        }
    }
}