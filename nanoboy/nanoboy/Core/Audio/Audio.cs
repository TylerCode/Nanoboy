/*
 * Copyright (C) 2014 - 2015 Frederic Meyer
 * 
 * This file is part of nanoboy.
 *
 * nanoboy is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *   
 * nanoboy is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with nanoboy.  If not, see <http://www.gnu.org/licenses/>.
 */

using nanoboy.Core.Audio.Backend;
using nanoboy.Core.Audio.Backend.NAudio;
using nanoboy.Core.Audio.Backend.OpenAL;
using System;
using System.Collections.Generic;

namespace nanoboy.Core.Audio
{
    public enum SweepMode
    {
        Addition = 0,
        Substraction = 1
    }

    public enum EnvelopeMode
    {
        Decrease = 0,
        Increase = 1
    }

    public enum SoundOutMode
    {
        OpenAL = 0,
        NAudio = 1
    }

    public sealed class AudioAvailableEventArgs : EventArgs
    {
        public float[] Buffer { get; set; }
        public int SampleRate { get; set; }

        public AudioAvailableEventArgs(float[] buffer, int rate)
        {
            Buffer = buffer;
            SampleRate = rate;
        }
    }

    public sealed class Audio : IDisposable
    {
        public event EventHandler<AudioAvailableEventArgs> AudioAvailable;
        public QuadChannel Channel1;
        public QuadChannel Channel2;
        public WaveChannel Channel3;
        public NoiseChannel Channel4;
        public int SampleRate;
        public int BufferSize;
        public bool Enabled;
        private int ticks;
        private int samples;
        private List<float> samplebuffer;

        public SoundOut soundout;

        public Audio(SoundOutMode mode)
        {
            Channel1 = new QuadChannel();
            Channel2 = new QuadChannel();
            Channel3 = new WaveChannel();
            Channel4 = new NoiseChannel();
            SampleRate = 44100;
            BufferSize = 1024;
            Enabled = true;
            ticks = 0;
            samples = 0;
            samplebuffer = new List<float>();

            switch (mode)
            {
                default:
                case SoundOutMode.OpenAL:
                    soundout = new ALSoundOut(this);
                    break;

                case SoundOutMode.NAudio:
                    soundout = new NAudioSoundOut(this);
                    break;

            }
        }

        public void Tick()
        {
            // Schedule all channels
            Channel1.Tick();
            Channel2.Tick();
            Channel3.Tick();
            Channel4.Tick();

            // At a given sample rate read samples from enabled channels
            if (ticks++ == 4057200 / SampleRate) {
                if (Enabled) {
                    float sample = (Channel1.Enabled ? Channel1.Next(SampleRate) : 0) +
                                   (Channel2.Enabled ? Channel2.Next(SampleRate) : 0) +
                                   (Channel3.Enabled ? Channel3.Next(SampleRate) : 0) +
                                   (Channel4.Enabled ? Channel4.Next(SampleRate) : 0);
                    samplebuffer.Add(sample);
                    if (samples++ == BufferSize) {
                        AudioAvailable?.Invoke(this, new AudioAvailableEventArgs(samplebuffer.ToArray(), SampleRate));
                        samplebuffer.Clear();
                        samples = 0;
                    }
                }
                ticks = 0;
            }
        }

        public static float ConvertFrequency(int frequency)
        {
            return 131072 / (2048 - frequency);
        }

        public void Dispose()
        {
            ((IDisposable)soundout).Dispose();
        }
    }
}
