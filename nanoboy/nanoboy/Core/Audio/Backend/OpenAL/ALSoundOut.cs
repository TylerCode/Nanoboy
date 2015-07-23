using System;
using OpenTK;
using OpenTK.Audio;
using nanoboy.Core.Audio;
using NAudio;
using NAudio.Wave;

namespace nanoboy.Core.Audio.Backend.OpenAL
{
    public sealed class ALSoundOut
    {
        public float Amplitude { get; set; }
        private AudioContext audiocontext;
        private WaveFileWriter writer;
        private int source;

        public ALSoundOut(Audio audio)
        {
            Amplitude = 0.25f;
            audiocontext = new AudioContext();
            writer = new WaveFileWriter("dump.wav", WaveFormat.CreateIeeeFloatWaveFormat(44100, 1));
            source = AL.GenSource();
            audio.AudioAvailable += audio_AudioAvailable;
        }

        ~ALSoundOut()
        {
            audiocontext.Dispose();
            writer.Close();
            writer.Dispose();
        }

        private void audio_AudioAvailable(object sender, AudioAvailableEventArgs e)
        {
            short[] buffer = new short[e.Buffer.Length];
            int albuffer = AL.GenBuffer();
            for (int i = 0; i < buffer.Length; i++) {
                buffer[i] = ConvertFloatTo16Bit(e.Buffer[i] * Amplitude);
            }
            // DUMP FOR REFERENCE
            for (int i = 0; i < e.Buffer.Length; i++) {
                writer.WriteSample(e.Buffer[i] * Amplitude);
                writer.Flush();
            }
            // PROCEED
            AL.SourcePause(source);
            AL.BufferData(albuffer, ALFormat.Mono16, buffer, buffer.Length * 2, 44100);
            AL.SourceQueueBuffer(source, albuffer);
            AL.SourcePlay(source);
        }

        private static Int16 ConvertFloatTo16Bit(float value)
        {
            return (Int16)(value * 32768);
        }
    }
}
