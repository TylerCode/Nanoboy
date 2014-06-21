/*
 * Copyright (C) 2014 Frederic Meyer
 * 
 * This file is part of GeekBoy.
 *
 * GeekBoy is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *   
 * GeekBoy is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with GeekBoy.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace GeekBoy
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
	
    public class SquareWaveProvider32 : WaveProvider32
    {
        int sample;
 
        public SquareWaveProvider32()
        {
            Frequency = 0;
            Amplitude = 0.25f; // let's not hurt our ears            
        }
 
        public float Frequency { get; set; }
        public float Amplitude { get; set; }
 
        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate;
            for (int n = 0; n < sampleCount; n++)
            {
                //buffer[n+offset] = (float)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
                if (Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate) > 0)
                    buffer[offset + n] = Amplitude;
                else
                    buffer[offset + n] = -Amplitude;
                sample++;
                if (sample >= sampleRate) sample = 0;
            }
            return sampleCount;
        }
    }

    public class ToneWaveProvider32 : WaveProvider32
    {
        int sample;
 
        public ToneWaveProvider32()
        {
            Frequency = 0;
            Amplitude = 0.25f; // let's not hurt our ears            
        }
 
        public float Frequency { get; set; }
        public float Amplitude { get; set; }
        public int Length { get; set; }
        public bool UseLength { get; set; }
        public bool EnvelopeDirection { get; set; }
 
        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate;
            for (int n = 0; n < sampleCount; n++)
            {
                if (Length > 0 || !UseLength)
                { 
                    //buffer[n+offset] = (float)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
                    if (Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate) > 0)
                        buffer[offset + n] = Amplitude;
                    else
                        buffer[offset + n] = -Amplitude;
                } else {
                    buffer[offset + n] = 0;
                }
                sample++;
                if (sample >= sampleRate) sample = 0;
            }
            return sampleCount;
        }
    }
	
    public struct AudioChannel /* Contains the channel specific registers */
    {
        public byte Sweep; // sweep register
        public byte SW; // sound length and wave duty
        public byte VolEnv; // volume envelope
        public byte FrLo; // frequency low
        public byte FrHi; // frequency high
    }

    public class Audio
    {
        public AudioChannel[] Channels { get; set; }
		private WaveOut[] _waveOut;
        private ToneWaveProvider32[] _waveProvider;
        private int[] _envClock;
        private int[] _envStep;
		
		public Audio()
		{
            Channels = new AudioChannel[4];
            _waveOut = new WaveOut[4];
            _waveProvider = new ToneWaveProvider32[4];
            _envClock = new int[4];
            _envStep = new int[4];
            for (int i = 0; i < 4; i++)
            {
                Channels[i] = new AudioChannel();
                _waveOut[i] = new WaveOut();
                _waveProvider[i] = new ToneWaveProvider32();
                _waveOut[i].Init(_waveProvider[i]);
                _waveOut[i].Play();
            }
		}

		public void HandleTone(int id, int register = 0)
		{
            int x, frequency;

            switch (register)
            {
                case 0: 
                    break;
                case 1:
                    _waveProvider[id].Length = (64 - (Channels[id].SW & 0x3F)) * (1/256) * 1000000; // 100000;
                    break;
                case 2:
                    _waveProvider[id].Amplitude = (0.25f / 15f) * (Channels[id].VolEnv >> 4);
                    _waveProvider[id].EnvelopeDirection = (Channels[id].VolEnv & 8) == 8;
                    break;
                case 3: 
                    x = (Channels[id].FrLo + ((Channels[id].FrHi & 7) << 8));
			        frequency = 131072 / (2048 - x);
                    _waveProvider[id].Frequency = frequency;
                    break;
                case 4: 
                    x = (Channels[id].FrLo + ((Channels[id].FrHi & 7) << 8));
			        frequency = 131072 / (2048 - x);
                    _waveProvider[id].Frequency = frequency;
                    _waveProvider[id].UseLength = (Channels[id].FrHi & 0x40) == 0x40;
                    if ((Channels[id].FrHi & 0x80) == 0x80)
                    {
                        _waveOut[id].Stop();
                        _waveOut[id] = new WaveOut();
                        _waveOut[id].Init(_waveProvider[id]);
                        _waveOut[id].Play();
                        Channels[id].FrHi -= 0x80;
                    }
                    break;
            }
		}

        public void Tick()
        {
            for (int i = 0; i < 4; i++)
            { 
                _waveProvider[i].Length--;
                _envClock[i]++;
                if (_envClock[i] == (Channels[i].VolEnv & 7) * (1/64) * 1000000)
                {
                    _envStep[i]++;
                    _waveProvider[i].Amplitude -= 0.01f;
                    if (_waveProvider[i].Amplitude < 0f) _waveProvider[i].Amplitude = 0f;
                }
            }
        }
		
	}
}
