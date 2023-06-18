using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FFTW.NET;
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

        private static uint FFT_SIZE = 1024;

        private FftwArrayComplex _fftwArrayIn;
        private FftwArrayComplex _fftwArrayOut;

        private double[] hanning_window_const = new double[FFT_SIZE];


        public Form1()
        {
            SetupHanningWindowArray();
            InitializeComponent();
            ColorChanger.ChangeControlColors(this);
            EnumerateSDRs();
        }

        private void SetupHanningWindowArray()
        {
            for (var i = 0; i < FFT_SIZE; i++)
            {
                hanning_window_const[i] = 0.5 * (1.0 - Math.Cos(2 * Math.PI * (((double)i) / FFT_SIZE)));
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            // Display the trackbar value in the text box.
            int tag = Convert.ToInt32(((TrackBar)sender).Tag);

            gainTextBoxes[tag].Text = "" + ((TrackBar)sender).Value;
        }

        private void QuerySdrButton1Click(object sender, EventArgs e)
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
                    listBox1.Items.Add($" Native format:");
                    listBox1.Items.Add($"  * {nativeStreamFormat}");

                    var formats = sdr.GetStreamFormats(Direction.Rx, 0);
                    var strBuilder = new StringBuilder();
                    listBox1.Items.Add(" Sample formats:");
                    strBuilder.Append("  * ");
                    foreach (var format in formats) strBuilder.Append(format + " ");

                    listBox1.Items.Add(strBuilder);

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

                    listBox1.Items.Add(" Gains:");
                    uint i = 0;
                    foreach (var gain in sdr.ListGains(Direction.Rx, 0))
                    {
                        if (!sdr.DriverKey.Equals("Airspy"))
                        {
                            Range gainRange = sdr.GetGainRange(Direction.Rx, i);
                            listBox1.Items.Add("  * " + gain + " : " + sdr.GetGain(Direction.Rx, 0, gain));
                            listBox1.Items.Add("  * " + gainRange);
                        }
                        if (sdr.DriverKey.Equals("Airspy"))
                        {
                            Range gainRange = sdr.GetGainRange(Direction.Rx, i, gain);
                            listBox1.Items.Add("  * " + gain + " : " + sdr.GetGain(Direction.Rx, 0, gain));
                            listBox1.Items.Add("  * " + gainRange);
                        }
                        
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
                        i++;

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
            }
            // TODO: should be network address input box, with search for button... 
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
                if (sdr.DriverKey.Equals("Airspy"))
                {
                    // var val = sdr.GetGainRange(Direction.Rx, 0).Maximum * 0.7;
                    sdr.SetGain(Direction.Rx, 0, "LNA", 9);
                    sdr.SetGain(Direction.Rx, 0, "MIX", 9);
                    sdr.SetGain(Direction.Rx, 0, "VGA", 9);
                }

                if (!sdr.DriverKey.Equals("Airspy"))
                {
                    var range = sdr.GetGainRange(Direction.Rx, 0);
                    sdr.SetGain(Direction.Rx, 0, range.Maximum * 0.75);
                }

                SDRs.Add(sdr);

                comboBox1.Items.Add(sdr.DriverKey);
                // sets stream format of first detected sdr 
                if (!firstSDR)
                {
                    firstSDR = true;
                    comboBox1.SelectedItem = sdr.DriverKey;
                    var nativeStreamFormat = sdr.GetNativeStreamFormat(Direction.Rx, 0, out _);

                    var formats = sdr.GetStreamFormats(Direction.Rx, 0);
                    foreach (var format in formats) comboBox2.Items.Add(format);
                    comboBox2.SelectedItem = nativeStreamFormat;
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
                    var sampleRate = sdr.DriverKey.Equals("RTLSDR") ? 2.4e06 : 10e06;
                    var qo100CenterFreq = 745.5e6;
                    var m = 1_000_000;

                    var format = comboBox2.SelectedItem.ToString();
                    var formatSize = StreamFormat.FormatToSize(format);
                    
                    // Create a reusable arrays for RX samples.
                    float[] floatBuffer;
                    short[] shortBuffer;
                    
                    // Setup a stream (complex floats).
                    uint[] channels = { 0 };
                    if (_rxStreams[sdrNumber] is null)
                    {
                        _rxStreams[sdrNumber] = sdr.SetupRxStream(format, channels, "bitpack=true");
                        _rxStreams[sdrNumber].Activate(); // Start streaming
                    }

                    var MTU = _rxStreams[sdrNumber].MTU;
                    StreamResult streamResult;
                    
                    // Receive some samples.
                    listBox1.Items.Clear();
                    uint numberOfSamples = 1;
                    listBox1.Items.Add($"Reading {numberOfSamples} samples, at {sampleRate / m} MSPS at {qo100CenterFreq / m} MHz");
                    listBox1.Items.Add($"Format {format} -> {formatSize} bytes * {MTU} bytes (MTU)");
                    if (format.Equals(StreamFormat.ComplexInt16))
                    {
                        // if (_rxStreams[sdrNumber].Format.Equals(StreamFormat.ComplexFloat32))
                        // {
                        //     // Disable 16bit stream and start a 32bit one
                        //     _rxStreams[sdrNumber].Deactivate();
                        //     _rxStreams[sdrNumber] = sdr.SetupRxStream(format, channels, "bitpack=true");
                        //     _rxStreams[sdrNumber].Activate(); 
                        // }
                        shortBuffer = new short[MTU * formatSize];
                        var errorCode = _rxStreams[sdrNumber].Read(ref shortBuffer, 100000, out streamResult);
                        if (selectedSDR.Equals("Airspy") && errorCode.Equals(ErrorCode.Overflow))
                            errorCode = _rxStreams[sdrNumber].Read(ref shortBuffer, 100000, out streamResult);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
                        comboBox2.Enabled = false;

                        if (!errorCode.Equals(ErrorCode.Overflow))
                            ExampleUsePlanDirectly(new float[0], shortBuffer, _rxStreams[sdrNumber], numberOfSamples);
                        streamResult.Dispose();
                    }
                    if (format.Equals(StreamFormat.ComplexFloat32))
                    {
                        // if (_rxStreams[sdrNumber].Format.Equals(StreamFormat.ComplexInt16))
                        // {
                        //     // Disable 16bit stream and start a 32bit one
                        //     _rxStreams[sdrNumber].Deactivate();
                        //     _rxStreams[sdrNumber] = sdr.SetupRxStream(format, channels, "bitpack=true");
                        //     _rxStreams[sdrNumber].Activate(); 
                        // }
                        floatBuffer = new float[MTU * formatSize];
                        var errorCode = _rxStreams[sdrNumber].Read(ref floatBuffer, 100000, out streamResult);
                        if (selectedSDR.Equals("Airspy") && errorCode.Equals(ErrorCode.Overflow))
                            errorCode = _rxStreams[sdrNumber].Read(ref floatBuffer, 100000, out streamResult);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
                        comboBox2.Enabled = false;

                        if (!errorCode.Equals(ErrorCode.Overflow))
                            ExampleUsePlanDirectly(floatBuffer, new short[0], _rxStreams[sdrNumber], numberOfSamples, true);
                        streamResult.Dispose();
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

        private void ExampleUsePlanDirectly(float[] floatBuffer,short[] shortBuffer, RxStream rxStream, decimal numSamps, bool floats = false)
        {
            var selectedSDR = comboBox1.SelectedItem;
            var sampleRate = 0d;

            foreach (var sdr in SDRs)
                if (sdr.DriverKey.Equals(selectedSDR))
                    sampleRate = sdr.GetSampleRate(Direction.Rx, 0);

            // Use the same arrays for as many transformations as you like.
            // If you can use the same arrays for your transformations, this is faster than calling DFT.FFT / DFT.IFFT
            // TODO: This is a combo of rtl_map and eshail-ghy-wb-fft-airspy from BATC, regarding data transforms.
            //     : this might be inaccurate 
            _fftwArrayIn = new FftwArrayComplex((int)(FFT_SIZE * numSamps));
            _fftwArrayOut = new FftwArrayComplex((int)(FFT_SIZE * numSamps));
            FftwArrayComplex pt = new FftwArrayComplex((int)(FFT_SIZE * numSamps));
            float FFT_TIME_SMOOTH = 0.9995f;
            double pwr_scale = 1.0 / (float)(FFT_SIZE * FFT_SIZE);
            double pwr, lpwr;
            double[] fft_buffer = new double[FFT_SIZE];
            int cnt = 0;
            double amp, db;
            // float amp, db;
            int out_r;
            int out_i;
            var strb = new StringBuilder();
            var chunks = sampleRate / FFT_SIZE;
            while (cnt < (int)(rxStream.MTU / FFT_SIZE))
            {
                int offset;
                offset = (int)((cnt * FFT_SIZE * 2) / 2);
                //
                /* Copy data out of rf buffer into fft_input buffer */
                for (var i = 0; i < FFT_SIZE; i++)
                {
                    // Real ?
                    if(floats) _fftwArrayIn[i] = floatBuffer[offset + (2 * i)] * hanning_window_const[i];
                    if(!floats) _fftwArrayIn[i] = shortBuffer[offset + (2 * i)] * hanning_window_const[i];
                    // Imaginary ?
                    // _fftwArrayIn[i][1] = buffer[offset+(2*i)+1] * hanning_window_const[i];
                }

                cnt++;
                // var n = 0;
                // for (int i=0; i<numSamps; i+=2){
                //     _fftwArrayIn[i] = (buffer[n]-127.34) + (buffer[n+1]-127.34) * _fftwArrayIn[i].Imaginary;
                //     n++;
                // }


                var fft = FftwPlanC2C.Create(_fftwArrayIn, _fftwArrayOut, DftDirection.Forwards);
                fft.Execute();

                // for (var i = 0; i < FFT_SIZE; i++)
                // {
                //     /* shift, normalize and convert to dBFS */
                //     if (i < FFT_SIZE / 2)
                //     {
                //         pt[0] = _fftwArrayOut[(int)(FFT_SIZE / 2 + i)] / FFT_SIZE;
                //         // pt[1] = _fftwArrayOut[FFT_SIZE / 2 + i] / FFT_SIZE;
                //     }
                //     else
                //     {
                //         pt[0] = _fftwArrayOut[(int)(i - FFT_SIZE / 2)] / FFT_SIZE;
                //         // pt[1] = _fftwArrayOut[i - FFT_SIZE / 2] / FFT_SIZE;
                //     }
                //
                //     pwr = pwr_scale * (pt[0].Real * pt[0].Real) + (pt[1].Imaginary * pt[1].Imaginary);
                //     float Ten = 10f;
                //     float one = 1f;
                //     lpwr = Ten * Math.Log10(pwr + 1.0e-20);
                //
                //     fft_buffer[i] = (lpwr * (one - FFT_TIME_SMOOTH)) + (fft_buffer[i] * FFT_TIME_SMOOTH);
                // }


                // foreach (var VARIABLE in fft_buffer)
                // {
                //     strb.Append($" - {VARIABLE}");
                // }


                for (int i = 0; i < FFT_SIZE; i++)
                {
                    // out_r = (int)(_fftwArrayOut[i].Real * _fftwArrayOut[i].Real);
                    // out_i = (int)(_fftwArrayOut[i].Imaginary * _fftwArrayOut[i].Imaginary);
                    // amp = (float)Math.Sqrt(out_r + out_i);
                    // db = (float)(10 * Math.Log10(amp));
                    amp = _fftwArrayOut[i].Phase;
                    db = _fftwArrayOut[i].Magnitude;
                    if(cnt == 62)
                        strb.Append($"[{db}|{amp}], ");
                }
            }

            var buffLen = floats ? floatBuffer.Length : shortBuffer.Length;
            listBox1.Items.Add($" Each FFT point is {chunks/1_000f} kHz wide ");
            listBox1.Items.Add($" Chunks of {FFT_SIZE} FFT:  {cnt}");
            listBox1.Items.Add($" Buffer length:  {buffLen}");
            Logger.Log(LogLevel.Info, $"{strb}");
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DeactivateActiveRxStreams();

            Save_Window_Location();
        }

        private void DeactivateActiveRxStreams()
        {
            foreach (var rxStream in _rxStreams)
            {
                if (rxStream is not null && rxStream.Active)
                {
                    Console.WriteLine(rxStream.ToString());
                    rxStream.Deactivate();
                    rxStream.Close();
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

        private void comboBox2_Click(object sender, EventArgs e)
        {
            var selectedSDR = comboBox1.SelectedItem;
            var sdrNumber = 0;
            foreach (var sdr in SDRs)
            {
                if (sdr.DriverKey.Equals(selectedSDR))
                {
                    // TODO: do something here ... 
                    // sdr.SetupRxStream();
                    
                }
            }
        }
    }
}