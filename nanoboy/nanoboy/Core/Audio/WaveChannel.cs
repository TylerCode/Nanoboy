﻿/*
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
using nanoboy.Core.Audio.Backend;

namespace nanoboy.Core.Audio
{
    public sealed class WaveChannel
    {
        // Enables or disables the channel
        public bool Enabled;

        // Sampledata 
        public byte[] WaveRAM;
        
        // Raw, unconverted frequency
        public int FrequencyRaw;

        // Converted frequency
        public int Frequency {
            get {
                return (int)Audio.ConvertFrequency(FrequencyRaw);
            }
        }
        
        // Channel activity
        public bool On;

        // Sound length
        public int SoundLength {
            get {
                return (256 - SoundLengthRaw) * (1 / 256) * 4194304;
            }
        }
        public int SoundLengthRaw;
        public bool StopOnLengthExpired;
        private int soundlengthcycles;

        // Channel output level
        public int OutputLevel;
        private float[] outputlevels = new float[] {0f, 1f, 0.5f, 0.25f};

        // Sound generation
        private int sample;

        public WaveChannel()
        {
            Enabled = true;
            WaveRAM = new byte[0x20];
        }

        public float Next(int samplerate)
        {
            if (On && (!StopOnLengthExpired || soundlengthcycles <= SoundLength)) {
                float index = (float)((5.093108 * Math.PI * sample * Audio.ConvertFrequency(FrequencyRaw)) / samplerate) % WaveRAM.Length;
                float value = (float)WaveRAM[(int)index] / 16f;
                if (++sample >= samplerate) {
                    sample = 0;
                }
                return outputlevels[OutputLevel] * value;
            } else {
                return 0f;
            }
        }

        public void Tick()
        {
            if (StopOnLengthExpired) {
                soundlengthcycles++;
            }
        }

        public void Restart()
        {
            soundlengthcycles = 0;
        }
    }
}
