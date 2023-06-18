namespace SoapySDRFFTGUI;

public class junk
{
    // uint FFT_SIZE = 1024;
                    
    // var bufferSlice = floatSpan.Slice((int)totalSamps, (int)expectedSamps);
                    
    // Span<float> floatSpan = MemoryMarshal.Cast<byte, float>(new Span<byte>(buff));

    // var expectedSamps = Math.Min(_rxStreams[sdrNumber].MTU, (n - totalSamps));
    // for (int i = 0; i < n; i++)
    // {
    //     StreamResult streamResult;
    //
    //     // var bufferSlice = floatSpan.Slice((int)totalSamps, (int)expectedSamps);
    //     var errorCode = _rxStreams[sdrNumber].Read(ref buff, 10000, out streamResult);
    //     listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
    //     listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Receive flags:  " + streamResult.Flags);
    //
    //     if (errorCode == ErrorCode.None)
    //     {
    //         for (var index = 0; index < 10; index++)
    //         {
    //             var VARIABLE = buff[index];
    //             listBox1.Items.Add(VARIABLE);
    //         }
    //     }
    //     
    //     // listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Timestamp (ns):  " + streamResult.TimeNs);
    // }

    // totalSamps += FFT_SIZE;
    //
    // if(bufferSlice.Length > 0) listBox1.Items.Add(bufferSlice[0]);

    // ExampleUsePlanDirectly((int)FFT_SIZE, streamResult, n);
    // Span<float> floatSpan = MemoryMarshal.Cast<byte, float>(new Span<byte>(buff));
    // uint totalSamps = 0;
                   
