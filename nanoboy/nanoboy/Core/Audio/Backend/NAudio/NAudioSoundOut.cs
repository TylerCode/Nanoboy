using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace nanoboy.Core.Audio.Backend.NAudio
{
    class GameboyWaveProvider : WaveProvider32
    {
        private Queue<float> samples = new Queue<float>();

        public void Audio_AudioAvailable(object sender, AudioAvailableEventArgs e)
        {
            foreach (float b in e.Buffer)
                samples.Enqueue(b);
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            //int sampleRate = WaveFormat.SampleRate;
            for (int n = 0; n < sampleCount; n++)
            {
                if (samples.Count > 0)
                    buffer[n + offset] = samples.Dequeue();
                
            }
            return sampleCount;
        }
    }

    public class NAudioSoundOut : SoundOut
    {
        private WaveOut waveOut;
        GameboyWaveProvider wave = new GameboyWaveProvider();

        public NAudioSoundOut(Audio audio) : base(audio)
        {
            wave.SetWaveFormat(44100, 1); // 16kHz mono
            waveOut = new WaveOut();
            waveOut.Init(wave);
            waveOut.Play();
            audio.AudioAvailable += wave.Audio_AudioAvailable;
        }

        public override void Dispose()
        {
            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;
        }

        protected override void Audio_AudioAvailable(object sender, AudioAvailableEventArgs e)
        {

        }
    }
}
