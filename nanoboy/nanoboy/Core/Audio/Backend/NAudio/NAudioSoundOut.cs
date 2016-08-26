using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nanoboy.Core.Audio.Backend.NAudio
{
    class NAudioSoundOut : SoundOut
    {
        public NAudioSoundOut(Audio audio) : base(audio)
        {

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override void Audio_AudioAvailable(object sender, AudioAvailableEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
