using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NFCTest.Resources;
using Windows.Networking.Proximity;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using NFCTest.SonyNdefUtils;




namespace NFCTest
{
    public partial class MainPage : PhoneApplicationPage
    {

        private ProximityDevice _device;
        private long _subscriptionIdNdef;
        private String output;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            output = "";

            Loaded += (s, e) => {
                this.initNFC();
            };
        }

        private void initNFC()
        {
            // Initialize NFC
            _device = ProximityDevice.GetDefault();
            // Only subscribe for messages if no NDEF subscription is already active
            if (_subscriptionIdNdef != 0 || _device == null)
            {
                Debug.WriteLine("It seems there's not NFC available device");
                return;
            }
            // Ask the proximity device to inform us about any kind of NDEF message received from
            // another device or tag.
            // Store the subscription ID so that we can cancel it later.
            _subscriptionIdNdef = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
 
        }

        private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            // Get the raw NDEF message data as byte array
            var parser = new SonyNdefParser(message);
            List<SonyNdefRecord> ndefRecords = new List<SonyNdefRecord>();
            try
            {
                ndefRecords = parser.Parse();
            }
            catch (NoNdefRecordException e)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("It seems there's no Sony's format record");
                });                
            }
            catch (NdefParseException e)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Parse error occured.");
                });
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Unexpected error has occured.");
                });
            }

            if (ndefRecords.Count > 0)
            {
                var record = ndefRecords[0];
                output = "SSID : " + record.SSID + Environment.NewLine + "Password: " + record.Password;

                Dispatcher.BeginInvoke(() =>
                {
                    OutputText.Text = output;
                });
            }
        }
    }
}