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
        public bool Enabled { get; set; }
        private int ticks;
        private int samples;
        private List<float> samplebuffer;

        public Audio()
        {
            Channel1 = new QuadChannel();
            Channel2 = new QuadChannel();
            Channel3 = new WaveChannel();
            Channel4 = new NoiseChannel();
            Enabled = true;
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

            // At a frequency of 44,1khz read samples from enabled channels
            if (ticks++ == 95) {
                if (Enabled) {
                    float sample = (Channel1.Enabled ? Channel1.Next(44100) : 0) +
                                   (Channel2.Enabled ? Channel2.Next(44100) : 0) +
                                   (Channel3.Enabled ? Channel3.Next(44100) : 0) +
                                   (Channel4.Enabled ? Channel4.Next(44100) : 0);
                    samplebuffer.Add(sample);
                    if (samples++ == 3528) {
                        if (AudioAvailable != null) {
                            AudioAvailable(this, new AudioAvailableEventArgs(samplebuffer.ToArray()));
                        }
                        samplebuffer.Clear();
                        samples = 0;
                    }
                }
                ticks = 0;
            }
        }

    }
}
