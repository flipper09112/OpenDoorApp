using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.Services.Natives
{
    [BroadcastReceiver]
    public class GetBluetoothDevicesReceiver : BroadcastReceiver
    {
        private Action<BluetoothDevice> addDevice;
        private Action startSearch;
        private Action endSearch;

        public GetBluetoothDevicesReceiver()
        {

        }

        public GetBluetoothDevicesReceiver(Action<BluetoothDevice> addDevice, Action startSearch, Action endSearch)
        {
            this.addDevice = addDevice;
            this.startSearch = startSearch;
            this.endSearch = endSearch;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            if (BluetoothDevice.ActionFound.Equals(action))
            {
                // Discovery has found a device. Get the BluetoothDevice
                // object and its info from the Intent.
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                /*String deviceName = device.getName();
                String deviceHardwareAddress = device.getAddress(); // MAC address*/
                addDevice?.Invoke(device);
            }

            if (BluetoothAdapter.ActionDiscoveryStarted.Equals(action)) startSearch?.Invoke();
            if (BluetoothAdapter.ActionDiscoveryFinished.Equals(action)) endSearch?.Invoke();

        }
    }
}