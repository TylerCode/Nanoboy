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

namespace nanoboy.Core
{
    /// <summary>
	/// The class "Gameboy" represents the emulator. It connects the different devices to a gameboy emulator.
	/// </summary>
    public class Gameboy
    {
        public Video Video { get; set; }
        public Joypad Joypad { get; set; }
        public Cpu Cpu { get; set; }
        public Disassembler Disassembler { get; set; }
        public MemoryRouter Memory {get; set; }
        private Timer timer;
        private int cycToGo = 0;

        public Gameboy(Rom rom, bool useBios = true)
        {
            Memory = new MemoryRouter(rom.Memory);
            timer = new Timer();
            Video = new Video();
            Joypad = new Joypad();
            Cpu = new Cpu(Memory);
            Disassembler = new Disassembler(Memory);

            Video.MemoryRouter = Memory;
            Memory.Video = Video;
            Joypad.MemoryRouter = Memory;
            Memory.Joypad = Joypad;
            timer.MemoryRouter = Memory;
            Memory.Timer = timer;

            InitDevice();
        }

        public void Step()
        {
            for (int i = 0; i < 70224; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    HandleInterrupts();
                    if (cycToGo == 0)
                    {
                        cycToGo = Cpu.ExecuteOp() - 1;
                    } else {
                        cycToGo--;
                    }
                    Video.ModeClock++;
                    HandleVideo();
                    timer.Tick();
                }
            }
        }

        private void _debugCore(int pc)
        {
            Console.WriteLine("AF: 0x{0:X} BC: 0x{1:X} DE: 0x{2:X} HL: 0x{3:X} SP: 0x{4:X} PC: 0x{5:X}",
                               (Cpu.A << 8) + Cpu.F,
                               (Cpu.B << 8) + Cpu.C,
                               (Cpu.D << 8) + Cpu.E,
                               (Cpu.H << 8) + Cpu.L,
                               Cpu.Sp,
                               pc);
            Console.ReadLine();
        }

        private void HandleInterrupts()
        {
            if (Cpu.Ime || Cpu.WaitForInterrupt)
            {
                int ifired = Memory.Ie & Memory.If;
                if ((ifired & 1) == 1)
                {
                    Memory.If &= 0xFE;
                    Cpu.Interrupt(0x40);
                    return;
                } else if ((ifired & 2) == 2) {
                    Memory.If &= 0xFD;
                    Cpu.Interrupt(0x48);
                    return;
                } else if ((ifired & 4) == 4) {
                    Memory.If &= 0xFB;
                    Cpu.Interrupt(0x50);
                    return;
                } else if ((ifired & 8) == 8) {
                    Memory.If &= 0xF7;
                    Cpu.Interrupt(0x58);
                    return;
                } else if ((ifired & 16) == 16) {
                    Memory.If &= 0xEF;
                    Cpu.Interrupt(0x60);
                    return;
                }
            }
        }

        private void HandleVideo()
        {
            switch (Video.ModeFlag)
            {
                case 0x2: // Scanline (accessing OAM)
                    if (Video.ModeClock >= 80)
                    {
                        Video.ModeFlag = 3;
                        Video.ModeClock = 0;
                    }
                    break;
                case 0x3: // Scanline (accessing VRAM)
                    if (Video.ModeClock >= 172)
                    {
                        Video.ModeFlag = 0;
                        Video.ModeClock = 0;

                        // Render line @ LY
                        Video.RenderLine();
                    }
                    break;
                case 0x0: // HBlank
                    if (Video.ModeClock >= 204)
                    {
                        Video.ModeClock = 0;
                        Video.LY++;
                        if (Video.LY == Video.LYC && Video.CoincidenceInterrupt)
                            Memory.If |= 2;
                        if (Video.LY == 144)
                        {
                            Video.ModeFlag = 1;
                        } else {
                            Video.ModeFlag = 2;
                        }
                    }
                    break;
                case 0x1: // VBlank
                    if (Video.ModeClock >= 456)
                    {
                        Video.ModeClock = 0;
                        Video.LY++;
                        if (Video.LY == Video.LYC && Video.CoincidenceInterrupt)
                            Memory.If |= 2;
                        // Request VBlank Interrupt
                        Memory.If |= 1;

                        if (Video.LY > 153)
                        {
                            Video.ModeFlag = 2;
                            Video.LY = 0;
                        }
                    }
                    break;
            }
        }

        private void InitDevice()
        {
            Cpu.A = 0x10;
            Cpu.B = 0x00;
            Cpu.C = 0x13;
            Cpu.D = 0x00;
            Cpu.E = 0xD8;
            Cpu.H = 0x01;
            Cpu.L = 0x4D;
            Cpu.Sp = 0xFFFE;
            Cpu.Pc = 0x0100;
            Cpu.FlagZ = true;
            Cpu.FlagN = false;
            Cpu.FlagHc = true;
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

    }
}
