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
using OpenDoorApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.UI.Fragments
{
    [Obsolete]
    public class FirstConnectDoorDeviceFragment : Fragment
    {
        private MainActivity _mainActivity;
        private ConstraintLayout _container;
        private AppCompatButton _nextBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.FirstConnectDoorDeviceFragment, container, false);

            _mainActivity = (MainActivity)Activity;

            _container = view.FindViewById<ConstraintLayout>(Resource.Id.container);
            _nextBtn = view.FindViewById<AppCompatButton>(Resource.Id.turnOnBtn);

            return view;
        }
        public override void OnPause()
        {
            base.OnPause();

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
            _nextBtn.Click += NextBtnClick;
        }

        private void CleanBindings()
        {
            _nextBtn.Click -= NextBtnClick;
        }

        private void NextBtnClick(object sender, EventArgs e)
        {
            var _mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (!_mBluetoothAdapter.IsEnabled)
            {
                FragmentsHelper.ShowFragment(Activity, new TurnOnBluetoothFragment());
            }
            else
            {
                FragmentsHelper.ShowFragment(Activity, new SelectDoorDeviceFragment());
            }
        }

        private void SetUI()
        {
            _container.SetBackgroundDrawable(GradientHelper.GetBackground());
        }
    }
}