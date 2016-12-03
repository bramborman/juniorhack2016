﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Project42
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage : Page
    {

        private ObservableCollection<DeviceInformation> _Devices
        {
            get { return BTWorker?._device; }
        }

        private StreamSocket _socket;
        BlueToothBackgroundWorker BTWorker;
        private RfcommDeviceService _service;
        DataReader reader;

        public TestPage()
        {
            this.InitializeComponent();

            BTWorker = new BlueToothBackgroundWorker();
        }

        private async void btnSend_Click(object sender,
                                         RoutedEventArgs e)
        {
            int dummy;

            if (!int.TryParse(tbInput.Text, out dummy))
            {
                tbError.Text = "Invalid input";
            }

            var noOfCharsSent = await Send(tbInput.Text);

            if (noOfCharsSent != 0)
            {
                tbError.Text = noOfCharsSent.ToString();
            }
        }
        private async Task<uint> Send(string msg)
        {
            tbError.Text = string.Empty;

            try
            {
                var writer = new DataWriter(_socket.OutputStream);

                writer.WriteString(msg);

                // Launch an async task to 
                //complete the write operation
                var store = writer.StoreAsync().AsTask();

                return await store;
            }
            catch (Exception ex)
            {
                tbError.Text = ex.Message;

                return 0;
            }
        }

        private async void btnConnect_Click(object sender,
                                            RoutedEventArgs e)
        {
            tbError.Text = string.Empty;

            try
            {
                var devices = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

                DeviceInformation temp = ((DeviceInformation)t.SelectedItem) ?? _Devices[0];

                if (temp == null)
                    throw new Exception("nefunguje toooooooooooooooooooo");

                var device = devices.FirstOrDefault(x => x.Id.Split('-')[1].Split('#')[0] == temp.Id.Split('-')[1]);

                if (device == null)
                    throw new Exception("nefunguje toooooooooooooooooooo");

                _service = await RfcommDeviceService.FromIdAsync(
                                                        device.Id);

                /*_socket = new StreamSocket();
                
                await _socket.ConnectAsync(_service.ConnectionHostName,
                                           _service.ConnectionServiceName);*/

                StreamSocketListener listener = new StreamSocketListener();
                listener.ConnectionReceived += Listener_ConnectionReceived;

                listener.BindServiceNameAsync("12345").AsTask().Wait();

                /*DataWriter writer = new DataWriter(_socket.OutputStream);

                writer.WriteString("1");
                await writer.StoreAsync();
                await writer.FlushAsync();*/

                /*var aqsFilter = SerialDevice.GetDeviceSelector("COM3");
                var devices = await DeviceInformation.FindAllAsync(aqsFilter);

                DeviceInformation temp = ((DeviceInformation)t.SelectedItem) ?? _Devices[0];

                var device = devices[0]; //devices.FirstOrDefault(x => x.Id.Split('-')[1].Split('#')[0] == temp.Id.Split('-')[1]);

                if (device != null)
                {
                    var deviceId = devices.First().Id;
                    var _device = await SerialDevice.FromIdAsync(deviceId);

                    if (device != null)
                    {
                        _device.BaudRate = 57600;
                        _device.StopBits = SerialStopBitCount.One;
                        _device.DataBits = 8;
                        _device.Parity = SerialParity.None;
                        _device.Handshake = SerialHandshake.None;

                        reader = new DataReader(_device.InputStream);
                    }
                }*/
            }
            catch (Exception ex)
            {
                tbError.Text = ex.Message;
            }
        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("new connection");

            using (var dw = new DataWriter(args.Socket.OutputStream))
            {
                dw.WriteString("1");
                await dw.StoreAsync();
                dw.DetachStream();
            }
        }

        private async void btnDisconnect_Click(object sender,
                                             RoutedEventArgs e)
        {
            tbError.Text = string.Empty;

            try
            {
                await _socket.CancelIOAsync();
                _socket.Dispose();
                _socket = null;
                _service.Dispose();
                _service = null;
            }
            catch (Exception ex)
            {
                tbError.Text = ex.Message;
            }
        }
    }
}