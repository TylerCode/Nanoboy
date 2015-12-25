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
using nanoboy.Core;
using System.Windows.Forms;

namespace nanoboy
{
    public sealed class NanoboySettings : IEmulatorSettings
    {
        public bool AudioEnable
        {
            get { return nanoboy.Properties.Settings.Default.AudioEnable; }
            set { nanoboy.Properties.Settings.Default.AudioEnable = value; }
        }

        public bool Channel1Enable
        {
            get { return nanoboy.Properties.Settings.Default.Channel1Enable; }
            set { nanoboy.Properties.Settings.Default.Channel1Enable = value; }
        }

        public bool Channel2Enable
        {
            get { return nanoboy.Properties.Settings.Default.Channel2Enable; }
            set { nanoboy.Properties.Settings.Default.Channel2Enable = value; }
        }

        public bool Channel3Enable
        {
            get { return nanoboy.Properties.Settings.Default.Channel3Enable; }
            set { nanoboy.Properties.Settings.Default.Channel3Enable = value; }
        }

        public bool Channel4Enable
        {
            get { return nanoboy.Properties.Settings.Default.Channel4Enable; }
            set { nanoboy.Properties.Settings.Default.Channel4Enable = value; }
        }

        public int VideoScaleFactor
        {
            get { return nanoboy.Properties.Settings.Default.VideoScaleFactor; }
            set { nanoboy.Properties.Settings.Default.VideoScaleFactor = value; }
        }

        public int Frameskip
        {
            get { return nanoboy.Properties.Settings.Default.Frameskip; }
            set { nanoboy.Properties.Settings.Default.Frameskip = value; }
        }

        public Keys KeyA
        {
            get { return nanoboy.Properties.Settings.Default.KeyA; }
            set { nanoboy.Properties.Settings.Default.KeyA = value; }
        }

        public Keys KeyB
        {
            get { return nanoboy.Properties.Settings.Default.KeyB; }
            set { nanoboy.Properties.Settings.Default.KeyB = value; }
        }

        public Keys KeyStart
        {
            get { return nanoboy.Properties.Settings.Default.KeyStart; }
            set { nanoboy.Properties.Settings.Default.KeyStart = value; }
        }

        public Keys KeySelect
        {
            get { return nanoboy.Properties.Settings.Default.KeySelect; }
            set { nanoboy.Properties.Settings.Default.KeySelect = value; }
        }

        public Keys KeyUp
        {
            get { return nanoboy.Properties.Settings.Default.KeyUp; }
            set { nanoboy.Properties.Settings.Default.KeyUp = value; }
        }

        public Keys KeyDown
        {
            get { return nanoboy.Properties.Settings.Default.KeyDown; }
            set { nanoboy.Properties.Settings.Default.KeyDown = value; }
        }

        public Keys KeyLeft
        {
            get { return nanoboy.Properties.Settings.Default.KeyLeft; }
            set { nanoboy.Properties.Settings.Default.KeyLeft = value; }
        }

        public Keys KeyRight
        {
            get { return nanoboy.Properties.Settings.Default.KeyRight; }
            set { nanoboy.Properties.Settings.Default.KeyRight = value; }
        }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            nanoboy.Properties.Settings.Default.Save();
        }

        public NanoboySettings()
        {
            nanoboy.Properties.Settings.Default.PropertyChanged += PropertyChanged;
        }
    }
}
