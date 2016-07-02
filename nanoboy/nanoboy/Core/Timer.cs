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

namespace nanoboy.Core
{
    public sealed class Timer
    {
        public int DIV;
        public int TIMA;
        public int TMA;
        public int TAC;
        private Interrupt interrupt;
        private int divcycles;
        private int timacycles;

        public Timer(Interrupt interrupt)
        {
            this.interrupt = interrupt;
        }

        public void Tick(bool doublespeed)
        {
            int divclock = doublespeed ? 128 : 256; // twice as fast in doublespeed mode
            int timaclock = 0;

            divcycles++;
            timacycles++;

            if (divcycles == divclock) {
                divcycles = 0;
                DIV = (DIV + 1) % 256;
            }

            if ((TAC & 0x4) == 0x4) { // is the timer active?
                switch (TAC & 3) 
                {
                    case 0: timaclock = 1024; break;
                    case 1: timaclock = 16; break;
                    case 2: timaclock = 64; break;
                    case 3: timaclock = 256; break;
                }

                if (timacycles == timaclock) {
                    timacycles = 0;
                    TIMA++;
                    if (TIMA > 256) {
                        TIMA = TMA;
                        interrupt.IF |= 4;
                    }
                }
            }
        }

    }
}
