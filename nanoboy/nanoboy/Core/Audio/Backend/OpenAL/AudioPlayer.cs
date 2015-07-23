using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using OpenTK.Audio;
using System.Windows.Forms;
using System.IO;

namespace nanoboy.Core.Audio.Backend.OpenAL
{
    public sealed class AudioPlayer
    {
        public List<ISoundChannel> Channels { get; set; }
        private AudioContext audiocontext;
        private int buffer;
        private uint source;
        private int delay;
        private int samplerate;
        private Thread thread;

        public AudioPlayer()
        {
            audiocontext = new AudioContext();
            //buffer = AL.GenBuffers(1)[0];
            //AL.GenSource(out source);
            //AL.Source(source, ALSourcei.Buffer, buffer);
            //AL.SourcePlay(source);
            //AL.Source(source, ALSourceb.Looping, true);
            delay = 1000;
            samplerate = 192000;
            Channels = new List<ISoundChannel>();
            thread = new Thread(PlaybackThread);
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        public static short Encode(double value)
        {
            int cnt = 0;
            while (value != Math.Floor(value)) {
                value *= 10.0;
                cnt++;
            }
            return (short)((cnt << 12) + (int)value);
        }

        private void PlaybackThread()
        {
            //int oldbuffer = -1;
            //int oldsource = -1;
            int[] sources = AL.GenSources(2);
            int currentsource = 0;
            //AL.Source(source, ALSourceb.Looping, true);
            while (true) {
                int buffer = AL.GenBuffers(1)[0];
                int numberofsamples = samplerate / (1000 / delay);
                float[] data = new float[numberofsamples];
                for (int i = 0; i < numberofsamples; i++) {
                    foreach (ISoundChannel channel in Channels) {
                        data[i] += channel.Next(samplerate);
                    }
                    data[i] *= 0.33f; // amplitude
                }
                Int16[] data2 = new Int16[numberofsamples];
                for (int i = 0; i < numberofsamples; i++) {
                    data2[i] = Encode((double)data[i]);
                }
                AL.BufferData(buffer, ALFormat.Mono16, data2, data2.Length, samplerate);
                
                ALError error = AL.GetError();
                if (error != ALError.NoError) {
                    MessageBox.Show(AL.GetErrorString(error));
                }
                AL.SourceStop(sources[currentsource]);
                currentsource = (currentsource + 1) % 2;
                AL.Source(sources[currentsource], ALSourcei.Buffer, buffer);
                AL.SourcePlay(sources[currentsource]);
                /*if (oldbuffer != -1 && oldsource != -1) {
                    AL.DeleteSource(oldsource);
                    AL.DeleteBuffer(oldbuffer);
                }
                oldbuffer = buffer;
                oldsource = source;*/
                Thread.Sleep(delay);
            }
        }

        ~AudioPlayer()
        {
            AL.DeleteBuffer(buffer);
            audiocontext.Dispose();
        }
    }
}
