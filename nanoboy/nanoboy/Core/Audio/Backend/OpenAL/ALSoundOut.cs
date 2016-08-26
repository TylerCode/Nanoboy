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
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Audio;

using System.IO;

namespace nanoboy.Core.Audio.Backend.OpenAL
{
    public sealed class ALSoundOut : SoundOut
    {
        public float Amplitude { get; set; }
        private int source;
        private int[] buffers;
        private AudioContext audiocontext;
        private Queue<short[]> audioqueue;
        private Thread audiothread;
        private int currentrate;

        public ALSoundOut(Audio audio) : base(audio)
        {
            Amplitude = 0.25f;
            audioqueue = new Queue<short[]>();
            currentrate = audio.SampleRate;
            audiothread = new Thread(StreamingThread);
            audiothread.Priority = ThreadPriority.Highest;
            audiothread.Start();
        }

        ~ALSoundOut()
        {
            Dispose();
        }

        public override void Dispose()
        {
            audiothread.Abort();
            audiocontext.Dispose();
        }

        protected override void Audio_AudioAvailable(object sender, AudioAvailableEventArgs e)
        {
            short[] buffer = new short[e.Buffer.Length];

            // Update sample rate
            if (currentrate != e.SampleRate) {
                currentrate = e.SampleRate;
                audiothread.Abort();
                audiothread = new Thread(StreamingThread);
                audiothread.Priority = ThreadPriority.Highest;
                audiothread.Start();
            }

            // Convert given buffer data
            for (int i = 0; i < buffer.Length; i++) {
                buffer[i] = ConvertFloatTo16Bit(e.Buffer[i] * Amplitude);
            }

            // Flush the queue if there are more than 20 buffers pending..
            if (audioqueue.Count > 20) {
                audioqueue.Clear();
            }

            // Pass it to the audio queue
            audioqueue.Enqueue(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stream(int buffer)
        {
            short[] data;
            if (audioqueue.Count == 0) {
                data = new short[500];
            } else {
                data = audioqueue.Dequeue();
            }
            AL.BufferData(buffer, ALFormat.Mono16, data, data.Length * 2, currentrate);
        }

        private void StreamingThread()
        {
            audiocontext = new AudioContext();
            source = AL.GenSource();
            buffers = AL.GenBuffers(10);
            AL.Source(source, ALSourceb.SourceRelative, true);

            for (int i = 0; i < 10; i++) {
                Stream(buffers[i]);
            }

            AL.SourceQueueBuffers(source, 2, buffers);
            AL.SourcePlay(source);

            while (true) {
                int processed;
                AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processed);

                if (processed != 0) {
                    int bufferid = 0;
                    AL.SourceUnqueueBuffers(source, 1, ref bufferid);
                    Stream(bufferid);
                    AL.SourceQueueBuffer(source, bufferid);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int16 ConvertFloatTo16Bit(float value)
        {
            return (Int16)(value * 32768);
        }
    }
}