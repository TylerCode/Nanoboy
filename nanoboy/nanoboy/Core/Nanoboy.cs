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
        public Disassembler Disassembler { get; set; }
        public IntPtr Image
        {
            get
            {
                return memory.Video.Frame;
            }
        }
        public CPU Cpu { get; set; }
        private Memory memory;

        public Nanoboy(ROM rom)
        {
            Cpu = new CPU();
            memory = new Memory(Cpu, rom);
            Cpu.Memory = memory;
            Disassembler = new Disassembler(memory);
            Reset();
        }

        public void Frame()
        {
            int cyclesleft = 0;
            for (int i = 0; i < 70224; i++) {
                for (int j = 0; j < 4 * (Cpu.IsDoubleSpeed ? 2 : 1); j++) {
                    if (cyclesleft == 0) {
                        cyclesleft = Cpu.Tick();
                    }
                    cyclesleft--;
                }
                memory.Interrupt.Tick();
                memory.Video.Tick();
                memory.Audio.Tick();
                memory.Timer.Tick(false);
            }
            memory.Video.FrameReady = false;
        }

        public void Reset()
        {
            Cpu.A = 0x11;
            Cpu.B = 0x00;
            Cpu.C = 0x13;
            Cpu.D = 0x00;
            Cpu.E = 0xD8;
            Cpu.H = 0x01;
            Cpu.L = 0x4D;
            Cpu.SP = 0xFFFE;
            Cpu.PC = 0x0100;
            Cpu.FlagZ = true;
            Cpu.FlagN = false;
            Cpu.FlagH = true;
            Cpu.FlagC = true;
            Cpu.Memory.WriteByte(0xFF05, 0x00); // TIMA
            Cpu.Memory.WriteByte(0xFF06, 0x00); // TMA
            Cpu.Memory.WriteByte(0xFF07, 0x00); // TAC
            Cpu.Memory.WriteByte(0xFF10, 0x80); // NR10
            Cpu.Memory.WriteByte(0xFF11, 0xBF); // NR11
            Cpu.Memory.WriteByte(0xFF12, 0xF3); // NR12
            Cpu.Memory.WriteByte(0xFF14, 0xBF); // NR14
            Cpu.Memory.WriteByte(0xFF16, 0x3F); // NR21
            Cpu.Memory.WriteByte(0xFF17, 0x00); // NR22
            Cpu.Memory.WriteByte(0xFF19, 0xBF); // NR24
            Cpu.Memory.WriteByte(0xFF1A, 0x7F); // NR30
            Cpu.Memory.WriteByte(0xFF1B, 0xFF); // NR31
            Cpu.Memory.WriteByte(0xFF1C, 0x9F); // NR32
            Cpu.Memory.WriteByte(0xFF1E, 0xBF); // NR33
            Cpu.Memory.WriteByte(0xFF20, 0xFF); // NR41
            Cpu.Memory.WriteByte(0xFF21, 0x00); // NR42
            Cpu.Memory.WriteByte(0xFF22, 0x00); // NR43
            Cpu.Memory.WriteByte(0xFF23, 0xBF); // NR30
            Cpu.Memory.WriteByte(0xFF24, 0x77); // NR50
            Cpu.Memory.WriteByte(0xFF25, 0xF3); // NR51
            Cpu.Memory.WriteByte(0xFF26, 0xF1); // NR52
            Cpu.Memory.WriteByte(0xFF40, 0x91); // LCDC
            Cpu.Memory.WriteByte(0xFF42, 0x00); // SCY
            Cpu.Memory.WriteByte(0xFF43, 0x00); // SCX
            Cpu.Memory.WriteByte(0xFF45, 0x00); // LYC
            Cpu.Memory.WriteByte(0xFF47, 0xFC); // BGP
            Cpu.Memory.WriteByte(0xFF48, 0xFF); // OBP0
            Cpu.Memory.WriteByte(0xFF49, 0xFF); // OBP1
            Cpu.Memory.WriteByte(0xFF4A, 0x00); // WY
            Cpu.Memory.WriteByte(0xFF4B, 0x00); // WX
            Cpu.Memory.WriteByte(0xFFFF, 0x00); // IE
        }

        public Subscription<CPUStatusUpdate> Debug(IObserver<CPUStatusUpdate> debugger)
        {
            return (Subscription<CPUStatusUpdate>)Cpu.Subscribe(debugger);
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
