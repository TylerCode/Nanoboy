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

using System.Windows.Forms;

namespace nanoboy.Core
{
    /// <summary>
    /// The class "Joypad" emulates the joypad.
    /// </summary>
    public class Joypad
    {
        public IEmulatorSettings Settings;
        public bool SelectButtonKeys;
        public bool SelectDirectionKeys;
        public bool KeyDown;
        public bool KeyUp;
        public bool KeyLeft;
        public bool KeyRight;
        public bool KeyA;
        public bool KeyB;
        public bool KeyStart;
        public bool KeySelect;
        private Interrupt interrupt;

        public Joypad(Interrupt interrupt)
        {
            this.interrupt = interrupt;
            KeyDown = true;
            KeyUp = true;
            KeyLeft = true;
            KeyRight = true;
            KeyA = true;
            KeyB = true;
            KeyStart = true;
            KeySelect = true;
        }

        public void Set(Keys key, bool status)
        {
            if (Settings != null) {
                if (key == Settings.KeyA) {
                    KeyA = status;
                } else if (key == Settings.KeyB) {
                    KeyB = status;
                } else if (key == Settings.KeyStart) {
                    KeyStart = status;
                } else if (key == Settings.KeySelect) {
                    KeySelect = status;
                } else if (key == Settings.KeyUp) {
                    KeyUp = status;
                } else if (key == Settings.KeyDown) {
                    KeyDown = status;
                } else if (key == Settings.KeyLeft) {
                    KeyLeft = status;
                } else if (key == Settings.KeyRight) {
                    KeyRight = status;
                }
                if (!status) {
                    interrupt.IF |= 16;
                }
            }
        }

    }
}
