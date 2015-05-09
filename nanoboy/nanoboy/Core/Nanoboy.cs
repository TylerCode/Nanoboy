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
using System.Drawing;
using System.Windows.Forms;

namespace nanoboy.Core
{
    public sealed class Nanoboy : IDisposable
    {

        public IntPtr Image
        {
            get
            {
                return memory.Video.Frame;
            }
        }
        private CPU cpu;
        private Memory memory;

        public Nanoboy(ROM rom)
        {
            cpu = new CPU();
            memory = new Memory(cpu, rom);
            cpu.Memory = memory;
            Reset();
        }

        public void Frame()
        {
            int cyclesleft = 0;
            for (int i = 0; i < 70224; i++) {
                for (int j = 0; j < 4 * (cpu.IsDoubleSpeed ? 2 : 1); j++) {
                    if (cyclesleft == 0) {
                        cyclesleft = cpu.Tick();
                    }
                    cyclesleft--;
                }
                memory.Interrupt.Tick();
                memory.Video.Tick();
                memory.channel1.Tick();
                memory.channel2.Tick();
                memory.channel3.Tick();
                memory.Timer.Tick(false);
            }
            memory.Video.FrameReady = false;
        }

        public void Reset()
        {
            cpu.A = 0x11;
            cpu.B = 0x00;
            cpu.C = 0x13;
            cpu.D = 0x00;
            cpu.E = 0xD8;
            cpu.H = 0x01;
            cpu.L = 0x4D;
            cpu.SP = 0xFFFE;
            cpu.PC = 0x0100;
            cpu.FlagZ = true;
            cpu.FlagN = false;
            cpu.FlagH = true;
            cpu.FlagC = true;
            cpu.Memory.WriteByte(0xFF05, 0x00); // TIMA
            cpu.Memory.WriteByte(0xFF06, 0x00); // TMA
            cpu.Memory.WriteByte(0xFF07, 0x00); // TAC
            cpu.Memory.WriteByte(0xFF10, 0x80); // NR10
            cpu.Memory.WriteByte(0xFF11, 0xBF); // NR11
            cpu.Memory.WriteByte(0xFF12, 0xF3); // NR12
            cpu.Memory.WriteByte(0xFF14, 0xBF); // NR14
            cpu.Memory.WriteByte(0xFF16, 0x3F); // NR21
            cpu.Memory.WriteByte(0xFF17, 0x00); // NR22
            cpu.Memory.WriteByte(0xFF19, 0xBF); // NR24
            cpu.Memory.WriteByte(0xFF1A, 0x7F); // NR30
            cpu.Memory.WriteByte(0xFF1B, 0xFF); // NR31
            cpu.Memory.WriteByte(0xFF1C, 0x9F); // NR32
            cpu.Memory.WriteByte(0xFF1E, 0xBF); // NR33
            cpu.Memory.WriteByte(0xFF20, 0xFF); // NR41
            cpu.Memory.WriteByte(0xFF21, 0x00); // NR42
            cpu.Memory.WriteByte(0xFF22, 0x00); // NR43
            cpu.Memory.WriteByte(0xFF23, 0xBF); // NR30
            cpu.Memory.WriteByte(0xFF24, 0x77); // NR50
            cpu.Memory.WriteByte(0xFF25, 0xF3); // NR51
            cpu.Memory.WriteByte(0xFF26, 0xF1); // NR52
            cpu.Memory.WriteByte(0xFF40, 0x91); // LCDC
            cpu.Memory.WriteByte(0xFF42, 0x00); // SCY
            cpu.Memory.WriteByte(0xFF43, 0x00); // SCX
            cpu.Memory.WriteByte(0xFF45, 0x00); // LYC
            cpu.Memory.WriteByte(0xFF47, 0xFC); // BGP
            cpu.Memory.WriteByte(0xFF48, 0xFF); // OBP0
            cpu.Memory.WriteByte(0xFF49, 0xFF); // OBP1
            cpu.Memory.WriteByte(0xFF4A, 0x00); // WY
            cpu.Memory.WriteByte(0xFF4B, 0x00); // WX
            cpu.Memory.WriteByte(0xFFFF, 0x00); // IE
        }

        public void SetKey(Keys key)
        {
            memory.Joypad.Set(key, false);
        }

        public void UnsetKey(Keys key)
        {
            memory.Joypad.Set(key, true);
        }


        public void Dispose()
        {
            memory.Dispose();
        }
    }
}
