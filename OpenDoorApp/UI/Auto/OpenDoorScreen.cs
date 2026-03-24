using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using Autofac;
using OpenDoorApp.Services.Interfaces;
using OpenDoorApp.UI.Fragments;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OpenDoorApp.UI.Auto
{
    public class OpenDoorScreen : Screen
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly string _deviceName;
        private bool _isConnected;
        private bool _isConnecting;

        public OpenDoorScreen(CarContext carContext) : base(carContext)
        {
            _bluetoothService = App.Container.Resolve<IBluetoothService>();
            _deviceName = Preferences.Get(HomepageFragment.LastDeviceSelected, string.Empty);
            StartConnection();
        }

        private void StartConnection()
        {
            if (string.IsNullOrEmpty(_deviceName))
                return;

            _isConnecting = true;
            Task.Run(() =>
            {
                _bluetoothService.Ping(
                    CarContext,
                    _deviceName,
                    connected =>
                    {
                        // BluetoothService calls this with false on each retry attempt
                        // and with true once the connection succeeds.
                        _isConnected = connected;
                        _isConnecting = !connected;
                        Invalidate();
                    },
                    () =>
                    {
                        _isConnected = false;
                        _isConnecting = false;
                        Invalidate();
                    },
                    data => { });
            });
        }

        public override ITemplate OnGetTemplate()
        {
            string statusMessage;
            if (string.IsNullOrEmpty(_deviceName))
            {
                statusMessage = CarContext.GetString(Resource.String.car_no_device_configured);
            }
            else if (_isConnecting)
            {
                statusMessage = CarContext.GetString(Resource.String.car_connecting);
            }
            else if (_isConnected)
            {
                statusMessage = CarContext.GetString(Resource.String.car_connected);
            }
            else
            {
                statusMessage = CarContext.GetString(Resource.String.car_disconnected);
            }

            var openClickListener = ParkedOnlyOnClickListener.Create(new OpenDoorClickListener(this));
            var openAction = new Action.Builder()
                .SetTitle(CarContext.GetString(Resource.String.car_open_door))
                .SetOnClickListener(openClickListener)
                .Build();

            return new MessageTemplate.Builder(statusMessage)
                .SetTitle(CarContext.GetString(Resource.String.app_name))
                .AddAction(openAction)
                .Build();
        }

        private void OpenDoor()
        {
            if (_isConnected)
            {
                _bluetoothService.SendCommand(_bluetoothService.OpenCommand);
            }
        }

        private sealed class OpenDoorClickListener : Java.Lang.Object, IOnClickListener
        {
            private readonly OpenDoorScreen _screen;

            public OpenDoorClickListener(OpenDoorScreen screen)
            {
                _screen = screen;
            }

            public void OnClick() => _screen.OpenDoor();
        }
    }
}
