using Android.Util;
using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using Autofac;
using OpenDoorApp.Services.Interfaces;
using OpenDoorApp.UI.Fragments;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Action = AndroidX.Car.App.Model.Action;

namespace OpenDoorApp.UI.Auto
{
    public class OpenDoorScreen : Screen
    {
        private const string TAG = "OpenDoorScreen";
        private readonly IBluetoothService _bluetoothService;
        private readonly string _deviceName;
        private bool _isConnected;
        private bool _isConnecting;
        private bool _isDoorOpening;
        private string _lastErrorMessage;
        private int _connectionRetries;
        private const int MaxRetries = 3;

        public OpenDoorScreen(CarContext carContext) : base(carContext)
        {
            _bluetoothService = App.Container.Resolve<IBluetoothService>();
            _deviceName = Preferences.Get(HomepageFragment.LastDeviceSelected, string.Empty);
            _connectionRetries = 0;
            Log.Debug(TAG, $"OpenDoorScreen initialized for device: {_deviceName}");
            StartConnection();
        }

        private void StartConnection()
        {
            if (string.IsNullOrEmpty(_deviceName))
            {
                Log.Warn(TAG, "No device configured");
                return;
            }

            _isConnecting = true;
            _isDoorOpening = false;
            _lastErrorMessage = string.Empty;
            Invalidate();

            Task.Run(() =>
            {
                try
                {
                    _bluetoothService.Ping(
                        CarContext,
                        _deviceName,
                        connected =>
                        {
                            _isConnected = connected;
                            _isConnecting = false;
                            if (connected)
                            {
                                _connectionRetries = 0;
                                _lastErrorMessage = string.Empty;
                                Log.Info(TAG, $"Connected to {_deviceName}");
                            }
                            Invalidate();
                        },
                        () =>
                        {
                            _isConnected = false;
                            _isConnecting = false;
                            _connectionRetries++;
                            _lastErrorMessage = CarContext.GetString(Resource.String.car_connection_failed);
                            Log.Error(TAG, $"Connection failed. Attempt {_connectionRetries}/{MaxRetries}");
                            Invalidate();
                        },
                        data =>
                        {
                            Log.Debug(TAG, $"Data received: {data}");
                        });
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, "Error during connection: " + ex.Message);
                    _isConnected = false;
                    _isConnecting = false;
                    _lastErrorMessage = CarContext.GetString(Resource.String.car_error_generic);
                    Invalidate();
                }
            });
        }

        public override ITemplate OnGetTemplate()
        {
            string statusMessage = GetStatusMessage();
            var messageBuilder = new MessageTemplate.Builder(statusMessage)
                .SetTitle(CarContext.GetString(Resource.String.app_name));

            // Add open door action if connected and not opening
            if (_isConnected && !_isDoorOpening)
            {
                var openClickListener = ParkedOnlyOnClickListener.Create(new OpenDoorClickListener(this));
                var openAction = new Action.Builder()
                    .SetTitle(CarContext.GetString(Resource.String.car_open_door))
                    .SetOnClickListener(openClickListener)
                    .Build();
                messageBuilder.AddAction(openAction);
            }

            // Add retry action if connection failed
            if (!_isConnected && !_isConnecting && _connectionRetries < MaxRetries)
            {
                var retryClickListener = new RetryClickListener(this);
                var retryAction = new Action.Builder()
                    .SetTitle(CarContext.GetString(Resource.String.car_retry))
                    .SetOnClickListener(retryClickListener)
                    .Build();
                messageBuilder.AddAction(retryAction);
            }

            return messageBuilder.Build();
        }

        private string GetStatusMessage()
        {
            if (string.IsNullOrEmpty(_deviceName))
            {
                return CarContext.GetString(Resource.String.car_no_device_configured);
            }

            if (_isDoorOpening)
            {
                return CarContext.GetString(Resource.String.car_opening_door);
            }

            if (_isConnecting)
            {
                return CarContext.GetString(Resource.String.car_connecting);
            }

            if (_isConnected)
            {
                return CarContext.GetString(Resource.String.car_connected);
            }

            // Show retry count if exceeded
            if (_connectionRetries >= MaxRetries)
            {
                return CarContext.GetString(Resource.String.car_max_retries_exceeded);
            }

            // Show error or disconnected
            if (!string.IsNullOrEmpty(_lastErrorMessage))
            {
                return _lastErrorMessage;
            }

            return CarContext.GetString(Resource.String.car_disconnected);
        }

        private void OpenDoor()
        {
            if (!_isConnected || _isDoorOpening)
            {
                Log.Warn(TAG, $"Cannot open door. Connected: {_isConnected}, Opening: {_isDoorOpening}");
                return;
            }

            try
            {
                _isDoorOpening = true;
                Log.Info(TAG, "Opening door...");
                Invalidate();

                Task.Run(() =>
                {
                    try
                    {
                        _bluetoothService.SendCommand(_bluetoothService.OpenCommand);
                        // Simulate operation feedback
                        Task.Delay(1000).Wait();
                        _isDoorOpening = false;
                        Log.Info(TAG, "Door opened successfully");
                        Invalidate();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(TAG, "Error sending open command: " + ex.Message);
                        _isDoorOpening = false;
                        _lastErrorMessage = CarContext.GetString(Resource.String.car_error_opening_door);
                        Invalidate();
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "Error opening door: " + ex.Message);
                _isDoorOpening = false;
                Invalidate();
            }
        }

        private void Retry()
        {
            if (_connectionRetries >= MaxRetries)
            {
                Log.Warn(TAG, "Max retries exceeded");
                return;
            }

            Log.Info(TAG, "Retrying connection...");
            StartConnection();
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

        private sealed class RetryClickListener : Java.Lang.Object, IOnClickListener
        {
            private readonly OpenDoorScreen _screen;

            public RetryClickListener(OpenDoorScreen screen)
            {
                _screen = screen;
            }

            public void OnClick() => _screen.Retry();
        }
    }
}
