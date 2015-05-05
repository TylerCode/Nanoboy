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
    public sealed class Interrupt
    {
        public int IE { get; set; }
        public int IF { get; set; }
        private CPU cpu;

        public Interrupt(CPU cpu)
        {
            this.cpu = cpu;
        }

        public void Tick()
        {
            if (cpu.IME) {
                int masked = IE & IF;
                if ((masked & 1) == 1) {
                    cpu.Interrupt(0x40);
                    IF &= ~1;
                    return;
                }
                if ((masked & 2) == 2) {
                    cpu.Interrupt(0x48);
                    IF &= ~2;
                    return;
                }
                if ((masked & 4) == 4) {
                    cpu.Interrupt(0x50);
                    IF &= ~4;
                    return;
                }
                if ((masked & 8) == 8) {
                    cpu.Interrupt(0x58);
                    IF &= ~8;
                    return;
                }
                if ((masked & 16) == 16) {
                    cpu.Interrupt(0x60);
                    IF &= ~16;
                    return;
                }
            }
        }

    }
}
