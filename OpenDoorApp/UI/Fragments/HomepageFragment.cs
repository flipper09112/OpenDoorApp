using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Autofac;
using Com.Airbnb.Lottie;
using OpenDoorApp.Helpers;
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

        private TextView _btDevicesSpinner;
        private ImageView _connectedSignal;
        private ConstraintLayout _container;
        private LottieAnimationView _openLottieBtn;
        private HomeSpinnerAdapter _spinnerAdapter;

        public static string LastDeviceSelected = "LastDeviceSelectedKey";
        private bool _doorConnected;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.HomepageFragment, container, false);

            _bluetoothService = App.Container.Resolve<IBluetoothService>();

            _btDevicesSpinner = view.FindViewById<TextView>(Resource.Id.btDevicesSpinner);
            _connectedSignal = view.FindViewById<ImageView>(Resource.Id.connectedSignal);
            _container = view.FindViewById<ConstraintLayout>(Resource.Id.container);
            _openLottieBtn = view.FindViewById<LottieAnimationView>(Resource.Id.openLottieBtn);

            _openLottieBtn.SetMaxProgress(0.50f);
            _openLottieBtn.PlayAnimation();

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

        }

        public override void OnDestroy()
        {
            _bluetoothService.Stop();
        }

        private void StartBTConnection()
        {
            Task.Run(() => { _bluetoothService.Ping(Context, Preferences.Get(LastDeviceSelected, string.Empty), UpdateConnectedInfo, SomethingWrong, ShowDataReceived); });
        }

        private void SetupBindings()
        {
            _openLottieBtn.Click += OpenLottieBtnClick;
        }
        private void CleanBindings()
        {
            _openLottieBtn.Click -= OpenLottieBtnClick;
        }

        private void OpenLottieBtnClick(object sender, EventArgs e)
        {
            if(_doorConnected)
            {
                _openLottieBtn.Speed = -1 * _openLottieBtn.Speed;
                _openLottieBtn.SetMinProgress(0.41f);
                _openLottieBtn.PlayAnimation();

                Vibrate();

                _bluetoothService.SendCommand(_bluetoothService.OpenCommand);
            }
        }

        private void Vibrate()
        {
            Vibrator v = (Vibrator)Activity.GetSystemService(Context.VibratorService);
            // Vibrate for 500 milliseconds
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.O)
            {
                v.Vibrate(VibrationEffect.CreateOneShot(200, VibrationEffect.DefaultAmplitude));
            }
            else
            {
                //deprecated in API 26 
                v.Vibrate(200);
            }
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
            _doorConnected = connected;
        }

        private void SomethingWrong()
        {
            Activity.RunOnUiThread(() => { 
                Toast.MakeText(Context, "Erro", ToastLength.Long).Show();
                _connectedSignal.SetBackgroundResource(Resource.Drawable.red_circle);
                _doorConnected = false;
            });
        }

        private void ShowDataReceived(string data)
        {
           // Activity.RunOnUiThread(() => { Toast.MakeText(Context, data, ToastLength.Long).Show(); });
        }

        private void SetUI()
        {
            _btDevicesSpinner.Text = Preferences.Get(LastDeviceSelected, string.Empty);
            _container.SetBackgroundDrawable(GradientHelper.GetBackground());
        }
    }
}