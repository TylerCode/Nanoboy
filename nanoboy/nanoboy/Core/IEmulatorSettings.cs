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
using System.Windows.Forms;

namespace nanoboy.Core
{
    public interface IEmulatorSettings
    {
        bool Channel1Enable { get; set; }
        bool Channel2Enable { get; set; }
        bool Channel3Enable { get; set; }
        bool Channel4Enable { get; set; }
        bool AudioEnable { get; set; }
        int Frameskip { get; set; }
        Keys KeyA { get; set; }
        Keys KeyB { get; set; }
        Keys KeyStart { get; set; }
        Keys KeySelect { get; set; }
        Keys KeyUp { get; set; }
        Keys KeyDown { get; set; }
        Keys KeyLeft { get; set; }
        Keys KeyRight { get; set; }
    }
}
