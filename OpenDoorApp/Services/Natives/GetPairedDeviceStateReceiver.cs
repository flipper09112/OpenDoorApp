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
    public class GetPairedDeviceStateReceiver : BroadcastReceiver
    {
        private Action showHomepage;

        public GetPairedDeviceStateReceiver()
        {

        }

        public GetPairedDeviceStateReceiver(Action showHomepage)
        {
            this.showHomepage = showHomepage;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            if (BluetoothDevice.ActionBondStateChanged.Equals(action))
            {
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                if (device.BondState == Bond.Bonded)
                {
                    showHomepage?.Invoke();
                }
            }
        }
    }
}