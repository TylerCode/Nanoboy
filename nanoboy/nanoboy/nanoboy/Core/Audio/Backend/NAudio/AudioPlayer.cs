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
using NAudio;
using NAudio.Wave;

// TODO: Refactor

/* Partially based on http://mark-dot-net.blogspot.de/2009/10/playback-of-sine-wave-in-naudio.html */
namespace nanoboy.Core.Audio.Backend.NAudio
{
    public abstract class WaveProvider32 : IWaveProvider
    {
        private WaveFormat waveFormat;

        public WaveProvider32()
            : this(44100, 1)
        {
        }

        public WaveProvider32(int sampleRate, int channels)
        {
            SetWaveFormat(sampleRate, channels);
        }

        public void SetWaveFormat(int sampleRate, int channels)
        {
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            WaveBuffer waveBuffer = new WaveBuffer(buffer);
            int samplesRequired = count / 4;
            int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }

        public abstract int Read(float[] buffer, int offset, int sampleCount);

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }
    }

    public class Mixer : WaveProvider32
    {
        public Mixer()
        {
            Amplitude = 0.25f; // let's not hurt our ears
            Channels = new List<ISoundChannel>();
        }

        public float Amplitude { get; set; }
        public List<ISoundChannel> Channels { get; set; }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            //int sampleRate = WaveFormat.SampleRate;
            /*for (int n = 0; n < sampleCount; n++)
            {
                buffer[n + offset] = (float)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
                sample++;
                if (sample >= sampleRate) sample = 0;
            }*/
            for (int n = 0; n < sampleCount; n++)
            {
                buffer[n + offset] = 0;
                foreach(ISoundChannel channel in Channels)
                {
                    buffer[n + offset] += channel.Next(WaveFormat.SampleRate);
                }
                buffer[n + offset] *= Amplitude;
            }
            return sampleCount;
        }
    }

    public class AudioPlayer : IDisposable
    {
        public List<ISoundChannel> Channels { get; set; }
        private Mixer mixer;
        private WaveOut waveOut;

        public AudioPlayer()
        {
            Channels = new List<ISoundChannel>();
            mixer = new Mixer();
            waveOut = new WaveOut();
        }

        public void Start()
        {
            mixer.Channels = this.Channels;
            waveOut.Init(mixer);
            waveOut.Play();
        }

        public void Pause()
        {
            waveOut.Pause();
        }

        public void Stop()
        {
            this.Dispose();            
        }

        public void Dispose()
        {
            waveOut.Stop();
            waveOut.Dispose();
        }
    }
}
