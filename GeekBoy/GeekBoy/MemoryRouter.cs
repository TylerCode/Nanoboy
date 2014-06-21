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
using System.IO;

namespace GeekBoy
{
    /// <summary>
	/// The class "MemoryRouter" emulates the memory managing unit (mmu).
	/// </summary>
    public class MemoryRouter : IMemoryDevice
    {
        public IMemoryDevice Mbc { get; set; }
        public Video Video { get; set; }
        public Joypad Joypad { get; set; }
        public Timer Timer { get; set; }
        public Audio Audio { get; set; }
        public int Ie { get; set; }
        public int If { get; set; }
        public bool RomEnable { get; set; }
        private byte[]  _wram = new byte[0x2000];
        private byte[] _hram = new byte[0x7F];
        private byte[] _bios;

        public MemoryRouter(IMemoryDevice mbc)
        {
            Mbc = mbc;
            _bios = File.ReadAllBytes("DMG_ROM.BIN");
        }

        public byte ReadByte(int address)
        {
            int value = 0;
            if (address >= 0 && address <= 0x7FFF)
            {
                if (!RomEnable && address <= 0xFF)
                    return _bios[address];
                return Mbc.ReadByte(address);
            }
            if (address >= 0x8000 && address <= 0x9FFF) {
                return Video.VRAM[address - 0x8000];
            }
            if (address >= 0xA000 && address <= 0xBFFF) {
                return Mbc.ReadByte(address);
            }
            if (address >= 0xC000 && address <= 0xDFFF) {
                return _wram[address - 0xC000];
            }
            if (address >= 0xE000 && address <= 0xFDFF) {
                return _wram[address - 0xE000];
            }
            if (address >= 0xFE00 && address <= 0xFE9F) {
                return Video.OAM[address - 0xFE00];
            }
            if (address >= 0xFEA0 && address <= 0xFEFF) {
                // NOT USABLE 
                return 0;
            }
            if (address >= 0xFF00 && address <= 0xFF7F) { 
                // IO
                switch (address - 0xFF00)
                {
                    case 0x00: // Joypad
                        value = Joypad.SelectButtonKeys ? 0x20 : 0x00;
                        value += Joypad.SelectDirectionKeys ? 0x10 : 0x00;
                        if (!Joypad.SelectDirectionKeys)
                        {
                            value += Joypad.KeyDown ? 0x00 : 0x08;
                            value += Joypad.KeyUp ? 0x00 : 0x04;
                            value += Joypad.KeyLeft ? 0x00 : 0x02;
                            value += Joypad.KeyRight ? 0x00 : 0x01;
                        } else if (!Joypad.SelectButtonKeys) {
                            value += Joypad.KeyStart ? 0x00 : 0x08;
                            value += Joypad.KeySelect ? 0x00 : 0x04;
                            value += Joypad.KeyB ? 0x00 : 0x02;
                            value += Joypad.KeyA ? 0x00 : 0x01;
                        }
                        return (byte)value;
                    case 0x04: // DIV (TIMER)
                        return (byte)Timer.DIV;
                    case 0x05: // TIMA
                        return (byte)Timer.TIMA;
                    case 0x06: // TMA
                        return (byte)Timer.TMA;
                    case 0x07: // TAC
                        value = Timer.Running ? 0x04 : 0x00;
                        switch (Timer.Frequency)
                        {
                            case 262144:
                                value += 1;
                                break;
                            case 65536:
                                value += 2;
                                break;
                            case 16384:
                                value += 3;
                                break;
                        }
                        return (byte)value;
                    case 0x0F: // Interrupt Flag
                        return (byte)If;
                    case 0x10: // NR10
                        return Audio.Channels[0].Sweep;
                    case 0x11: // NR11
                        return Audio.Channels[0].SW;
                    case 0x12: // NR12
                        return Audio.Channels[0].VolEnv;
                    case 0x13: // NR13
                        return Audio.Channels[0].FrLo;
                    case 0x14: // NR14
                        return Audio.Channels[0].FrHi;
                    case 0x40: // LCDC
                        value = Video.LcdEnable ? 0x80 : 0x0;
                        value += Video.WindowTileMapSelect ? 0x40 : 0x0;
                        value += Video.WindowEnable ? 0x20 : 0x0;
                        value += Video.TileDataSelect ? 0x10 : 0x0;
                        value += Video.BgTileMapSelect ? 0x08 : 0x0;
                        value += Video.ObjSize ? 0x04 : 0x0;
                        value += Video.ObjEnable ? 0x02 : 0x0;
                        value += Video.BgEnable ? 0x01 : 0x0;
                        return (byte)value;
                    case 0x41: // LCDC STAT
                        value = Video.CoincidenceInterrupt ? 0x40 : 0x0;
                        value += Video.OamInterrupt ? 0x20 : 0x0;
                        value += Video.VBlankInterrupt ? 0x10 : 0x0;
                        value += Video.HBlankInterrupt ? 0x08 : 0x0;
                        value += Video.CoincidenceFlag ? 0x04 : 0x0;
                        value += Video.ModeFlag & 0x03;
                        return (byte)value;
                    case 0x42: // SCY
                        return (byte)Video.SCY;
                    case 0x43: // SCX
                        return (byte)Video.SCX;
                    case 0x44: // LY  
                        return (byte)Video.LY;
                    case 0x45: // LYC
                        return (byte)Video.LYC;
                    case 0x47: // BGP
                        return (byte)Video.BGP;
                    case 0x48: // OBP0
                        return (byte)Video.OBP0;
                    case 0x49: // OBP1
                        return (byte)Video.OBP1;
                    case 0x4A: // WY
                        return (byte)Video.WY;
                    case 0x4B: // WX
                        return (byte)Video.WX;
                    case 0x50: // ROM ENABLE
                        return RomEnable ? (byte)1 : (byte)0;
                    default:
                        return 0;
                }
            }
            if (address >= 0xFF80 && address <= 0xFFFE) {
                return _hram[address - 0xFF80];
            }
            if (address == 0xFFFF) {
                // INTERRUPT ENABLE REGISTER
                return (byte)Ie;
            }
            throw new Exception("Cannot handle memory address 0x" + address.ToString("X") + " (on read)");
        }

