using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pothosware.SoapySDR;
using SoapySDRFFTGUI.Classes;

namespace SoapySDRFFTGUI
{

    public partial class Form1 : Form
    {
        public bool firstSDR { get; set; }
        public List<Device> SDRs { get; set; } = new List<Device>();

        private Control[] gainTrackBars;
        private Control[] gainTextBoxes;

        private Panel panel1;

        private RxStream[] _rxStreams;

        public Form1()
        {
            InitializeComponent();
            ColorChanger.ChangeControlColors(this);
            EnumerateSDRs();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            // Display the trackbar value in the text box.
            int tag = Convert.ToInt32(((TrackBar)sender).Tag);

            gainTextBoxes[tag].Text = "" + ((TrackBar)sender).Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Query device info.
            var selectedSDR = comboBox1.SelectedItem;
                       
            foreach (var sdr in SDRs)
            {
                if (sdr.DriverKey.Equals(selectedSDR))
                {
                    gainTrackBars = new Control[sdr.ListGains(Direction.Rx, 0).Count];
                    gainTextBoxes = new Control[sdr.ListGains(Direction.Rx, 0).Count];
                    
                    Controls.AddRange(gainTrackBars);
                    listBox1.Items.Clear();
                    listBox1.Items.Add($@" {selectedSDR} Parameters: ");
                    listBox1.Items.Add(" Sample Rate : " + (sdr.GetSampleRate(Direction.Rx, 0) / 1_000_000) +
                                       " MSPS");
                    listBox1.Items.Add(" Antennas:");

                    foreach (var antenna in sdr.ListAntennas(Direction.Rx, 0))
                        listBox1.Items.Add(" * " + antenna);
                    
                    var nativeStreamFormat = sdr.GetNativeStreamFormat(Direction.Rx, 0, out _);
                    listBox1.Items.Add($" Native format: \n * {nativeStreamFormat}");
                    listBox1.Items.Add($"  * {nativeStreamFormat}");
                    
                    var formats = sdr.GetStreamFormats(Direction.Rx, 0);
                    var strBuilder = new StringBuilder();
                    listBox1.Items.Add(" Sample formats:");
                    strBuilder.Append("  * ");
                    foreach (var format in formats) 
                        strBuilder.Append(format + " ");
                    listBox1.Items.Add(strBuilder);
                    
                    // if(streamFormats.Count > 0)
                    //     listBox1.Items.Add("Stream format:");
                    // foreach (var streamFormat in streamFormats)
                    //     listBox1.Items.Add(" * " + streamFormat.Key);
                    
                    var bandwidthRange = sdr.GetBandwidthRange(Direction.Rx, 0);

                    if (bandwidthRange.Count > 0)
                    {
                        strBuilder.Clear();
                        listBox1.Items.Add(" Bandwidths:");
                        strBuilder.Append("  * ");
                    }
    
                    foreach (var bwrange in bandwidthRange)
                        strBuilder.Append(bwrange.Maximum / 1_000_000 + " ");
                    if (bandwidthRange.Count > 0)
                    {
                        strBuilder.Append(" MHz");
                        listBox1.Items.Add(strBuilder);
                    }
                        
                    
                    // sdr.SetGain(Direction.Rx, 0, 7);
                    // sdr.SetGain(Direction.Rx, 1, 8);
                    // sdr.SetGain(Direction.Rx, 2, 9);
                    // if (selectedSDR.Equals("Airspy"))
                    // {
                    //     sdr.SetGainMode(Direction.Rx, 0, true);
                    //     sdr.SetGainMode(Direction.Rx, 1, true);
                    //     sdr.SetGainMode(Direction.Rx, 2, true);
                    // }
                    // sdr.SetGainMode(Direction.Rx, 0, true);
                   

                    listBox1.Items.Add(" Gains:");
                    uint i = 0;
                    foreach (var gain in sdr.ListGains(Direction.Rx, 0))
                    {
                        Range gainRange = sdr.GetGainRange(Direction.Rx, 0);
                        listBox1.Items.Add("  * " + gain + " : " + sdr.GetGain(Direction.Rx, 0, gain));
                        listBox1.Items.Add("  * " + gainRange);
                        // panel1 = new Panel();
                        // // 
                        // // panel1
                        // // 
                        // panel1.Location = new System.Drawing.Point(12, 305);
                        // panel1.Name = "panel1";
                        // panel1.Size = new System.Drawing.Size(419, 48);
                        // panel1.TabIndex = 4;
                        //
                        // var trackBar = new TrackBar();
                        // var textBox = new TextBox();
                        //
                        // textBox.Location = new System.Drawing.Point((int)(10 + i* 48 + 140), panel1.Location.Y + 100);
                        // textBox.Size = new System.Drawing.Size(48, 20);
                        // textBox.Text = gain;
                        // textBox.TabIndex = (int)i + 1 + 4;
                        //
                        //
                        // // Set up the TrackBar.
                        // // Controls.Add(textBox);
                        // gainTextBoxes[i] = (textBox);
                        // // Controls.Add(textBox);
                        // trackBar.Text = gain;
                        // trackBar.Location = new System.Drawing.Point((int)(panel1.Location.X + 10 + i * 140), panel1.Location.Y + 10);
                        // trackBar.Size = new System.Drawing.Size(140, 45);
                        // trackBar.Tag = i;
                        // trackBar.TickStyle = TickStyle.BottomRight;
                        //
                        // trackBar.Scroll += new EventHandler(trackBar1_Scroll);
                        // // The Maximum property sets the value of the track bar when
                        // // the slider is all the way to the right.
                        // trackBar.Maximum = (int)gainRange.Maximum;
                        //
                        // trackBar.Minimum = (int)gainRange.Minimum;
                        //
                        //
                        // // The TickFrequency property establishes how many positions
                        // // are between each tick-mark.
                        // trackBar.TickFrequency = 1;
                        //
                        // // The LargeChange property sets how many positions to move
                        // // if the bar is clicked on either side of the slider.
                        // trackBar.LargeChange = 3;
                        //
                        // // The SmallChange property sets how many positions to move
                        // // if the keyboard arrows are used to move the slider.
                        // trackBar.SmallChange = 1;
                        // // groupBox1.Controls.Add(trackBar1);
                        // gainTrackBars[i] = trackBar;
                        // // Controls.Add(trackBar);
                        //
                        // i++;

                        // sdr.SetGain(Direction.Rx, 0, 11);
                        // listBox1.Items.Add(gain + sdr.GetGain(Direction.Rx, 0));
                    }
                    
                    // Debug.WriteLine("Frequency ranges:");
                    listBox1.Items.Add(" Frequency ranges:");

                    foreach (var freqRange in sdr.GetFrequencyRange(Direction.Rx, 0))
                        // Debug.WriteLine(" * " + freqRange);
                        listBox1.Items.Add("  * " + freqRange);

                    // Debug.WriteLine("SampleRate ranges:");
                    listBox1.Items.Add(" SampleRate ranges:");

                    foreach (var sampleRate in sdr.GetSampleRateRange(Direction.Rx, 0))
                    {
                        // Debug.WriteLine(" * " + sampleRate);
                        listBox1.Items.Add("  * " + sampleRate);

                    }

                    
                    // panel1.Controls.AddRange(gainTextBoxes);
                    // panel1.Controls.AddRange(gainTrackBars);
                    // panel1.Visible = true;
                    // Controls.Add(panel1);

                }
            }

        }

