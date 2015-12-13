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
using OpenTK;
using OpenTK.Audio;
using nanoboy.Core.Audio;

using System.IO;

namespace nanoboy.Core.Audio.Backend.OpenAL
{
    public sealed class ALSoundOut
    {
        public float Amplitude { get; set; }
        private int source;
        private int[] buffers;
        private AudioContext audiocontext;
        private Queue<short[]> audioqueue;
        private Thread audiothread;

        public ALSoundOut(Audio audio)
        {
            Amplitude = 0.25f;
            audio.AudioAvailable += audio_AudioAvailable;
            audioqueue = new Queue<short[]>();
            audiothread = new Thread(StreamingThread);
            audiothread.Priority = ThreadPriority.AboveNormal;
            audiothread.Start();
        }

        ~ALSoundOut()
        {
            audiothread.Suspend();
            audiocontext.Dispose();
        }

        private void audio_AudioAvailable(object sender, AudioAvailableEventArgs e)
        {
            short[] buffer = new short[e.Buffer.Length];

            // Convert given buffer data
            for (int i = 0; i < buffer.Length; i++) {
                buffer[i] = ConvertFloatTo16Bit(e.Buffer[i] * Amplitude);
            }

            // Flush the queue if the lag is bigger than 100ms (approx. 2 buffers pending)
            if (audioqueue.Count > 2) {
                audioqueue.Dequeue();
            }

            // Pass it to the audio queue
            audioqueue.Enqueue(buffer);
        }

        private void Stream(int buffer)
        {
            short[] data;
            if (audioqueue.Count == 0) {
                data = new short[2048];
            } else {
                data = audioqueue.Dequeue();
            }
            AL.BufferData(buffer, ALFormat.Mono16, data, data.Length * 2, 44100);
        }

        private void StreamingThread()
        {
            audiocontext = new AudioContext();
            source = AL.GenSource();
            buffers = AL.GenBuffers(2);
            AL.Source(source, ALSourceb.SourceRelative, true);
            Stream(buffers[0]);
            Stream(buffers[1]);
            AL.SourceQueueBuffers(source, 2, buffers);
            AL.SourcePlay(source);

            while (true) {
                int processed;
                AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processed);

                // add more buffers while we need them
                while (processed-- > 0) {
                    int bufferid = 0;
                    AL.SourceUnqueueBuffers(source, 1, ref bufferid);
                    Stream(bufferid);
                    AL.SourceQueueBuffer(source, bufferid);
                }
            }
        }

        private static Int16 ConvertFloatTo16Bit(float value)
        {
            return (Int16)(value * 32768);
        }
    }
}