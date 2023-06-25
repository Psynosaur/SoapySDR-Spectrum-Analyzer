using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FFTW.NET;
using Pothosware.SoapySDR;
using ScottPlot;
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
        private double qo100CenterFreq = 745.5e6;

        private double[] radioValues;

        private double[] FftValues;

        private int DrawCnt = 0;

        Graphics tmp;
        Pen greyPen = new Pen(Color.Green);
        Pen greyPen2 = new Pen(Color.FromArgb(200, 123, 123, 123));
        Pen redPen = new Pen(Color.Red);
        static Bitmap bmp;
        SolidBrush tuner1Brush = new(Color.FromArgb(50, Color.Blue));

        public Form1()
        {
            SetupHanningWindowArray();
            InitializeComponent();
            ColorChanger.ChangeControlColors(this);
            EnumerateSDRs();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            tmp = Graphics.FromImage(bmp);

            formsPlot1.Plot.YLabel("Spectral Power dB");
            formsPlot1.Plot.XLabel("Frequency (MHz)");
            formsPlot1.Plot.Title($"QO-100 Spectrum");
            formsPlot1.Plot.SetAxisLimits(740.5, 750.5, -30, 30);
            formsPlot1.Refresh();
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
            // if (!firstSDR)
            //     argsStr.Add("driver=plutosdr,hostname=192.168.2.1");

            foreach (var radio in argsStr)
            {
                // Create device instance.
                // Args can be user-defined or from the enumeration result.
                var args = radio;
                Device sdr = new Device(args);
                var sampleRate = sdr.DriverKey.Equals("RTLSDR") ? 2.4e06 : 10e06;
                //if (sdr.DriverKey.Equals("RTLSDR")) args = "offset_tune=true";
                // if (sdr.DriverKey.Equals("Airspy")) sdr.WriteSetting("bitpack", "true");
                //
                // if (sdr.DriverKey.Equals("RTLSDR")) sdr.WriteSetting("offset_tune", "true");
                //if (sdr.DriverKey.Equals("Airspy")) args = "bitpack=true"; 
                sdr.SetSampleRate(Direction.Rx, 0, sampleRate);
                sdr.SetFrequency(Direction.Rx, 0, qo100CenterFreq);
                if (sdr.DriverKey.Equals("Airspy"))
                {
                    // var val = sdr.GetGainRange(Direction.Rx, 0).Maximum * 0.7;
                    sdr.SetGain(Direction.Rx, 0, "LNA", 11);
                    sdr.SetGain(Direction.Rx, 0, "MIX", 11);
                    sdr.SetGain(Direction.Rx, 0, "VGA", 11);
                }

                if (!sdr.DriverKey.Equals("Airspy"))
                {
                    var range = sdr.GetGainRange(Direction.Rx, 0);
                    sdr.SetGain(Direction.Rx, 0, range.Maximum * 0.75);
                }

                SDRs.Add(sdr);

                comboBox1.Items.Add(sdr.DriverKey);
                // sets native stream format of first detected sdr 
                if (!firstSDR)
                {
                    firstSDR = true;
                    comboBox1.SelectedItem = sdr.DriverKey;
                    comboBox2.SelectedItem = sdr.GetNativeStreamFormat(Direction.Rx, 0, out _);
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
                    var args = "";
                    //if (sdr.DriverKey.Equals("RTLSDR")) args = "offset_tune=true";
                    //if (sdr.DriverKey.Equals("RTLSDR")) sdr.WriteSetting("offset_tune", "true");
                    //if (sdr.DriverKey.Equals("Airspy")) args = "bitpack=true"; 
                    //if (sdr.DriverKey.Equals("Airspy")) sdr.WriteSetting("bitpack", "true");

                    var m = 1_000_000;

                    var format = comboBox2.SelectedItem.ToString();
                    var formatSize = StreamFormat.FormatToSize(format);
                    if (!format.Equals(StreamFormat.ComplexInt16) && !format.Equals(StreamFormat.ComplexFloat32))
                        return;
                    // Create a reusable arrays for RX samples.
                    float[] floatBuffer;
                    short[] shortBuffer;

                    // Setup a stream (complex floats).
                    uint[] channels = { 0 };
                    if (_rxStreams[sdrNumber] is null)
                    {
                        _rxStreams[sdrNumber] = sdr.SetupRxStream(format, channels, args);
                        _rxStreams[sdrNumber].Activate(); // Start streaming
                    }

                    var MTU = _rxStreams[sdrNumber].MTU;
                    StreamResult streamResult;

                    // Receive some samples.
                    listBox1.Items.Clear();
                    listBox1.Items.Add($"setting{sdr.ReadSetting("bitpack")} ");
                    uint numberOfSamples = 1;
                    listBox1.Items.Add(
                        $"Reading {numberOfSamples} samples, at {sampleRate / m} MSPS at {numericUpDown1.Value / m} MHz");
                    listBox1.Items.Add($"Format {format} -> {formatSize} bytes * {MTU} bytes (MTU)");
                    if (format.Equals(StreamFormat.ComplexInt16))
                    {
                        shortBuffer = new short[MTU * 2];
                        var errorCode = _rxStreams[sdrNumber].Read(ref shortBuffer, 100000, out streamResult);
                        while (errorCode.Equals(ErrorCode.Overflow))
                            errorCode = _rxStreams[sdrNumber].Read(ref shortBuffer, 100000, out streamResult);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | StreamResult size:     " +
                                           streamResult.NumSamples);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Short buffer size:     " +
                                           shortBuffer.Length);

                        comboBox2.Enabled = false;
                        Complex[] transformedValues = new Complex[shortBuffer.Length];
                        double largestMagnitude = 0; //Stores the largest magnitude that has been found 
                        int biggestMagIndex = 0;
                        if (errorCode.Equals(ErrorCode.None))
                        {
                            Span<byte> byteSpan = MemoryMarshal.Cast<short, byte>(new Span<short>(shortBuffer));
                            anotherFFT(byteSpan.ToArray(), null, format);
                            //Data_To_FFT(new float[0], shortBuffer, sdr, _rxStreams[sdrNumber], numberOfSamples);
                            //transformedValues = fft(shortBuffer);
                            //TheBestFFTAlgo(shortBuffer);

                            //drawTimeGraph(shortBuffer);
                            //drawMagnitudeGraph(transformedValues, biggestMagIndex);
                        }


                        streamResult.Dispose();
                    }

                    if (format.Equals(StreamFormat.ComplexFloat32))
                    {
                        floatBuffer = new float[MTU * 2];
                        var errorCode = _rxStreams[sdrNumber].Read(ref floatBuffer, 100000, out streamResult);
                        for (int i = 0; i < 10; i++)
                        {
                            errorCode = _rxStreams[sdrNumber].Read(ref floatBuffer, 100000, out streamResult);
                            if (errorCode.Equals(ErrorCode.None)) anotherFFT(new byte[0], floatBuffer, format);
                        }

                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | StreamResult size:     " +
                                           streamResult.NumSamples);
                        listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Float buffer size:     " +
                                           floatBuffer.Length);
                        comboBox2.Enabled = false;


                        streamResult.Dispose();
                    }

                    Logger.Log(LogLevel.Info, $"{_rxStreams[sdrNumber].ToString()}");
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

        void anotherFFT(byte[] buffer, float[] floatBuf = null, string format = "")
        {
            var selectedSdr = comboBox1.SelectedItem;
            double sampleRate = 0;
            foreach (var sdr in SDRs)
                if (sdr.DriverKey.Equals(selectedSdr))
                    sampleRate = sdr.GetSampleRate(Direction.Rx, 0);

            int bytesPerSample = format.Equals("CF32") ? 1 : 2;

            var chunkInHz = sampleRate / FFT_SIZE;
            var chunkInKHz = sampleRate / FFT_SIZE / 1_000f;

            radioValues = new double[buffer.Length / bytesPerSample];
            int bufferSampleCount;

            if (floatBuf is not null)
            {
                bufferSampleCount = floatBuf.Length / bytesPerSample;
                radioValues = new double[floatBuf.Length / bytesPerSample];
                for (int i = 0; i < bufferSampleCount - 1; i++)
                {
                    radioValues[i] = BitConverter.ToUInt32(BitConverter.GetBytes(floatBuf[i * bytesPerSample]), 0);
                }
            }

            if (buffer.Length > 0)
            {
                bufferSampleCount = buffer.Length / bytesPerSample;
                for (int i = 0; i < bufferSampleCount - 1; i++)
                {
                    radioValues[i] = BitConverter.ToUInt16(buffer, i * bytesPerSample);
                    ;
                }
            }


            double[] paddedSignal = FftSharp.Pad.ZeroPad(radioValues);
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(paddedSignal);
            var freqScale = FftSharp.FFT.FrequencyResolution(spectrum.Length, bytesPerSample);
            double[] fftMag = FftSharp.FFT.Magnitude(spectrum);
            double[] filtered = FftSharp.Filter.LowPass(fftMag.Skip(1).ToArray(), sampleRate, maxFrequency: 48000);
            //double[] freq = FftSharp.FFT.FrequencyScale(fftMag.Length, 5_000_000);
            //double[] fftMag = FftSharp.Transform.FFTpower(paddedAudio);
            FftValues = new double[fftMag.Length - 1];
            var window = new FftSharp.Windows.Blackman();
            double[] windowed = window.Apply(fftMag);
            //System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(radioValues);
            //double[] psd = FftSharp.FFT.Power(spectrum);
            //double[] freq = FftSharp.FFT.FrequencyScale(psd.Length, 5_000_000);
            //formsPlot1.Plot.AddScatterLines(freq, psd);
            var tempArray = filtered.Skip(1).ToArray();

            Array.Copy(tempArray, FftValues, tempArray.Length);

            // find the frequency peak
            //int peakIndex = 0;
            //for (int i = 0; i < fftMag.Length; i++)
            //{
            //    if (fftMag[i] > fftMag[peakIndex])
            //        peakIndex = i;
            //}
            //double fftPeriod = FftSharp.Transform.FFTfreqPeriod(10, fftMag.Length);
            //double peakFrequency = fftPeriod * peakIndex;
            //label3.Text = $"Peak Frequency: {peakFrequency:N0} Hz";

            // request a redraw using a non-blocking render queue
            for (int i = 0; i < 10; i++)
            {
            }


            if (DrawCnt < 30) DrawCnt += 1;
            if (DrawCnt == 30)
            {
                DrawCnt = 0;
                formsPlot1.Plot.Clear();
            }

            var buffLen = FftValues.Length;
            var centerFreq = (double)numericUpDown1.Value;
            listBox1.Items.Add($" Centre Freq {centerFreq} Hz ");
            var startFreq = ((double)numericUpDown1.Value / 1000000f) - 5; // Convert to MHz
            var stopFreq =
                startFreq + (FFT_SIZE * chunkInHz) /
                1000000f; // Calculate the stop frequency based on the start frequency and FFT size
            listBox1.Items.Add($" Each FFT point is {chunkInKHz} kHz wide ");
            listBox1.Items.Add($" Chunks of {FFT_SIZE} FFT:  {1}");
            listBox1.Items.Add($" Buffer length:  {buffLen}");
            listBox1.Items.Add($" Start Frequency :  {startFreq} MHz");
            listBox1.Items.Add($" Stop Frequency :  {stopFreq} MHz");

            var highLow = GetMinMax(FftValues);
            // formsPlot1.Plot.SetAxisLimits(startFreq, stopFreq, highLow.Max - 15, highLow.Max + 3);
            formsPlot1.Plot.SetAxisLimits(startFreq, stopFreq, highLow.Min, highLow.Max + 150);
            var arrangeArr = new double[FftValues.Length];
            int j = 0;
            for (int i = FftValues.Length / 2; i > 0; i--)
            {
                arrangeArr[j] = FftValues[i];
                j++;
            }

            for (int i = FftValues.Length - 1; i > FftValues.Length / 2; i--)
            {
                arrangeArr[j] = FftValues[i];
                j++;
            }

            var pew = sampleRate / 1_000_000f / FftValues.Length;
            double[] xValues = Enumerable.Range(0, FftValues.Length).Select(i => startFreq + (i * pew)).ToArray();
            formsPlot1.Plot.AddSignalXY(xValues, arrangeArr);
            // formsPlot1.Plot.AddSignal(FftValues, (double)numericUpDown3.Value);
            // formsPlot1.Plot.XAxis.DateTimeFormat(true);
            formsPlot1.RefreshRequest();
        }

        public class MinMax
        {
            public double Min;
            public double Max;

            public MinMax(double min, double max)
            {
                Min = min;
                Max = max;
            }
        }

        public MinMax GetMinMax(double[] sourceArray)
        {
            var minMax = new MinMax(sourceArray[0], sourceArray[0]);
            for (int index = 1; index < sourceArray.Length; index++)
            {
                if (sourceArray[index] > minMax.Max)
                    minMax.Max = sourceArray[index];
                if (sourceArray[index] < minMax.Min)
                {
                    minMax.Min = sourceArray[index];
                }
            }

            return minMax;
        }

        public double GetSmallestElement(double[] sourceArray)
        {
            double maxElement = sourceArray[0];
            for (int index = 1; index < sourceArray.Length; index++)
            {
                if (sourceArray[index] > maxElement)
                    maxElement = sourceArray[index];
            }

            return maxElement;
        }


        private void SetupHanningWindowArray()
        {
            for (var i = 0; i < FFT_SIZE; i++)
            {
                hanning_window_const[i] = 0.5 * (1.0 - Math.Cos(2 * Math.PI * (((double)i) / FFT_SIZE)));
            }
        }


        private Complex[] fft(short[] inputArr)
        {
            Complex[]
                inputComp = new Complex[inputArr
                    .Length]; //Array to store the complex numbers that will be inputted to the FFT
            Complex[]
                outputComp =
                    new Complex[inputComp
                        .Length]; //Array to store the complex numbers that will be outputted from the FFT

            //Loop through all of the items in the array and save them to the complex input array with 0 imaginary part
            for (int i = 0; i < inputArr.Length; i++)
            {
                inputComp[i] = inputArr[i];
            }


            //The libary used for the Fast Fourier Transformation requires pinned arrays. Using "Using" means that the memory can be returned to the heap once it's done
            using (var pinnedInput = new PinnedArray<Complex>(inputComp))
            using (var pinnedOutput = new PinnedArray<Complex>(outputComp))
            {
                DFT.FFT(pinnedInput,
                    pinnedOutput); //Perform the actual transformation using pinnedInput as the input and saving the output to pinnedOutput
            }

            //Return the complex array from the transformation
            return outputComp;
        }

        void drawMagnitudeGraph(Complex[] output, int biggestMag)
        {
            pictureBox1.Image = null;
            Pen plotter = new Pen(Brushes.Gainsboro);
            for (int i = 0; i < output.Length / 2; i++)
            {
                using (Graphics g = pictureBox1.CreateGraphics())
                {
                    g.DrawEllipse(plotter, i / 2,
                        500 - Convert.ToSingle(output[i].Magnitude) * (500 / Convert.ToSingle(biggestMag)), 4, 4);
                }
            }
        }

        void drawTimeGraph(short[] inBuffer)
        {
            pictureBox1.Image = null;
            Graphics g = pictureBox1.CreateGraphics();

            Pen plotter = new Pen(Brushes.Gainsboro);

            for (int i = 0; i < inBuffer.Length; i++)
            {
                g.DrawEllipse(plotter, i, 255 - (Convert.ToInt32(inBuffer[i]) / 128), 5, 5);
            }


            // pictureBox1.Image = g.DrawImage(new Rectangle(0, 0, pictureBox1.Width,pictureBox1.Height));
        }

        void drawBuff(short[] inAudio)
        {
            using (Bitmap drawnPic = new Bitmap(pictureBox1.Width, pictureBox1.Height))
            {
                //Draw the pixel black
                for (int i = 0; i < inAudio.Length; i++)
                {
                    short val = inAudio[i];
                    //drawnPic.SetPixel((i/(inAudio.Length/drawnPic.Width)), (drawnPic.Height / 2) - (inAudio[i]/(65550 / (drawnPic.Height)))-1, Color.Black);
                    drawnPic.SetPixel((i / (inAudio.Length / drawnPic.Width)),
                        (drawnPic.Height / 2) - (inAudio[i] >> 8) * 2 - 1, Color.Black);
                }

                drawnPic.SetPixel(512, 256, Color.Black);
                pictureBox1.Image = drawnPic.Clone(new Rectangle(0, 0, drawnPic.Width, drawnPic.Height),
                    System.Drawing.Imaging.PixelFormat.DontCare);
            }
        }

        void drawTimeGraphDouble(double[] inBuffer)
        {
            pictureBox1.Image = null;
            Graphics g = pictureBox1.CreateGraphics();

            Pen plotter = new Pen(Brushes.Gainsboro);

            for (int i = 0; i < inBuffer.Length; i++)
            {
                g.DrawEllipse(plotter, i, 255 - (Convert.ToInt32(inBuffer[i]) / 128), 2, 2);
            }
        }


        private void Data_To_FFT(float[] floatBuffer, short[] shortBuffer, Device sdr, RxStream rxStream,
            decimal numSamps, bool floats = false)
        {
            var sampleRate = sdr.GetSampleRate(Direction.Rx, 0);
            var centerFreq = sdr.GetFrequency(Direction.Rx, 0);
            var halfSampleRate = sampleRate / 2;
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

            int cnt = 0;
            double amp, db;
            // float amp, db;
            int out_r;
            int out_i;
            var strb = new StringBuilder();
            var chunkInHz = sampleRate / FFT_SIZE;
            var chunkInKHz = sampleRate / FFT_SIZE / 1_000f;
            var magnitudeArray = new double[FFT_SIZE];
            var bufferSize = floats ? floatBuffer.Length : shortBuffer.Length;
            float[] fft_buffer = new float[bufferSize / 2];
            while (cnt < (int)(bufferSize / 2 / FFT_SIZE / 2) - 1)
            {
                int offset;
                offset = (int)((cnt * FFT_SIZE * 2) / 2);
                //
                /* Copy data out of rf buffer into fft_input buffer */
                for (var i = 0; i < FFT_SIZE; i++)
                {
                    // Real ?
                    if (floats) _fftwArrayIn[i] = floatBuffer[offset + (2 * i)] * hanning_window_const[i];
                    // if (floats) _fftwArrayIn[i] = floatBuffer[offset + (2 * i)];
                    if (!floats) _fftwArrayIn[i] = shortBuffer[offset + (2 * i)] * hanning_window_const[i];
                    // if (!floats) _fftwArrayIn[i] = shortBuffer[offset + (2 * i)];
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
                //for (int i = _fftwArrayOut.Length / 2; i < _fftwArrayOut.Length; i++)
                //    _fftwArrayOut[i] = 0;

                for (var i = 0; i < FFT_SIZE; i++)
                {
                    /* shift, normalize and convert to dBFS */
                    if (i < FFT_SIZE / 2)
                    {
                        pt[0] = _fftwArrayOut[(int)(FFT_SIZE / 2 + i)] / FFT_SIZE;
                        // pt[1] = _fftwArrayOut[FFT_SIZE / 2 + i] / FFT_SIZE;
                    }
                    else
                    {
                        pt[0] = _fftwArrayOut[(int)(i - FFT_SIZE / 2)] / FFT_SIZE;
                        // pt[1] = _fftwArrayOut[i - FFT_SIZE / 2] / FFT_SIZE;
                    }

                    pwr = pwr_scale * (pt[0].Real * pt[0].Real) + (pt[1].Imaginary * pt[1].Imaginary);
                    lpwr = 10f * Math.Log10(pwr + 1.0e-20);

                    fft_buffer[i * cnt] =
                        (float)((lpwr * (1f - FFT_TIME_SMOOTH)) + (fft_buffer[i * cnt] * FFT_TIME_SMOOTH));
                }


                // foreach (var VARIABLE in fft_buffer)
                // {
                //     strb.Append($" - {VARIABLE}");
                // }
                // var populated = false;
                // for (int i = 0; i < FFT_SIZE; i++)
                // {
                //     out_r = (int)(_fftwArrayOut[i].Real * _fftwArrayOut[i].Real);
                //     out_i = (int)(_fftwArrayOut[i].Imaginary * _fftwArrayOut[i].Imaginary);
                //     amp = (float)Math.Sqrt(out_r + out_i);
                //     db = (float)(10 * Math.Log10(amp));
                //     // amp = _fftwArrayOut[i].Phase;
                //     // db = _fftwArrayOut[i].Magnitude;
                //     if (cnt == 62)
                //     {
                //         strb.Append($"[{db}|{amp}], ");
                //         if (!populated) magnitudeArray[i] = _fftwArrayOut[i].Magnitude;
                //         if (i == 1023) populated = true;
                //     }
                // }
            }

            var buffLen = floats ? floatBuffer.Length : shortBuffer.Length;
            listBox1.Items.Add($" Centre Freq {centerFreq} Hz ");
            var startFreq = (centerFreq - FFT_SIZE / 2 * chunkInHz) / 1000000f;
            var stopFreq = (centerFreq + FFT_SIZE / 2 * chunkInHz) / 1000000f;
            listBox1.Items.Add($" Each FFT point is {chunkInKHz} kHz wide ");
            listBox1.Items.Add($" Chunks of {FFT_SIZE} FFT:  {cnt}");
            listBox1.Items.Add($" Buffer length:  {buffLen}");
            listBox1.Items.Add($" Start Frequency :  {startFreq} MHz");
            listBox1.Items.Add($" Stop Frequency :  {stopFreq} MHz");
            listBox1.Items.Add($" Size of MagArray :  {magnitudeArray.Length}");
            // Logger.Log(LogLevel.Info, $"{strb}");
            // Draw FFT
            Span<double> doubleSpan = MemoryMarshal.Cast<float, double>(new Span<float>(fft_buffer));
            // Span<ushort> uintSpan = stackalloc ushort[1024];
            // for (var index = 0; index < magnitudeArray.Length; index++)
            // {
            //     var VARIABLE = magnitudeArray[index];
            //     uintSpan[index] = (ushort)VARIABLE;
            // }

            // drawspectrum(uintSpan.ToArray(), (centerFreq - FFT_SIZE / 2 * chunkInHz), chunkInHz);
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.AddSignal(doubleSpan.ToArray(), 1024);
            formsPlot1.RefreshRequest();
            //drawTimeGraph(uintSpan.ToArray());
        }

        void TheBestFFTAlgo(Span<short> buffer)
        {
            using (var timeDomain = new FftwArrayComplex(buffer.Length))
            using (var frequencyDomain = new FftwArrayComplex(buffer.Length))
            using (var fft = FftwPlanC2C.Create(timeDomain, frequencyDomain, DftDirection.Forwards))
            using (var ifft = FftwPlanC2C.Create(frequencyDomain, timeDomain, DftDirection.Backwards))
            {
                // Set the input after the plan was created as the input may be overwritten
                // during planning
                for (int i = 0; i < timeDomain.Length; i++)
                    timeDomain[i] = i % 10;

                // timeDomain -> frequencyDomain
                fft.Execute();
                listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Buffer size: {buffer.Length}");

                for (int i = frequencyDomain.Length / 2; i < frequencyDomain.Length; i++)
                    frequencyDomain[i] = 0;

                // frequencyDomain -> timeDomain
                ifft.Execute();
                double[] fftValues = new double[buffer.Length];
                for (var index = 0; index < buffer.Length; index++)
                {
                    fftValues[index] = ifft.Output[index].Magnitude;
                }

                // Do as many forwards and backwards transformations here as you like
                // drawTimeGraph(buffer.ToArray());
                formsPlot1.Plot.AddSignal(fftValues, 10);
                formsPlot1.RefreshRequest();
            }
        }

        private void drawspectrum(UInt16[] fft_data, double startFreq, double chunkInHz)
        {
            tmp.Clear(Color.Black); //clear canvas
            List<int> numbers = new List<int>(4);
            for (var index = 0; index < fft_data.Length; index++)
            {
                var VARIABLE = fft_data[index];
                numbers.Add(VARIABLE);
                if ((index % 4) == 0)
                {
                    Logger.Log(LogLevel.Info, $"{index} {startFreq + (index / 4 * chunkInHz)} - {numbers.Average()}");
                    numbers.Clear();
                }
            }

            int spectrum_h = pictureBox1.Height - 30;
            float spectrum_w = pictureBox1.Width;
            float spectrum_wScale = spectrum_w / 1024;

            int i = 1;
            int y = 0;

            for (i = 1; i <= 4; i++)
            {
                y = spectrum_h - ((i * (spectrum_h / 4)) - (spectrum_h / 6));
                tmp.DrawLine(greyPen, 10, y, spectrum_w - 10, y);
            }

            decimal segments = (decimal)spectrum_h / 256 * 2 / 3;
            var yCoordThreshold = segments * 75;
            float yTh = (float)yCoordThreshold;
            var lbl = "Weak signal cut off level";

            var ptMed = new Point((int)(spectrum_w / 2), (int)(spectrum_h - yTh));
            var offset = tmp.MeasureString(lbl, this.Font);

            ptMed.Y -= (int)offset.Height - 12;
            ptMed.X -= (int)offset.Width;
            // tmp.DrawString(lbl, this.Font, Brushes.White, ptMed);
            tmp.DrawString(lbl, new Font("Tahoma", 12), Brushes.Red, ptMed);

            tmp.DrawLine(redPen, 10, spectrum_h - yTh, spectrum_w - 10, spectrum_h - yTh);

            PointF[] points = new PointF[fft_data.Length - 2];

            for (i = 1; i < fft_data.Length - 3; i++) //ignore padding?
            {
                PointF point = new PointF(i * spectrum_wScale, 255 - fft_data[i] / 255);
                points[i] = point;
            }

            points[0] = new PointF(0, 255);
            points[points.Length - 1] = new PointF(spectrum_w, 255);


            tmp.FillRectangle(tuner1Brush,
                new RectangleF(0, 0, spectrum_w,
                    spectrum_h));

            //tmp.DrawPolygon(greenpen, points);
            SolidBrush spectrumBrush = new SolidBrush(Color.Blue);

            LinearGradientBrush linGrBrush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, 255),
                // Color.FromArgb(255, 255, 99, 132), // Opaque red
                // Color.FromArgb(255, 54, 162, 235)); // Opaque blue
                Color.Transparent,
                Color.Coral);

            tmp.FillPolygon(linGrBrush, points);

            tmp.DrawImage(bmp, 0, 255 - 30); //bandplan

            y = 0;
            int y_offset = 0;
            ;

            // for (int tuner = 0; tuner < mt.ts_devices; tuner++)
            // {
            //     y = spectrum_h - ((spectrum_h / 2) * tuner + 3);
            //     y_offset = (spectrum_h / 2) / 2 + 10;
            //
            //     //draw block showing signal selected
            //     if (rx_blocks[tuner, 0] > 0)
            //     {
            //         //tmp.FillRectangles(shadowBrush, new RectangleF[] { new System.Drawing.Rectangle(Convert.ToInt32((rx_blocks[0] * spectrum_wScale) - ((rx_blocks[1] * spectrum_wScale) / 2)), 1, Convert.ToInt32(rx_blocks[1] * spectrum_wScale), (255) - 4) });
            //         tmp.FillRectangles(shadowBrush,
            //             new RectangleF[]
            //             {
            //                 new Rectangle(
            //                     Convert.ToInt32((rx_blocks[tuner, 0] - (rx_blocks[tuner, 1] / 2)) * spectrum_wScale),
            //                     spectrum_h - y + 1, Convert.ToInt32((rx_blocks[tuner, 1] * spectrum_wScale)),
            //                     (mt.ts_devices == 1 ? spectrum_h : spectrum_h / 2) - 4)
            //             });
            //     }
            // }


            tmp.DrawString("InfoText", new Font("Tahoma", 15), Brushes.White, new PointF(10, 10));
            tmp.DrawString("TX_Text", new Font("Tahoma", 15), Brushes.Red,
                new PointF(70, pictureBox1.Height - 50)); //dh3cs
            tmp = Graphics.FromImage(bmp);

            //drawspectrum_signals(sigs.detect_signals(fft_data));
            // sigs.detect_signals(fft_data);

            // draw over power
            // foreach (var sig in sigs.signalsData)
            // {
            //     if (sig.overpower)
            //     {
            //         tmp.FillRectangles(overpowerBrush,
            //             new RectangleF[]
            //             {
            //                 new Rectangle(
            //                     Convert.ToInt16(sig.fft_centre * spectrum_wScale) -
            //                     (Convert.ToInt16((sig.fft_stop - sig.fft_start) * spectrum_wScale) / 2), 1,
            //                     Convert.ToInt16((sig.fft_stop - sig.fft_start) * spectrum_wScale), (255) - 4)
            //             });
            //     }
            // }

            // if (spectrumTunerHighlight > 0)
            //     tmp.DrawString("RX" + spectrumTunerHighlight.ToString(), new Font("Tahoma", 15), Brushes.White,
            //         new PointF(10, (spectrum_h / 2) + (spectrumTunerHighlight == 1 ? -30 : 10)));
            //
            // drawspectrum_signals(sigs.signalsData);
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBox2.Enabled)
                return;
            var selectedSdr = comboBox1.SelectedItem;
            foreach (var sdr in SDRs)
                if (sdr.DriverKey.Equals(selectedSdr))
                {
                    comboBox2.Items.Clear();
                    var nativeStreamFormat = sdr.GetNativeStreamFormat(Direction.Rx, 0, out _);
                    var freqRange = sdr.GetFrequencyRange(Direction.Rx, 0);
                    foreach (var freq in freqRange)
                    {
                        //numericUpDown1.Increment = 1000000;
                        numericUpDown1.Minimum = (decimal)freq.Minimum;
                        numericUpDown1.Maximum = (decimal)freq.Maximum;
                        numericUpDown1.Value = (decimal)qo100CenterFreq;
                    }

                    var formats = sdr.GetStreamFormats(Direction.Rx, 0);
                    foreach (var format in formats) comboBox2.Items.Add(format);
                    comboBox2.SelectedItem = nativeStreamFormat;
                }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var centerFreq = (NumericUpDown)sender;
            numericUpDown1.Value = centerFreq.Value;
            var selectedSdr = comboBox1.SelectedItem;
            foreach (var sdr in SDRs)
                if (sdr.DriverKey.Equals(selectedSdr))
                    sdr.SetFrequency(Direction.Rx, 0, (double)centerFreq.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            formsPlot1.Plot.Clear();
            formsPlot1.RefreshRequest();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (var index = 0; index < _rxStreams.Length; index++)
            {
                if (_rxStreams[index] is not null)
                {
                    _rxStreams[index].Deactivate();
                    _rxStreams[index].Close();
                    _rxStreams[index] = null;
                }
            }

            comboBox2.Enabled = true;
        }
    }
}