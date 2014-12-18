/*
 * Copyright (C) 2014 Frederic Meyer
 * 
 * This file is part of nanoboy.
 *
 * GeekBoy is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *   
 * GeekBoy is distributed in the hope that it will be useful,
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
        public MemoryRouter MemoryRouter;

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

        public Joypad()
        {
            KeyDown = true;
            KeyUp = true;
            KeyLeft = true;
            KeyRight = true;
            KeyA = true;
            KeyB = true;
            KeyStart = true;
            KeySelect = true;
        }

        public void HandleInput(Keys k, bool value)
        {
            switch (k)
            {
                case Keys.Shift:
                    KeySelect = value;
                    break;
                case Keys.Enter:
                    KeyStart = value;
                    break;
                case Keys.Up:
                    KeyUp = value;
                    break;
                case Keys.Down:
                    KeyDown = value;
                    break;
                case Keys.Left:
                    KeyLeft = value;
                    break;
                case Keys.Right:
                    KeyRight = value;
                    break;
                case Keys.X:
                    KeyB = value;
                    break;
                case Keys.Y:
                    KeyA = value;
                    break;
            }
            if (value)
                MemoryRouter.If |= 16;
        }

    }
}
