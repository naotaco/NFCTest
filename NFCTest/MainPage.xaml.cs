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
using NdefLibrary.Ndef;
using Windows.Networking.Proximity;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace NFCTest
{
    public partial class MainPage : PhoneApplicationPage
    {

        private ProximityDevice _device;
        private long _subscriptionIdNdef;
        private long _publishingMessageId;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();


            Loaded += (s, e) => {
                this.initNFC();
            };
        }

        private void initNFC()
        {
            // Initialize NFC
            _device = ProximityDevice.GetDefault();
            // Only subscribe for messages if no NDEF subscription is already active
            if (_subscriptionIdNdef != 0) return;
            // Ask the proximity device to inform us about any kind of NDEF message received from
            // another device or tag.
            // Store the subscription ID so that we can cancel it later.
            _subscriptionIdNdef = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
        }

        private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            // Get the raw NDEF message data as byte array
            var rawMsg = message.Data.ToArray();

            var parser = new SonyNdefParser(rawMsg);

            Debug.WriteLine("raw length: " + rawMsg.Length);

            StringBuilder sb = new StringBuilder();
            var values = new List<string>();

            for (int i = 0; i < rawMsg.Length; i++)
            {

                int hex = (int)rawMsg[i];
                char buf = (char)rawMsg[i];
                // Debug.WriteLine(hex + " " + buf);

                if (hex < 20)
                {
                    if (sb.ToString().Length > 2)
                    {
                        values.Add(sb.ToString());
                    }
                    sb.Clear();
                }
                else
                {
                    sb.Append((char)rawMsg[i]);
                }
            }
            values.Add(sb.ToString());

            foreach (String str in values)
            {
                // Debug.WriteLine("append raw message: " + str);
            }


            // Let the NDEF library parse the NDEF message out of the raw byte array
            var ndefMessage = NdefMessage.FromByteArray(rawMsg);

            // Analysis result
            var tagContents = new StringBuilder();

            // Loop over all records contained in the NDEF message
            foreach (NdefRecord record in ndefMessage)
            {
                // --------------------------------------------------------------------------
                // Print generic information about the record
                if (record.Id != null && record.Id.Length > 0)
                {
                    // Record ID (if present)
                    tagContents.AppendFormat("Id: {0}\n", Encoding.UTF8.GetString(record.Id, 0, record.Id.Length));
                }
                
                // Record type
                if (record.Type != null && record.Type.Length > 0)
                {
                    tagContents.AppendFormat("Record type: {0}\n",
                                             Encoding.UTF8.GetString(record.Type, 0, record.Type.Length));
                }

                if (record.Payload != null && record.Payload.Length > 0)
                {
                    tagContents.AppendFormat("Payload: {0}\n", Encoding.UTF8.GetString(record.Payload, 0, record.Payload.Length));
                }

                // --------------------------------------------------------------------------
                // Check the type of each record
                // Using 'true' as parameter for CheckSpecializedType() also checks for sub-types of records,
                // e.g., it will return the SMS record type if a URI record starts with "sms:"
                // If using 'false', a URI record will always be returned as Uri record and its contents won't be further analyzed
                // Currently recognized sub-types are: SMS, Mailto, Tel, Nokia Accessories, NearSpeak, WpSettings
                var specializedType = record.CheckSpecializedType(true);

                if (specializedType == typeof(NdefMailtoRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Mailto record info
                    var mailtoRecord = new NdefMailtoRecord(record);
                    tagContents.Append("-> Mailto record\n");
                    tagContents.AppendFormat("Address: {0}\n", mailtoRecord.Address);
                    tagContents.AppendFormat("Subject: {0}\n", mailtoRecord.Subject);
                    tagContents.AppendFormat("Body: {0}\n", mailtoRecord.Body);
                }
                else if (specializedType == typeof(NdefUriRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract URI record info
                    var uriRecord = new NdefUriRecord(record);
                    tagContents.Append("-> URI record\n");
                    tagContents.AppendFormat("URI: {0}\n", uriRecord.Uri);
                }
                else if (specializedType == typeof(NdefSpRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Smart Poster info
                    var spRecord = new NdefSpRecord(record);
                    tagContents.Append("-> Smart Poster record\n");
                    tagContents.AppendFormat("URI: {0}", spRecord.Uri);
                    tagContents.AppendFormat("Titles: {0}", spRecord.TitleCount());
                    if (spRecord.TitleCount() > 1)
                        tagContents.AppendFormat("1. Title: {0}", spRecord.Titles[0].Text);
                    tagContents.AppendFormat("Action set: {0}", spRecord.ActionInUse());
                    // You can also check the action (if in use by the record), 
                    // mime type and size of the linked content.
                }
                else if (specializedType == typeof(NdefLaunchAppRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract LaunchApp record info
                    var launchAppRecord = new NdefLaunchAppRecord(record);
                    tagContents.Append("-> LaunchApp record" + Environment.NewLine);
                    if (!string.IsNullOrEmpty(launchAppRecord.Arguments))
                        tagContents.AppendFormat("Arguments: {0}\n", launchAppRecord.Arguments);
                    if (launchAppRecord.PlatformIds != null)
                    {
                        foreach (var platformIdTuple in launchAppRecord.PlatformIds)
                        {
                            if (platformIdTuple.Key != null)
                                tagContents.AppendFormat("Platform: {0}\n", platformIdTuple.Key);
                            if (platformIdTuple.Value != null)
                                tagContents.AppendFormat("App ID: {0}\n", platformIdTuple.Value);
                        }
                    }
                }
                else
                {
                    if (record.Id != null)
                    {
                        String id = System.Text.Encoding.UTF8.GetString(record.Id, 0, record.Id.Length);
                        tagContents.Append("ID: " + id + Environment.NewLine);
                    }
                    if (record.Payload != null)
                    {
                        String pl = System.Text.Encoding.UTF8.GetString(record.Payload, 0, record.Payload.Length);
                        tagContents.Append("payload: " + pl + Environment.NewLine);
                    }
                    // Other type, not handled by this demo
                    tagContents.Append("NDEF record not parsed by this demo app" + Environment.NewLine);
                }
            }
        }

    }
}