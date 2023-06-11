using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SoapySDRFFTGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var argsStr = new List<string>();

            // Enumerate devices.
            var results = Pothosware.SoapySDR.Device.Enumerate();
            foreach (var result in results)
            foreach (var tuple in result)
            {
                if (tuple.Key.Equals("driver"))
                {
                    argsStr.Add(tuple.Key.ToLowerInvariant() + "=" + tuple.Value.ToLowerInvariant());
                }
                Console.WriteLine(tuple);
            }
            argsStr.Add("driver=plutosdr,hostname=192.168.2.1");

            foreach (var radio in argsStr)
            {
                // Create device instance.
                // Args can be user-defined or from the enumeration result.
                var args = radio;
                var sdr = new Pothosware.SoapySDR.Device(args);

                // Query device info.
                Console.WriteLine("Antennas:");
                foreach (var antenna in sdr.ListAntennas(Pothosware.SoapySDR.Direction.Rx, 0))
                    Console.WriteLine(" * " + antenna);

                Console.WriteLine("Gains:");
                foreach (var gain in sdr.ListGains(Pothosware.SoapySDR.Direction.Rx, 0))
                    Console.WriteLine(" * " + gain);

                Console.WriteLine("Frequency ranges:");
                foreach (var freqRange in sdr.GetFrequencyRange(Pothosware.SoapySDR.Direction.Rx, 0))
                    Console.WriteLine(" * " + freqRange);
                Console.WriteLine("SampleRate ranges:");
                foreach (var sampleRate in sdr.GetSampleRateRange(Pothosware.SoapySDR.Direction.Rx, 0))
                    Console.WriteLine(" * " + sampleRate);

                // Apply settings.
                //var sampleRate = sdr.GetSampleRate(Pothosware.SoapySDR.Direction.Rx, 0);
                sdr.SetSampleRate(Pothosware.SoapySDR.Direction.Rx, 0, 2.5e06);
                sdr.SetFrequency(Pothosware.SoapySDR.Direction.Rx, 0, 747.5e6);

                // Setup a stream (complex floats).
                var rxStream = sdr.SetupRxStream("CF32" ,new uint[] { 0 }, "");
                rxStream.Activate(); // Start streaming

                // Create a reusable array for RX samples.
                var buff = new float[rxStream.MTU * 2];

                // Receive some samples.
                for (var i = 0; i < 10; ++i)
                {
                    Pothosware.SoapySDR.StreamResult streamResult;

                    var errorCode = rxStream.Read(ref buff, 0, out streamResult);
                    Console.WriteLine("Error code:     " + errorCode);
                    Console.WriteLine("Receive flags:  " + streamResult.Flags);
                    Console.WriteLine("Timestamp (ns): " + streamResult.TimeNs);
                }

                // Shut down the stream.
                rxStream.Deactivate();
                rxStream.Close();
            }
            
        }
    }
}