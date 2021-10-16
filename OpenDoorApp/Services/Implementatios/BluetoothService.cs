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
        private Action<bool> _updateConnected;
        private Action _somethingWrongAction;
        private Action<string> _showDataReceived;
        private bool _sendPing;
        private string _data;

        public string OpenCommand { get => "a"; }
        private string PingCommand { get => "p"; }

		public void Ping(string deviceName, Action<bool> updateConnected, Action somethingWrongAction, Action<string> showDataReceived)
        {
			_updateConnected = updateConnected;
			_somethingWrongAction = somethingWrongAction;
			_showDataReceived = showDataReceived;

			FindBT(deviceName);
			OpenBT(updateConnected);

			StartServer();
        }

        private void StartPingRequests()
        {
			Handler handler = new Handler();

			_stopPingWorker = false;
			_workerPingThread = new Thread(Ping);

			_workerPingThread.Start();
		}

        private void StartServer()
		{
			Handler handler = new Handler();
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
			if (_mBluetoothAdapter == null)
			{
				//myLabel.setText("No bluetooth adapter available");
			}

			if (!_mBluetoothAdapter.IsEnabled)
			{
				//Intent enableBluetooth = new Intent(BluetoothAdapter.ActionRequestEnable);
				//StartActivityForResult(enableBluetooth, 0);
			}

			var pairedDevices = _mBluetoothAdapter.BondedDevices;
			_mmDevice = _mBluetoothAdapter.BondedDevices.First(item => item.Name == deviceName);
			if (pairedDevices.Contains(_mmDevice))
			{
				//myLabel.setText("Bluetooth Device Found, address: " + mmDevice.getAddress());
				//Log.d("ArduinoBT", "BT is paired");
			}

			//myLabel.setText("Bluetooth Device Found");
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

					updateConnected.Invoke(true);
					StartPingRequests();
				}
				catch (System.Exception e) { }

				Task.Delay(1000);
			}


			_mmOutputStream = _mmSocket.OutputStream;
			_mmInputStream = _mmSocket.InputStream;
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

							Log.Debug("BluetoothService", "Receive => " + data);
							
							_readBufferPosition = 0;
						}
					}
				} 
				catch (IOException ex) {
					_stopWorker = true;
					_somethingWrongAction?.Invoke();
				}
			}
		}

		private void Ping()
		{
			while (!Thread.CurrentThread().IsInterrupted && !_stopPingWorker)
			{
				try
				{
					_sendPing = true;
					SendCommand(PingCommand);
					Task.Delay(2500);
					Log.Debug("BluetoothService", "Ping");

					if (_data == "ping")
                    {
						_updateConnected.Invoke(true);
					}
					else
					{
						_updateConnected.Invoke(false); 
						_stopWorker = true;
					}
				}
				catch (IOException ex)
				{
					//_stopWorker = true;
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