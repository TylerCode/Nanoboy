/*
 * Copyright (C) 2014 Frederic Meyer
 * 
 * This file is part of GeekBoy.
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
 * along with GeekBoy.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace GeekBoy.Core
{
    /// <summary>
	/// The class "GameboyTimer" emulates the timer of the gameboy.
	/// </summary>
    public class Timer
    {
        public int DIV { get; set; }
        public int TIMA { get; set; }
        public int TMA { get; set; }
        public bool Running { get; set; }
        public int Frequency { get; set; }
        private int Clock1;
        private int Clock2;

        public MemoryRouter MemoryRouter { get; set; }

        private void TimerDIV()
        {
            DIV = (DIV + 1) % 256;
        }

        private void TimerTIMA()
        {
            if (Running)
            { 
                TIMA++;
                if (TIMA == 256)
                {
                    MemoryRouter.If |= 4;
                    TIMA = TMA;
                }
            }
        }

        public void Tick()
        {
            Clock1++;
            Clock2++;

            if (Clock1 == 61)
            {
                TimerDIV();
                Clock1 = 0;
            }

            if (Frequency != 0)
            {
                if (Clock2 == 1048576 / Frequency)
                {
                    TimerTIMA();
                    Clock2 = 0;
                }
            }
        }

    }
}
