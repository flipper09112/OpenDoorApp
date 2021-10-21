using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using OpenDoorApp.Helpers;
using System;
namespace OpenDoorApp.UI.Fragments
{
    [Obsolete]
    public class TurnOnBluetoothFragment : Fragment
    {
        private MainActivity _mainActivity;
        private ConstraintLayout _container;
        private AppCompatButton _turnOnBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.TurnOnBluetoothFragment, container, false);

            _mainActivity = (MainActivity)Activity;

            _container = view.FindViewById<ConstraintLayout>(Resource.Id.container);
            _turnOnBtn = view.FindViewById<AppCompatButton>(Resource.Id.turnOnBtn);

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

            SetUI();
            SetupBindings();
        }

        private void SetupBindings()
        {
            _turnOnBtn.Click += TurnOnBtnClick;
        }

        private void CleanBindings()
        {
            _turnOnBtn.Click -= TurnOnBtnClick;
        }

        private void TurnOnBtnClick(object sender, EventArgs e)
        {
            Intent enableBluetooth = new Intent(BluetoothAdapter.ActionRequestEnable);
            Activity.StartActivityForResult(enableBluetooth, MainActivity.EnableBluetoothRequestCode);
        }

        private void SetUI()
        {
            _container.SetBackgroundDrawable(GradientHelper.GetBackground());
        }
    }
}
