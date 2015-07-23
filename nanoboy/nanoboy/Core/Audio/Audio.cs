using System;
using System.Collections.Generic;

namespace nanoboy.Core.Audio
{
    public sealed class AudioAvailableEventArgs : EventArgs
    {
        public float[] Buffer { get; set; }

        public AudioAvailableEventArgs(float[] buffer)
        {
            Buffer = buffer;
        }
    }

    public sealed class Audio
    {
        public event EventHandler<AudioAvailableEventArgs> AudioAvailable;
        public QuadChannel Channel1 { get; set; }
        public QuadChannel Channel2 { get; set; }
        public WaveChannel Channel3 { get; set; }
        public NoiseChannel Channel4 { get; set; }
        private int ticks;
        private int samples;
        private List<float> samplebuffer;

        public Audio()
        {
            Channel1 = new QuadChannel();
            Channel2 = new QuadChannel();
            Channel3 = new WaveChannel();
            Channel4 = new NoiseChannel();
            ticks = 0;
            samples = 0;
            samplebuffer = new List<float>();
        }

        public void Tick()
        {
            // Schedule all channels
            Channel1.Tick();
            Channel2.Tick();
            Channel3.Tick();
            Channel4.Tick();
            // At a frequency of 44,1khz read samples
            if (ticks++ == 95) {
                float sample = Channel1.Next(44100) +
                               Channel2.Next(44100) +
                               Channel3.Next(44100) +
                               Channel4.Next(44100);
                samplebuffer.Add(sample);
                if (samples++ == 44) {
                    AudioAvailable(this, new AudioAvailableEventArgs(samplebuffer.ToArray()));
                    samplebuffer.Clear();
                    samples = 0;
                }
                ticks = 0;
            }
        }

    }
}
