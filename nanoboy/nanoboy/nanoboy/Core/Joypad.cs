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
        public bool SelectButtonKeys { get; set; }
        public bool SelectDirectionKeys { get; set; }
        public bool KeyDown { get; set; }
        public bool KeyUp { get; set; }
        public bool KeyLeft { get; set; }
        public bool KeyRight { get; set; }
        public bool KeyA { get; set; }
        public bool KeyB { get; set; }
        public bool KeyStart { get; set; }
        public bool KeySelect { get; set; }
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
            switch (key) 
            {
                case Keys.Shift:
                    KeySelect = status;
                    break;
                case Keys.Enter:
                    KeyStart = status;
                    break;
                case Keys.Up:
                    KeyUp = status;
                    break;
                case Keys.Down:
                    KeyDown = status;
                    break;
                case Keys.Left:
                    KeyLeft = status;
                    break;
                case Keys.Right:
                    KeyRight = status;
                    break;
                case Keys.X:
                    KeyB = status;
                    break;
                case Keys.Y:
                    KeyA = status;
                    break;
            }
            if (status) {
                interrupt.IF |= 16;
            }
        }

    }
}
