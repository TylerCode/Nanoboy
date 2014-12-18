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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace nanoboy.Core
{
    /// <summary>
	/// The class "Gameboy" represents the emulator. It connects the different devices to a gameboy emulator.
	/// </summary>
    public class Gameboy
    {
        public Disassembler Disassembler { get; set; }
        private Cpu cpu;
        private MemoryRouter memory;
        private Timer timer;
        private Video video;
        private Joypad joypad;
        private int cycToGo = 0;

        public Gameboy(Rom rom, bool useBios = true)
        {
            memory = new MemoryRouter(rom.Memory);
            timer = new Timer();
            video = new Video();
            joypad = new Joypad();
            cpu = new Cpu(memory);
            Disassembler = new Disassembler(memory);

            video.MemoryRouter = memory;
            memory.Video = video;
            joypad.MemoryRouter = memory;
            memory.Joypad = joypad;
            timer.MemoryRouter = memory;
            memory.Timer = timer;

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
                        cycToGo = cpu.ExecuteOp() - 1;
                    } else {
                        cycToGo--;
                    }
                    video.ModeClock++;
                    HandleVideo();
                    timer.Tick();
                }
            }
        }

        public Bitmap GetVideo()
        {
            return video.Buffer;
        }

        public void SetKey(Keys key)
        {
            joypad.HandleInput(key, false);
        }

        public void ClearKey(Keys key)
        {
            joypad.HandleInput(key, true);
        }

        private void HandleInterrupts()
        {
            if (cpu.Ime || cpu.WaitForInterrupt)
            {
                int ifired = memory.Ie & memory.If;
                if ((ifired & 1) == 1)
                {
                    memory.If &= 0xFE;
                    cpu.Interrupt(0x40);
                    return;
                } else if ((ifired & 2) == 2) {
                    memory.If &= 0xFD;
                    cpu.Interrupt(0x48);
                    return;
                } else if ((ifired & 4) == 4) {
                    memory.If &= 0xFB;
                    cpu.Interrupt(0x50);
                    return;
                } else if ((ifired & 8) == 8) {
                    memory.If &= 0xF7;
                    cpu.Interrupt(0x58);
                    return;
                } else if ((ifired & 16) == 16) {
                    memory.If &= 0xEF;
                    cpu.Interrupt(0x60);
                    return;
                }
            }
        }

        private void HandleVideo()
        {
            switch (video.ModeFlag)
            {
                case 0x2: // Scanline (accessing OAM)
                    if (video.ModeClock >= 80)
                    {
                        video.ModeFlag = 3;
                        video.ModeClock = 0;
                    }
                    break;
                case 0x3: // Scanline (accessing VRAM)
                    if (video.ModeClock >= 172)
                    {
                        video.ModeFlag = 0;
                        video.ModeClock = 0;

                        // Render line @ LY
                        video.RenderLine();
                    }
                    break;
                case 0x0: // HBlank
                    if (video.ModeClock >= 204)
                    {
                        video.ModeClock = 0;
                        video.LY++;
                        if (video.LY == video.LYC && video.CoincidenceInterrupt)
                            memory.If |= 2;
                        if (video.LY == 144)
                        {
                            video.ModeFlag = 1;
                        } else {
                            video.ModeFlag = 2;
                        }
                    }
                    break;
                case 0x1: // VBlank
                    if (video.ModeClock >= 456)
                    {
                        video.ModeClock = 0;
                        video.LY++;
                        if (video.LY == video.LYC && video.CoincidenceInterrupt)
                            memory.If |= 2;
                        // Request VBlank Interrupt
                        memory.If |= 1;

                        if (video.LY > 153)
                        {
                            video.ModeFlag = 2;
                            video.LY = 0;
                        }
                    }
                    break;
            }
        }

        private void InitDevice()
        {
            cpu.A = 0x10;
            cpu.B = 0x00;
            cpu.C = 0x13;
            cpu.D = 0x00;
            cpu.E = 0xD8;
            cpu.H = 0x01;
            cpu.L = 0x4D;
            cpu.Sp = 0xFFFE;
            cpu.Pc = 0x0100;
            cpu.FlagZ = true;
            cpu.FlagN = false;
            cpu.FlagHc = true;
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

    }
}