        private void EnumerateSDRs()
        {
            var argsStr = new List<string>();
            var sdrList = new List<string>();

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

            if (!firstSDR)
                argsStr.Add("driver=plutosdr,hostname=192.168.2.1");

            foreach (var radio in argsStr)
            {
                // Create device instance.
                // Args can be user-defined or from the enumeration result.
                var args = radio;
                Device sdr = new Device(args);
                var sampleRate = sdr.DriverKey.Equals("RTLSDR") ? 2.4e06 : 10e06;
                var qo100CenterFreq = 747.5e6;
                sdr.SetSampleRate(Direction.Rx, 0, sampleRate);
                sdr.SetFrequency(Direction.Rx, 0, qo100CenterFreq);
                SDRs.Add(sdr);
                
                comboBox1.Items.Add(sdr.DriverKey);
                if (!firstSDR)
                {
                    firstSDR = true;
                    comboBox1.SelectedItem = sdr.DriverKey;
                }
                sdrList.Add(sdr.DriverKey);
            }

            listBox1.Items.Clear();
            listBox1.Items.Add($"Found {sdrList.Count} radios : ");
            foreach (string sdr in sdrList)
            {
                listBox1.Items.Add(" * " + sdr);
            }

            _rxStreams = new RxStream[SDRs.Count];
        }

