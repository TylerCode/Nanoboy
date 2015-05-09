using System;
using System.Collections.Generic;
using System.Threading;
using OpenTK.Audio;
//using OpenTK.Audio.OpenAL;

namespace nanoboy.Core.Audio.Backend.OpenAL
{
    public sealed class AudioPlayer
    {
        public List<ISoundChannel> Channels { get; set; }
        private Thread thread;

        public AudioPlayer()
        {
            Channels = new List<ISoundChannel>();
            // initialize openal
            int buffer = AL.GenBuffer();
            AudioContext audiocontext = new AudioContext();
            XRamExtension xram = new XRamExtension();
            if (xram.IsInitialized) {
                xram.SetBufferMode(0, ref buffer, XRamExtension.XRamStorage.Hardware);
            }
            AL.SourcePlay(buffer);
            AL.Source(buffer, ALSourceb.Looping, true);
            thread = new Thread(delegate() {

            });
            thread.Priority = ThreadPriority.Highest;
        }

        public void Start() 
        {
            thread.Start();
        }

        public void Stop()
        {
            //thread.
        }
    }
}