    // for (var i = 0; i < n; ++i)
    // {
    //    
    //     var expectedSamps = Math.Min(_rxStreams[sdrNumber].MTU, (n - totalSamps));
    //     
    //     var bufferSlice = floatSpan.Slice((int)totalSamps, (int)expectedSamps);
    //     // var streamFlags = Pothosware.SoapySDR.StreamFlags.None;
    //     // if ((totalSamps + expectedSamps) == n) streamFlags |= Pothosware.SoapySDR.StreamFlags.EndBurst;
    //     StreamResult streamResult;
    //     
    //     var errorCode = _rxStreams[sdrNumber].Read(bufferSlice, 100000, out streamResult);
    //     for (var index = 0; index < 16; index++)
    //     {
    //         var VARIABLE = bufferSlice[index] * hanning_window_const[index];
    //         listBox1.Items.Add(VARIABLE);
    //     }
    //     listBox1.Items.Add(bufferSlice.ToString());
    //     // var errorCode = _rxStreams[sdrNumber].Read(buff, 100000, out streamResult);
    //     listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Error code:     " + errorCode);
    //     // using (var timeDomain = new FftwArrayComplex(buff.Length))
    //     // using (var frequencyDomain = new FftwArrayComplex(buff.Length))
    //     // using (var fft = FftwPlanC2C.Create(timeDomain, frequencyDomain, DftDirection.Forwards))
    //     // using (var ifft = FftwPlanC2C.Create(frequencyDomain, timeDomain, DftDirection.Backwards))
    //     // {
    //     //     // Set the input after the plan was created as the input may be overwritten
    //     //     // during planning
    //     //     for (int j = 0; j < timeDomain.Length; j++)
    //     //         timeDomain[i] = i % 10;
    //     //
    //     //     // timeDomain -> frequencyDomain
    //     //     fft.Execute();
    //     //     for (var index = 0; index < 50; index++)
    //     //     {
    //     //         var VARIABLE = ifft.Output[index];
    //     //         listBox1.Items.Add(VARIABLE);
    //     //     }
    //     // }
    //     listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Receive flags:  " + streamResult.Flags);
    //     listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | Timestamp (ns): " + streamResult.TimeNs);
    // }
    /*
     *  FFT stuff
     */
     // var mtu = rxStream.MTU;
    // listBox1.Items.Add($"Stream MTU: {mtu}");
    // var format = StreamFormat.ComplexFloat32;
    // var formatSize = StreamFormat.FormatToSize(format);
    // // Create as buffer for ease of writing, but we can use System.Span to use this
    // // memory with the appropriate type.
    // ushort[] buffer = new ushort[mtu * formatSize];
    // var floatSpan = MemoryMarshal.Cast<ushort, float>(new Span<ushort>(buffer));
    //
    // uint totalSamps = 0;
    // double[] hanning_window_const = new double[fftSize];
    // for(var i=0; i < fftSize; i++)
    // {
    //     hanning_window_const[i] = 0.5 * (1.0 - Math.Cos(2*Math.PI*(((double)i)/fftSize)));
    //     listBox1.Items.Add(hanning_window_const[i]);
    // }
    // while (totalSamps < numSamps)
    // {
    //     var expectedSamps = Math.Min(mtu, (numSamps - totalSamps));
    //
    //     var bufferSlice = floatSpan.Slice((int)totalSamps, (int)expectedSamps);
    //
    //     var streamFlags = Pothosware.SoapySDR.StreamFlags.None;
    //     if ((totalSamps + expectedSamps) == numSamps) streamFlags |= Pothosware.SoapySDR.StreamFlags.EndBurst;
    //
    //     var streamResult = new Pothosware.SoapySDR.StreamResult();
    //     var status = rxStream.Read(bufferSlice, 100000, out streamResult);
    //     if (status != Pothosware.SoapySDR.ErrorCode.None)
    //     {
    //         throw new ApplicationException(string.Format("Read returned: {0}", status));
    //     }
    //     else if (streamResult.NumSamples != expectedSamps)
    //     {
    //         throw new ApplicationException(string.Format("Read returned {0} elements, expected {1}",
    //             streamResult.NumSamples, expectedSamps));
    //     }
    //
    //     // listBox1.Items.Add(Encoding.Default.GetString(buffer));
    //     // AppendData(file, buffer, (int)(totalSamps * formatSize), (int)(expectedSamps * formatSize));
    //     using (var timeDomain = new FftwArrayComplex(buffer.Length))
    //     using (var frequencyDomain = new FftwArrayComplex(buffer.Length))
    //     using (var fft = FftwPlanC2C.Create(timeDomain, frequencyDomain, DftDirection.Forwards))
    //     using (var ifft = FftwPlanC2C.Create(frequencyDomain, timeDomain, DftDirection.Backwards))
    //     {
    //         // Set the input after the plan was created as the input may be overwritten
    //         // during planning
    //         for (int i = 0; i < timeDomain.Length; i++)
    //             timeDomain[i] = i % 10;
    //
    //         // timeDomain -> frequencyDomain
    //         fft.Execute();
    //         listBox1.Items.Add($"{DateTime.Now.ToLocalTime()} | {buffer.Length}");
    //         // for (int j = 0; j < 100; j++)
    //         // {
    //         //     var VARIABLE = buffer[j];
    //         //     listBox1.Items.Add(VARIABLE);
    //         // }
    //         // for (var index = 0; index < 50; index++)
    //         // {
    //         //     var VARIABLE = fft.Output[index];
    //         //     listBox1.Items.Add(VARIABLE);
    //         // }
    //         // listBox1.Items.Add(timeDomain.GetSize());
    //
    //         for (int i = frequencyDomain.Length / 2; i < frequencyDomain.Length; i++)
    //             frequencyDomain[i] = 0;
    //         
    //         // frequencyDomain -> timeDomain
    //         ifft.Execute();
    //         for (var index = 0; index < 50; index++)
    //         {
    //             var VARIABLE = ifft.Output[index];
    //             listBox1.Items.Add(VARIABLE);
    //         }
    //         // Do as many forwards and backwards transformations here as you like
    //     }
    //
    //     totalSamps += streamResult.NumSamples;
    // }
}