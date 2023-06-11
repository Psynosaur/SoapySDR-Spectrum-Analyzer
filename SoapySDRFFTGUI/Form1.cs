using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Pothosware.SoapySDR;

namespace SoapySDRFFTGUI
{
    
    public partial class Form1 : Form
    {
        public bool firstSDR { get; set; }
        public List<Device> SDRs { get; set; } = new List<Device>();
        public Form1()
        {
            InitializeComponent();
            var argsStr = new List<string>();

            // Enumerate devices.
            var results = Device.Enumerate();
            foreach (var result in results)
            foreach (var tuple in result)
            {
                if (tuple.Key.Equals("driver"))
                {
                    argsStr.Add(tuple.Key.ToLowerInvariant() + "=" + tuple.Value.ToLowerInvariant());
                }
                // Console.WriteLine(tuple);
            }
            argsStr.Add("driver=plutosdr,hostname=192.168.2.1");

            foreach (var radio in argsStr)
            {
                // Create device instance.
                // Args can be user-defined or from the enumeration result.
                var args = radio;
                Device sdr = new Device(args);
                SDRs.Add(sdr);
                
                comboBox1.Items.Add(sdr.DriverKey);
                if (!firstSDR)
                {
                    firstSDR = true;
                    comboBox1.SelectedItem = sdr.DriverKey;
                }
                
                // Query device info.
                // Console.WriteLine("Antennas:");
                // foreach (var antenna in sdr.ListAntennas(Pothosware.SoapySDR.Direction.Rx, 0))
                //     Console.WriteLine(" * " + antenna);
                //
                // Console.WriteLine("Gains:");
                // foreach (var gain in sdr.ListGains(Pothosware.SoapySDR.Direction.Rx, 0))
                //     Console.WriteLine(" * " + gain);
                //
                // Console.WriteLine("Frequency ranges:");
                // foreach (var freqRange in sdr.GetFrequencyRange(Pothosware.SoapySDR.Direction.Rx, 0))
                //     Console.WriteLine(" * " + freqRange);
                // Console.WriteLine("SampleRate ranges:");
                // foreach (var sampleRate in sdr.GetSampleRateRange(Pothosware.SoapySDR.Direction.Rx, 0))
                //     Console.WriteLine(" * " + sampleRate);

                // Apply settings.
                //var sampleRate = sdr.GetSampleRate(Pothosware.SoapySDR.Direction.Rx, 0);
                // sdr.SetSampleRate(Pothosware.SoapySDR.Direction.Rx, 0, 2.5e06);
                // sdr.SetFrequency(Pothosware.SoapySDR.Direction.Rx, 0, 747.5e6);
                //
                // // Setup a stream (complex floats).
                // var rxStream = sdr.SetupRxStream("CF32" ,new uint[] { 0 }, "");
                // rxStream.Activate(); // Start streaming
                //
                // // Create a reusable array for RX samples.
                // var buff = new float[rxStream.MTU * 2];
                //
                // // Receive some samples.
                // for (var i = 0; i < 10; ++i)
                // {
                //     Pothosware.SoapySDR.StreamResult streamResult;
                //
                //     var errorCode = rxStream.Read(ref buff, 0, out streamResult);
                //     Console.WriteLine("Error code:     " + errorCode);
                //     Console.WriteLine("Receive flags:  " + streamResult.Flags);
                //     Console.WriteLine("Timestamp (ns): " + streamResult.TimeNs);
                // }
                //
                // // Shut down the stream.
                // rxStream.Deactivate();
                // rxStream.Close();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Query device info.
            var selectedSDR = comboBox1.SelectedItem;
            foreach (var sdr in SDRs)
            {
                if (sdr.DriverKey.Equals(selectedSDR))
                {
                    listBox1.Items.Clear();
                    listBox1.Items.Add($@"{selectedSDR} Parameters : ");
                    // Debug.WriteLine();
                    // Debug.WriteLine("Antennas:");
                    listBox1.Items.Add("Antennas:");

                    foreach (var antenna in sdr.ListAntennas(Direction.Rx, 0))
                        // Debug.WriteLine(" * " + antenna);
                        listBox1.Items.Add(" * " + antenna);

            
                    // Debug.WriteLine("Gains:");
                    listBox1.Items.Add("Gains:");

                    foreach (var gain in sdr.ListGains(Direction.Rx, 0))
                    {
                        // Debug.WriteLine(" * " + gain);
                        listBox1.Items.Add(" * " + gain + " : " + sdr.GetGain(Direction.Rx, 0));
                        // sdr.SetGain(Direction.Rx, 0, 11);
                        // listBox1.Items.Add(gain + sdr.GetGain(Direction.Rx, 0));
                    }
            
                    // Debug.WriteLine("Frequency ranges:");
                    listBox1.Items.Add("Frequency ranges:");

                    foreach (var freqRange in sdr.GetFrequencyRange(Direction.Rx, 0))
                        // Debug.WriteLine(" * " + freqRange);
                        listBox1.Items.Add(" * " + freqRange);
                    
                    // Debug.WriteLine("SampleRate ranges:");
                    listBox1.Items.Add("SampleRate ranges:");

                    foreach (var sampleRate in sdr.GetSampleRateRange(Direction.Rx, 0))
                    {
                        // Debug.WriteLine(" * " + sampleRate);
                        listBox1.Items.Add(" * " + sampleRate);
                        
                    }
                    listBox1.Items.Add("Current SampleRate : " + (sdr.GetSampleRate(Direction.Rx, 0) / 1_000_000) + "MSPS");
                }
            }
            
        }
    }
}