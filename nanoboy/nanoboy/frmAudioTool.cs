using System;
using System.Windows.Forms;
using nanoboy.Core;
using nanoboy.Core.Audio;

namespace nanoboy
{
    public partial class frmAudioTool : Form
    {
        public Nanoboy Nanoboy;

        public frmAudioTool()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Audio audio;

            if (Nanoboy != null) {
                audio = Nanoboy.Memory.Audio;

                // Channel Volume
                levelDisplayControl1.Level = (int)(audio.Channel1.CurrentVolume / 16f * levelDisplayControl1.Height);
                levelDisplayControl2.Level = (int)(audio.Channel2.CurrentVolume / 16f * levelDisplayControl2.Height);
                levelDisplayControl3.Level = audio.Channel3.OutputLevel * levelDisplayControl3.Height;
                levelDisplayControl4.Level = (int)(audio.Channel4.CurrentVolume / 16f * levelDisplayControl4.Height);

                // Channel 1
                labelQ1Freq.Text = audio.Channel1.CurrentFrequency.ToString() + "Hz";
                labelQ1SweepCycles.Text = QuadChannel.SweepClockTable[audio.Channel1.SweepTime].ToString();
                labelQ1SweepShift.Text = audio.Channel1.SweepShift.ToString();
                labelQ1SweepDirection.Text = audio.Channel1.SweepDirection == SweepMode.Addition ? "Up" : "Down";
                labelQ1EnvelSweep.Text = audio.Channel1.EnvelopeSweep.ToString();
                labelQ1EnvelDirection.Text = audio.Channel1.EnvelopeDirection == EnvelopeMode.Increase ? "Up" : "Down";
                labelQ1SoundLength.Text = audio.Channel1.SoundLength.ToString() + (!audio.Channel1.StopOnLengthExpired ? " (ignored)" : "");
                labelQ1WaveDuty.Text = QuadChannel.WaveDutyTable[audio.Channel1.WavePatternDuty].ToString();

                // Channel 2
                labelQ2Freq.Text = audio.Channel2.CurrentFrequency.ToString() + "Hz";
                labelQ2EnvelSweep.Text = audio.Channel2.EnvelopeSweep.ToString();
                labelQ2EnvelDirection.Text = audio.Channel2.EnvelopeDirection == EnvelopeMode.Increase ? "Up" : "Down";
                labelQ2SoundLength.Text = audio.Channel2.SoundLength.ToString() + (!audio.Channel2.StopOnLengthExpired ? " (ignored)" : "");
                labelQ2WaveDuty.Text = QuadChannel.WaveDutyTable[audio.Channel2.WavePatternDuty].ToString();

                // Channel 3
                labelWFreq.Text = audio.Channel3.Frequency.ToString() + "Hz";
                labelWSoundLength.Text = audio.Channel3.SoundLength.ToString() + (!audio.Channel3.StopOnLengthExpired ? " (ignored)" : "");
                waveDataControl1.WaveForm = audio.Channel3.WaveRAM;

                // Channel 4
                labelNClockFreq.Text = audio.Channel4.ClockFrequency.ToString();
                labelNDividingRatio.Text = audio.Channel4.DividingRatio.ToString();
                labelNCounterBits.Text = audio.Channel4.CounterStep ? "7 bits" : "15 bits";
                labelNCounter.Text = audio.Channel4.Counter.ToString();
                labelNResultFreq.Text = audio.Channel4.ResultFrequency.ToString();
                labelNEnvelSweep.Text = audio.Channel4.EnvelopeSweep.ToString();
                labelNEnvelDirection.Text = audio.Channel4.EnvelopeDirection == EnvelopeMode.Increase ? "Up" : "Down";
                labelNSoundLength.Text = audio.Channel4.SoundLength.ToString() + (!audio.Channel4.StopOnLengthExpired ? " (ignored)" : "");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = checkBox1.Enabled;
        }
    }
}
