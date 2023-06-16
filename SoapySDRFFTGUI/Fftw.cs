
namespace SoapySDRFFTGUI
{
    public class Fftw
    {
        // float FLOAT32_EL_SIZE_BYTE = 4;
        // int FFT_SIZE = 1024;
        // double hanning_window_const = FFT_SIZE;
        // #define FFT_PRESCALE 3.0
        //
        // #define FFT_OFFSET  92
        // #define FFT_SCALE   (FFT_PRESCALE * 3000)
        //
        //
        // #define FLOOR_TARGET	(FFT_PRESCALE * 47000)
        // #define FLOOR_TIME_SMOOTH 0.995
        //
        // #define FLOOR_OFFSET    (FFT_PRESCALE * 38000)
        //
        // static uint32_t lowest_smooth = FLOOR_TARGET;
        //
        // static uint32_t fft_output_data[FFT_SIZE];
        
        // void *thread_fft(void *dummy)
        // {
        //     (void) dummy;
        //     int             i, offset;
        //     fftw_complex    pt;
        //     double           pwr, lpwr;
        //
        //     double pwr_scale = 1.0 / ((float)FFT_SIZE * (float)FFT_SIZE);
        //
        //     while(1)
        //     {
        //         /* Lock input buffer */
        //         pthread_mutex_lock(&rf_buffer.mutex);
        //
        //         if(rf_buffer.index == rf_buffer.size)
        //         {
        //             /* Wait for signalled input */
        //             pthread_cond_wait(&rf_buffer.signal, &rf_buffer.mutex);
        //         }
        //
        //         /* Move forward half an FFT length, giving half an FFT of overlap */
        //         offset = (rf_buffer.index * FFT_SIZE * 2) / 2;
        //
        //         /* Copy data out of rf buffer into fft_input buffer */
        //         for (i = 0; i < FFT_SIZE; i++)
        //         {
        //             fft_in[i][0] = ((float*)rf_buffer.data)[offset+(2*i)] * hanning_window_const[i];
        //             fft_in[i][1] = ((float*)rf_buffer.data)[offset+(2*i)+1] * hanning_window_const[i];
        //         }
        //
        //         rf_buffer.index++;
        //
        //         /* Unlock input buffer */
        //         pthread_mutex_unlock(&rf_buffer.mutex);
        //
        //         /* Run FFT */
        //         fftw_execute(fft_plan);
        //
        //         /* Lock output buffer */
        //         pthread_mutex_lock(&fft_buffer.mutex);
        //
        //         for (i = 0; i < FFT_SIZE; i++)
        //         {
        //             /* shift, normalize and convert to dBFS */
        //             if (i < FFT_SIZE / 2)
        //             {
        //                 pt[0] = fft_out[FFT_SIZE / 2 + i][0] / FFT_SIZE;
        //                 pt[1] = fft_out[FFT_SIZE / 2 + i][1] / FFT_SIZE;
        //             }
        //             else
        //             {
        //                 pt[0] = fft_out[i - FFT_SIZE / 2][0] / FFT_SIZE;
        //                 pt[1] = fft_out[i - FFT_SIZE / 2][1] / FFT_SIZE;
        //             }
        //             pwr = pwr_scale * (pt[0] * pt[0]) + (pt[1] * pt[1]);
        //             lpwr = 10.f * log10(pwr + 1.0e-20);
	       //  
        //             fft_buffer.data[i] = (lpwr * (1.f - FFT_TIME_SMOOTH)) + (fft_buffer.data[i] * FFT_TIME_SMOOTH);
        //         }
        //
        //         /* Unlock output buffer */
        //         pthread_mutex_unlock(&fft_buffer.mutex);
        //     }
        //
        // }
    }
}