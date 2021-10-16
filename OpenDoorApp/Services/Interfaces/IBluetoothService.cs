using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.Services.Interfaces
{
    public interface IBluetoothService
    {
        public string OpenCommand { get; }
        void Ping(string deviceName, Action<bool> updateConnected, Action somethingWrong, Action<string> showReceivedData);
        void Stop();
        void SendCommand(string openCommand);
    }
}