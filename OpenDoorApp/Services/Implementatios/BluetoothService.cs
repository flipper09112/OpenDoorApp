using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;
using OpenDoorApp.Services.Interfaces;
using OpenDoorApp.UI.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDoorApp.Services
{
    public class BluetoothService : IBluetoothService
    {
        private bool _stopWorker;
        private bool _stopPingWorker;
        private int _readBufferPosition;
        private byte[] _readBuffer;
        private Thread _workerThread;
        private BluetoothAdapter _mBluetoothAdapter;
        private BluetoothDevice _mmDevice;
        private BluetoothSocket _mmSocket;
        private Stream _mmOutputStream;
        private Stream _mmInputStream;
        private Thread _workerPingThread;
        private Context _context;
        private string _deviceName;
        private Action<bool> _updateConnected;
        private Action _somethingWrongAction;
        private Action<string> _showDataReceived;
        private bool _sendPing;
        private string _data;

        public string OpenCommand { get => "a"; }
        private string PingCommand { get => "p"; }

		public void Ping(Context context, string deviceName, Action<bool> updateConnected, Action somethingWrongAction, Action<string> showDataReceived)
        {
            _context = context;
			_deviceName = deviceName;
			_updateConnected = updateConnected;
			_somethingWrongAction = somethingWrongAction;
			_showDataReceived = showDataReceived;

			FindBT(deviceName);
			OpenBT(updateConnected);

			StartPingRequests();
			StartServer();
		}

		private void StartPingRequests()
        {
			 _stopPingWorker = false;
			_workerPingThread = new Thread(Ping);

			_workerPingThread.Start();
		}

        private void StartServer()
		{
			_stopWorker = false;
			_readBufferPosition = 0;
			_readBuffer = new byte[1024];
			_workerThread = new Thread(Server);

			_workerThread.Start();
		}

        public void Stop()
        {
			_stopWorker = true;
			_stopPingWorker = true;
		}

        private void FindBT(string deviceName)
        {
			_mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
			
			var pairedDevices = _mBluetoothAdapter.BondedDevices;
			_mmDevice = _mBluetoothAdapter.BondedDevices.First(item => item.Name == deviceName);
		}

		private void OpenBT(Action<bool> updateConnected)
		{
			UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"); //Standard //SerialPortService ID
			_mmSocket = _mmDevice.CreateRfcommSocketToServiceRecord(uuid);

			while(!_mmSocket.IsConnected && !_stopWorker)
			{
				try
				{
					updateConnected.Invoke(false);
					Log.Debug("BluetoothService", "Try connect");
					_mmSocket.Connect();

					SendBroadcast();
					updateConnected.Invoke(true);
				}
				catch (RuntimeException ex) {
					Log.Debug("BluetoothService", ex.StackTrace);
				}
				catch (System.Exception e)
				{
					Log.Debug("BluetoothService", e.StackTrace);
				}

				//Task.Delay(1000);
			}


			_mmOutputStream = _mmSocket.OutputStream;
			_mmInputStream = _mmSocket.InputStream;
		}

        private void SendBroadcast()
        {
			Intent intent = new Intent(_context, typeof(SimpleAppWidget));
			intent.SetAction(SimpleAppWidget.ACTION_CONNECTED);
			_context.SendBroadcast(intent);
		}

        private void Server()
        {
			while (!Thread.CurrentThread().IsInterrupted && !_stopWorker)
			{
				try
				{
					byte delimiter = 10; //This is the ASCII code for a newline character
					byte[] packetBytes = new byte[50];
					int bytesAvailable = _mmInputStream.Read(packetBytes);
					for (int i = 0; i < bytesAvailable; i++)
					{
						byte b = packetBytes[i];
						if (b == delimiter)
						{
							byte[] encodedBytes = new byte[_readBufferPosition];
							System.Array.Copy(_readBuffer, 0, encodedBytes, 0, encodedBytes.Length);
							string data = System.Text.Encoding.UTF8.GetString(encodedBytes);// new string(encodedBytes, "US-ASCII");

							_showDataReceived.Invoke(data);

							if (_sendPing)
							{
								_data = data;
								_sendPing = false;
							}

							Console.WriteLine("BluetoothService", "Receive => " + data);

							_readBufferPosition = 0;
						}
					}
				}
				catch (Java.IO.IOException ex)
				{
					_stopWorker = true;
					_somethingWrongAction?.Invoke();
					Log.Debug("BluetoothService", ex.StackTrace);
				}
				catch (Java.Lang.NullPointerException ex)
				{
					/*_stopWorker = true;
					_somethingWrongAction?.Invoke();*/
					Log.Debug("BluetoothService", ex.StackTrace);
				}
			}
		}

		private async void Ping()
		{
			while (!Thread.CurrentThread().IsInterrupted && !_stopPingWorker)
			{
				if (!_mmSocket.IsConnected) continue;
				try
				{
					_sendPing = true;

					Log.Debug("BluetoothService", "Ping Sended");
					SendCommand(PingCommand);
					await Task.Delay(2500);

					Log.Debug("BluetoothService", "Ping check response");
					if (_data == "ping")
                    {
						//_updateConnected.Invoke(true);
					}
					else if(_data == null)
					{
						//error here
						_mmInputStream.Close();
						_mmOutputStream.Close();

						_updateConnected.Invoke(false); 
						_stopWorker = true;
						_stopPingWorker = true;
						//Ping(_deviceName, )
					}
					_data = null;
				}
				catch (IOException ex)
				{
					//_somethingWrongAction?.Invoke();
					//_stopWorker = true;
					Log.Debug("BluetoothService", ex.StackTrace);
				}
			}
		}

		public void SendCommand(string openCommand)
        {
			byte[] bytes = Encoding.ASCII.GetBytes(openCommand);
			_mmOutputStream.Write(bytes, 0, bytes.Length);
        }
    }
}