        private void RxStream()
        {
            var selectedSDR = comboBox1.SelectedItem;
            var sdrNumber = 0;
            foreach (var sdr in SDRs)
            {
                if (sdr.DriverKey.Equals(selectedSDR))
                {
                    // Apply settings.
                    // var sampleRate = sdr.GetSampleRate(Direction.Rx, 0);
                    var sampleRate = sdr.DriverKey.Equals("RTLSDR") ? 2.4e06 : 10e06;
                    var qo100CenterFreq = 745.5e6;
                    var m = 1_000_000;
                    // sdr.SetSampleRate(Direction.Rx, 0, sampleRate);
                    // sdr.SetFrequency(Direction.Rx, 0, qo100CenterFreq);
                    // Setup a stream (complex floats).
                    uint[] channels = { 0 };
                    RxStream rxStream = null;
                    if (_rxStreams[sdrNumber] is null)
                    {
                        rxStream = sdr.SetupRxStream("CS16" ,channels, "");
                        rxStream.Activate(); // Start streaming
                        _rxStreams[sdrNumber] = rxStream;
                    }

                    if (_rxStreams[sdrNumber] is not null)
                    {
                        rxStream = _rxStreams[sdrNumber];
                    }
                    // Create a reusable array for RX samples.
                    var buff = new short[rxStream.MTU * 2];
                   
                    // Receive some samples.
                    listBox1.Items.Clear();
                    var n = 15;
                    listBox1.Items.Add($"Reading {n} samples, at {sampleRate/m} MSPS at {qo100CenterFreq/m} MHz");
                    for (var i = 0; i < n; ++i)
                    {
                        StreamResult streamResult;
            
                        var errorCode = rxStream.Read(ref buff, 5, out streamResult);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
                        // listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Receive flags:  " + streamResult.Flags);
                        // listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Timestamp (ns): " + streamResult.TimeNs);
                    }
            
                    
                }
                sdrNumber++;
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            RxStream();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Restore_Window_Location();
        }

        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DeactivateActiveRxStreams();

            Save_Window_Location();
        }

        private void DeactivateActiveRxStreams()
        {
            for (var index = 0; index < _rxStreams.Length; index++)
            {
                if (_rxStreams[index] is not null)
                {
                    _rxStreams[index].Deactivate();
                    _rxStreams[index].Close();
                }
            }
        }

        private void Save_Window_Location()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximised = true;
                Properties.Settings.Default.Minimised = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Size = Size;
                Properties.Settings.Default.Maximised = false;
                Properties.Settings.Default.Minimised = false;
            }
            else
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximised = false;
                Properties.Settings.Default.Minimised = true;
            }

            Properties.Settings.Default.Save();
        }
        
        private void Restore_Window_Location()
        {
            if (Properties.Settings.Default.Maximised)
            {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Maximized;
                Size = Properties.Settings.Default.Size;
            }
            else if (Properties.Settings.Default.Minimised)
            {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Minimized;
                Size = Properties.Settings.Default.Size;
            }
            else
            {
                Location = Properties.Settings.Default.Location;
                Size = Properties.Settings.Default.Size;
            }
        }

    }
}