        public void WriteByte(int address, byte value)
        {
            if (address <= 0x7FFF)
            {
                Mbc.WriteByte(address, value);
            } else if (address >= 0x8000 && address <= 0x9FFF) {
                Video.VRAM[address - 0x8000] = value;
            } else if (address >= 0xA000 && address <= 0xBFFF) {
                Mbc.WriteByte(address, value);
            } else if (address >= 0xC000 && address <= 0xDFFF) {
                _wram[address - 0xC000] = value;
            } else if (address >= 0xE000 && address <= 0xFDFF) { 
                // NOP
            }else if (address >= 0xFE00 && address <= 0xFE9F) {
                Video.OAM[address - 0xFE00] = value;
            } else if (address >= 0xFEA0 && address <= 0xFEFF) {
                // NOT USABLE 
            } else if (address >= 0xFF00 && address <= 0xFF7F) { 
                // IO
                switch (address - 0xFF00)
                {
                    case 0x00: // Joypad
                        Joypad.SelectButtonKeys = ((value >> 5) & 1) == 1;
                        Joypad.SelectDirectionKeys = ((value >> 4) & 1) == 1;
                        break;
                    case 0x04: // DIV (TIMER)
                        Timer.DIV = 0;
                        break;
                    case 0x05: // TIMA
                        Timer.TIMA = value;
                        break;
                    case 0x06: // TMA
                        Timer.TMA = value;
                        break;
                    case 0x07: // TAC
                        Timer.Running = ((value >> 2) & 1) == 1;
                        switch (value & 3)
                        {
                            case 0x0:
                                Timer.Frequency = 4096;
                                break;
                            case 0x1:
                                Timer.Frequency = 262144;
                                break;
                            case 0x2:
                                Timer.Frequency = 65536;
                                break;
                            case 0x3:
                                Timer.Frequency = 16384;
                                break;
                        }
                        break;
                    case 0x0F: // Interrupt Flag
                        If = value;
                        break;
                    case 0x10: // NR10
						Audio.Channels[0].Sweep = value;
						Audio.HandleTone(0, 0);
                        break;
					case 0x11: // NR11
						Audio.Channels[0].SW = value;
						Audio.HandleTone(0, 1);
                        break;
					case 0x12: // NR12
						Audio.Channels[0].VolEnv = value;
						Audio.HandleTone(0, 2);
                        break;
					case 0x13: // NR13
						Audio.Channels[0].FrLo = value;
						Audio.HandleTone(0, 3);
						break;
					case 0x14: // NR14
						Audio.Channels[0].FrHi = value;
						Audio.HandleTone(0, 4);
						break;
					case 0x16: // NR21
						Audio.Channels[1].SW = value;
						Audio.HandleTone(1, 1);
                        break;
					case 0x17: // NR22
						Audio.Channels[1].VolEnv = value;
						Audio.HandleTone(1, 2);
                        break;
					case 0x18: // NR23
						Audio.Channels[1].FrLo = value;
						Audio.HandleTone(1, 3);
						break;
					case 0x19: // NR24
						Audio.Channels[1].FrHi = value;
						Audio.HandleTone(1, 4);
						break;
                    case 0x40: // LCDC
                        Video.LcdEnable = ((value >> 7) & 1) == 1;
                        Video.WindowTileMapSelect = ((value >> 6) & 1) == 1;
                        Video.WindowEnable = ((value >> 5) & 1) == 1;
                        Video.TileDataSelect = ((value >> 4) & 1) == 1;
                        Video.BgTileMapSelect = ((value >> 3) & 1) == 1;
                        Video.ObjSize = ((value >> 2) & 1) == 1;
                        Video.ObjEnable = ((value >> 1) & 1) == 1;
                        Video.BgEnable = (value & 1) == 1;
                        break;
                    case 0x41: // LCDC STAT
                        Video.CoincidenceInterrupt = ((value >> 6) & 1) == 1;
                        Video.OamInterrupt = ((value >> 5) & 1) == 1;
                        Video.VBlankInterrupt = ((value >> 4) & 1) == 1;
                        Video.HBlankInterrupt = ((value >> 3) & 1) == 1;
                        Video.CoincidenceFlag = ((value >> 2) & 1) == 1;
                        Video.ModeFlag = value & 3;
                        break;
                    case 0x42: // SCY
                        Video.SCY = value;
                        break;
                    case 0x43: // SCX
                        Video.SCX = value;
                        break;
                    case 0x45: // LYC
                        Video.LYC = value;
                        break;
                    case 0x46: // OAM DMA
                        int addr = value << 8;
                        for (int i = 0; i < 0x9F; i++ )
                            WriteByte(0xFE00 + i, ReadByte(addr + i));
                        break;
                    case 0x47: // BGP
                        Video.BGP = value;
                        break;
                    case 0x48: // OBP0
                        Video.OBP0 = value;
                        break;
                    case 0x49: // OBP1
                        Video.OBP1 = value;
                        break;
                    case 0x4A: // WY
                        Video.WY = value;
                        break;
                    case 0x4B: // WX
                        Video.WX = value;
                        break;
                    case 0x50: // ROM ENABLE
                        RomEnable = value > 0;
                        break;
                }
            } else if (address >= 0xFF80 && address <= 0xFFFE) {
                _hram[address - 0xFF80] = value;
            } else if (address == 0xFFFF) {
                // INTERRUPT ENABLE REGISTER
                Ie = value;
            } else {
                throw new Exception("Cannot handle memory address 0x" + address.ToString("X") + " (on write)");
            }
        }

    }
}
