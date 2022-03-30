using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using AppCompatActivity = AndroidX.AppCompat.App.AppCompatActivity;
using OpenDoorApp.Helpers;
using OpenDoorApp.UI.Fragments;
using Android.Bluetooth;
using AndroidX.CoordinatorLayout.Widget;
using Android.Content;
using Xamarin.Essentials;
using Autofac;
using OpenDoorApp.Services.Interfaces;
using OpenDoorApp.UI.Fragments.Onboard;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

namespace OpenDoorApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private FrameLayout _frame;
        private RelativeLayout _containerFragsLayout;
        private IOnboardService _onboardService;
        private Toolbar _toolbar;

        public static int EnableBluetoothRequestCode => 1234;
        public static int EnableLocationRequestCode => 1235;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            AppCenter.Start("67dfd7c4-f121-4a35-a0df-52178ce63e6c", typeof(Analytics), typeof(Crashes));
            SetContentView(Resource.Layout.activity_main);

            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);

            _frame = FindViewById<FrameLayout>(Resource.Id.fragmentContainer);
            _containerFragsLayout = FindViewById<RelativeLayout>(Resource.Id.containerFragsLayout);

            _onboardService = App.Container.Resolve<IOnboardService>();

            FragmentsHelper.ShowFragment(this, GetFirstFragment());
        }

        internal void HideToolbar()
        {
            CoordinatorLayout.LayoutParams param = (CoordinatorLayout.LayoutParams)_containerFragsLayout.LayoutParameters;
            param.SetMargins(0,0,0,0);
            _containerFragsLayout.LayoutParameters = param;

            SupportActionBar.Hide();
        }

        internal void ShowToolbar()
        {
            CoordinatorLayout.LayoutParams param = (CoordinatorLayout.LayoutParams)_containerFragsLayout.LayoutParameters;
            param.SetMargins(0, 44, 0, 0);
            _containerFragsLayout.LayoutParameters = param;

            SupportActionBar.Show();
        }

        private Fragment GetFirstFragment()
        {
            var _mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            var deviceSelected = Preferences.Get(HomepageFragment.LastDeviceSelected, string.Empty);

            if (_mBluetoothAdapter == null)
            {
                //myLabel.setText("No bluetooth adapter available");
            }
            
            if (deviceSelected == string.Empty || deviceSelected == null)
            {
                _onboardService.IsOnboarding = true;
                return new FirstConnectDoorDeviceFragment();
            }
            else if (!_mBluetoothAdapter.IsEnabled)
            {
                return new TurnOnBluetoothFragment();
            }

            return new HomepageFragment();
        }

        /* public override bool OnCreateOptionsMenu(IMenu menu)
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
         }*/

        public override void OnBackPressed()
        {
            Fragment frag = FragmentManager.FindFragmentById(Resource.Id.fragmentContainer);

            if(frag is HomepageFragment)
            {
                //ignore
            }
            else if(frag is TurnOnBluetoothFragment)
            {
                if(FragmentManager.BackStackEntryCount != 0)
                    base.OnBackPressed();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Fragment frag = FragmentManager.FindFragmentById(Resource.Id.fragmentContainer);
            if (EnableBluetoothRequestCode == requestCode)
            {
                if(resultCode == Result.Ok)
                {
                    if (_onboardService.IsOnboarding)
                    {
                        _onboardService.IsOnboarding = false;
                        FragmentsHelper.ShowFragment(this, new SelectDoorDeviceFragment(), nameof(SelectDoorDeviceFragment));
                    }
                    else
                    {
                        FragmentsHelper.ShowFragment(this, GetFirstFragment());
                    }
                }
            }

            if (EnableLocationRequestCode == requestCode)
            {
                if(frag is SearchDevicesToPairFragment devicesToPairFragment)
                {
                    devicesToPairFragment.StartScan();
                }
            }
        }
    }
}
