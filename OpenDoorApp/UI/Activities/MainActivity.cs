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

namespace OpenDoorApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private FrameLayout _frame;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            _frame = FindViewById<FrameLayout>(Resource.Id.fragmentContainer);

            FragmentsHelper.ShowFragment(this, GetFirstFragment());
        }

        private Fragment GetFirstFragment()
        {
            var _mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (_mBluetoothAdapter == null)
            {
                //myLabel.setText("No bluetooth adapter available");
            }

            if (!_mBluetoothAdapter.IsEnabled)
            {
                //return new TurnOnBluetoothFragment();
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
	}
}
