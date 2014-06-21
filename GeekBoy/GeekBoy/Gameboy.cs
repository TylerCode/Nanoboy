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
using System.Threading;
using MicroLibrary;

namespace GeekBoy
{
    /// <summary>
	/// The class "Gameboy" represents the emulator. It connects the different devices to a gameboy emulator.
	/// </summary>
    public class Gameboy
    {
        public Video Video { get; set; }
        public Audio Audio { get; set; }
        public Joypad Joypad { get; set; }
        private Cpu cpu;
        private MemoryRouter mr;
        private Timer timer;
        private Thread mc;

        public Gameboy(Rom rom, bool useBios = true)
        {
            mr = new MemoryRouter(rom.Memory);
            cpu = new Cpu(mr);
            timer = new Timer();
            Video = new Video();
            Joypad = new Joypad();
            Audio = new Audio();

            Video.MemoryRouter = mr;
            mr.Video = Video;
            Joypad.MemoryRouter = mr;
            mr.Joypad = Joypad;
            timer.MemoryRouter = mr;
            mr.Timer = timer;
            mr.Audio = Audio;
            
            if (!useBios) InitDevice();
        }


        public void MainCycle()
        {
            mc = new Thread(_MainCycle);
            mc.Priority = ThreadPriority.Highest;
            mc.Start();
        }

        private void _MainCycle()
        {
            MicroStopwatch mw = new MicroStopwatch();
            int cycToGo = 0;
            while (true)
            {
                mw.Start();
                
                for (int i = 0; i < 4; i++)
                {
                    HandleInterrupts();
                    if (cycToGo == 0)
                    {
                        cycToGo = cpu.ExecuteOp() - 1;
                    } else {
                        cycToGo--;
                    }
                    Video.ModeClock++;
                    HandleVideo();
                }

                timer.Tick();
                Audio.Tick();

                while (mw.ElapsedMicroseconds == 0) ;
                mw.Reset();
            }
        }

        private void _debugCore(int pc)
        {
            Console.WriteLine("AF: 0x{0:X} BC: 0x{1:X} DE: 0x{2:X} HL: 0x{3:X} SP: 0x{4:X} PC: 0x{5:X}",
                               (cpu.A << 8) + cpu.F,
                               (cpu.B << 8) + cpu.C,
                               (cpu.D << 8) + cpu.E,
                               (cpu.H << 8) + cpu.L,
                               cpu.Sp,
                               pc);
            Console.ReadLine();
        }

        private void HandleInterrupts()
        {
            if (cpu.Ime || cpu.WaitForInterrupt)
            {
                int ifired = mr.Ie & mr.If;
                if ((ifired & 0x01) == 1)
                {
                    mr.If &= 0xFE;
                    cpu.Interrupt(0x40);
                    return;
                } else if ((ifired & 2) == 2) {
                    mr.If &= 0xFD;
                    cpu.Interrupt(0x48);
                    return;
                } else if ((ifired & 4) == 4) {
                    mr.If &= 0xFB;
                    cpu.Interrupt(0x50);
                    return;
                } else if ((ifired & 8) == 8) {
                    mr.If &= 0xF7;
                    cpu.Interrupt(0x58);
                    return;
                } else if ((ifired & 16) == 16) {
                    mr.If &= 0xEF;
                    cpu.Interrupt(0x60);
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
                            mr.If |= 2;
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
                            mr.If |= 2;
                        // Request VBlank Interrupt
                        mr.If |= 1;

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
            cpu.A = 0x01;
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
            mr.RomEnable = true;
        }

    }
}
