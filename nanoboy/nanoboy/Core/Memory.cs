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
using nanoboy.Core.Audio;
using nanoboy.Core.Audio.Backend.OpenAL;

namespace nanoboy.Core
{
    public sealed class Memory : IMemoryDevice, IDisposable
    {
        public Interrupt Interrupt;
        public Video Video;
        public Audio.Audio Audio;
        public Joypad Joypad;
        public Timer Timer;
        private CPU cpu;
        private HDMA hdma;
        private IMemoryDevice mbc;
        private ROM rom;
        private byte[,] wram;
        private int wrambank;
        private byte[] hram;
        private ALSoundOut soundout;
        private ISerialDevice serial;

        public Memory(CPU cpu, ROM rom)
        {
            this.cpu = cpu;
            this.mbc = rom.MBC;
            this.rom = rom;
            hdma = new HDMA(this);
            wram = new byte[8, 0x1000];
            wrambank = 1;
            hram = new byte[0x7F];
            Audio = new Audio.Audio();
            soundout = new ALSoundOut(Audio);
            serial = new SerialConsole();
            Interrupt = new Interrupt(cpu);
            Video = new Video(Interrupt, hdma, rom);
            Joypad = new Joypad(Interrupt);
            Timer = new Timer(Interrupt);
        }

        public byte ReadByte(int address)
        {
            if (address <= 0x7FFF) {
                return mbc.ReadByte(address);
            } else if (address <= 0x9FFF) {
                return Video.ReadVRAM(address - 0x8000);
            } else if (address <= 0xBFFF) {
                return mbc.ReadByte(address);
            } else if (address <= 0xCFFF) {
                return wram[0, address - 0xC000];
            } else if (address <= 0xDFFF) {
                return wram[wrambank, address - 0xD000];
            } else if (address <= 0xEFFF) {
                return wram[0, address - 0xE000];
            } else if (address <= 0xFDFF) {
                return wram[wrambank, address - 0xF000];
            } else if (address <= 0xFE9F) {
                return Video.ReadOAM(address - 0xFE00);
            } else if (address <= 0xFEFF) {
                // Not usable
                return 0;
            } else if (address <= 0xFF7F) {
                int value = 0;
                switch (address - 0xFF00) 
                {
                    case 0x00: // Joypad
                        value = Joypad.SelectButtonKeys ?  0x00 : 0x20;
                        value += Joypad.SelectDirectionKeys ? 0x00 : 0x10;
                        if (Joypad.SelectButtonKeys) {
                            value += Joypad.KeyB ? 0x02 : 0x00;
                            value += Joypad.KeyA ? 0x01 : 0x00;
                        } else {
                            value += Joypad.KeyLeft ? 0x02 : 0x00;
                            value += Joypad.KeyRight ? 0x01 : 0x00;
                        }
                        if (Joypad.SelectDirectionKeys) {
                            value += Joypad.KeyDown ? 0x08 : 0x00;
                            value += Joypad.KeyUp ? 0x04 : 0x00;
                        } else {
                            value += Joypad.KeyStart ? 0x08 : 0x00;
                            value += Joypad.KeySelect ? 0x04 : 0x00;
                        }
                        return (byte)value;
                    case 0x01: // SB Serial transfer data
                        return serial.Read();
                    case 0x04: // Divider Register
                        return (byte)Timer.DIV;
                    case 0x05: // Timer Counter
                        return (byte)Timer.TIMA;
                    case 0x06: // Timer Modulo
                        return (byte)Timer.TMA;
                    case 0x07: // Timer Control
                        return (byte)Timer.TAC;
                    case 0x0F: // Interrupt Flag
                        return (byte)Interrupt.IF;
                    case 0x10: // NR10 Channel 1 Sweep register
                        value = Audio.Channel1.SweepShift |
                                ((int)Audio.Channel1.SweepDirection << 3) |
                                (Audio.Channel1.SweepTime << 4);
                        return (byte)value;
                    case 0x11: // NR11 Channel 1 Sound length/Wave pattern duty
                        value = Audio.Channel1.SoundLengthData |
                                (Audio.Channel1.WavePatternDuty << 6);
                        return (byte)value;
                    case 0x12: // NR12 Channel 1 Volume Envelope
                        value = Audio.Channel1.EnvelopeSweep |
                                ((int)Audio.Channel1.EnvelopeDirection << 3) |
                                (Audio.Channel1.Volume << 4);
                        return (byte)value;
                    case 0x14: // NR14 Channel 1 Frequency hi
                        return (byte)(Audio.Channel1.StopOnLengthExpired ? 0x40 : 0x00);
                    case 0x16: // NR21 Channel 2 Sound length/Wave pattern duty
                        value = Audio.Channel2.SoundLengthData |
                                (Audio.Channel2.WavePatternDuty << 6);
                        return (byte)value;
                    case 0x17: // NR22 Channel 2 Volume Envelope
                        value = Audio.Channel2.EnvelopeSweep |
                                ((int)Audio.Channel2.EnvelopeDirection << 3) |
                                (Audio.Channel2.Volume << 4);
                        return (byte)value;
                    case 0x19: // NR24 Channel 2 Frequency hi
                        return (byte)(Audio.Channel2.StopOnLengthExpired ? 0x40 : 0x00);
                    case 0x1A: // NR30 Channel 3 Sound on/off
                        return (byte)(Audio.Channel3.On ? 0x80 : 0x00);
                    case 0x1B: // NR31 Channel 3 Sound Length
                        return (byte)Audio.Channel3.SoundLengthData;
                    case 0x1C: // NR32 Channel 3 Select output level
                        return (byte)(Audio.Channel3.OutputLevel << 5);
                    case 0x1E: // NR33 Channel 3 Frequency hi
                        return (byte)(Audio.Channel3.StopOnLengthExpired ? 0x40 : 0x00);
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23: throw new NotImplementedException();
                    // FF30-FF3F Wave Pattern RAM
                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                    case 0x38:
                    case 0x39:
                    case 0x3A:
                    case 0x3B:
                    case 0x3C:
                    case 0x3D:
                    case 0x3E:
                    case 0x3F:
                        value = Audio.Channel3.WaveRAM[(address & 0xF) * 2 + 1] |
                                (Audio.Channel3.WaveRAM[(address & 0xF) * 2] << 4);
                        return (byte)value;
                    case 0x40: // LCDC
                        value = Video.LCDEnable ? 0x80 : 0x0;
                        value += Video.WindowTileMapSelect ? 0x40 : 0x0;
                        value += Video.WindowEnable ? 0x20 : 0x0;
                        value += Video.TileDataSelect ? 0x10 : 0x0;
                        value += Video.BackgroundTileMapSelect ? 0x08 : 0x0;
                        value += Video.ObjectSize ? 0x04 : 0x0;
                        value += Video.ObjectEnable ? 0x02 : 0x0;
                        value += Video.BackgroundEnable ? 0x01 : 0x0;
                        return (byte)value;
                    case 0x41: // LCDC STAT
                        value = Video.CoincidenceInterrupt ? 0x40 : 0x0;
                        value += Video.OAMInterrupt ? 0x20 : 0x0;
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
                    case 0x4D: // Prepare Speed Switch
                        value = cpu.IsDoubleSpeed ? 0x80 : 0x00;
                        value |= cpu.PrepareSpeedSwitch ? 1 : 0;
                        return (byte)value;
                    case 0x4F: // VRAM bank
                        return (byte)Video.VRAMBank;
                    case 0x51: // HDMA Source, High
                        return (byte)(hdma.SourceAddress >> 8);
                    case 0x52: // HDMA Source, Low
                        return (byte)hdma.SourceAddress;
                    case 0x53: // HDMA Destination, High
                        return (byte)(hdma.DestinationAddress >> 8);
                    case 0x54: // HDMA Destination, Low
                        return (byte)hdma.DestinationAddress;
                    case 0x55: // HDMA Length/Mode/Start
                        value = (hdma.Length / 0x10 - 1) |
                                (hdma.IsHBlank ? 0x80 : 0x00);
                        return (byte)value;
                    case 0x68: // Background Palette Index
                        value = Video.BackgroundPaletteIndex;
                        value |= Video.BackgroundPaletteAI ? 0x80 : 0x00;
                        return (byte)value;
                    case 0x69: // Background Palette Data
                        return Video.ReadPRAM(0);
                    case 0x6A: // Object Palette Index
                        value = Video.ObjectPaletteIndex;
                        value |= Video.ObjectPaletteAI ? 0x80 : 0x00;
                        return (byte)value;
                    case 0x6B: // Object Palette Data
                        return Video.ReadPRAM(1);
                    case 0x70: // WRAM bank
                        return (byte)wrambank;
                    default:
                        //throw new Exception(string.Format("Unknown IO port at 0x{0:X} (READ)", address));
                        return 0;
                }
            } else if (address <= 0xFFFE) {
                return hram[address - 0xFF80];
            } else {
                return (byte)Interrupt.IE;
            }
        }

        public void WriteByte(int address, byte value)
        {
            if (address <= 0x7FFF) {
                mbc.WriteByte(address, value);
            } else if (address <= 0x9FFF) {
                Video.WriteVRAM(address - 0x8000, value);
            } else if (address <= 0xBFFF) {
                mbc.WriteByte(address, value);
            } else if (address <= 0xCFFF) {
                wram[0, address - 0xC000] = value;
            } else if (address <= 0xDFFF) {
                wram[wrambank, address - 0xD000] = value;
            } else if (address <= 0xEFFF) {
                wram[0, address - 0xE000] = value;
            } else if (address <= 0xFDFF) {
                wram[wrambank, address - 0xF000] = value;
            } else if (address <= 0xFE9F) {
                Video.WriteOAM(address - 0xFE00, value);
            } else if (address <= 0xFEFF) {
                // Not usable
            } else if (address <= 0xFF7F) {
                switch (address - 0xFF00) {
                    case 0x00: // Joypad
                        Joypad.SelectButtonKeys = ((value >> 5) & 1) == 0;
                        Joypad.SelectDirectionKeys = ((value >> 4) & 1) == 0;
                        break;
                    case 0x01: // SB Serial transfer data
                        serial.Write(value);
                        break;
                    case 0x02: // SC Serial Transfer Control
                        if ((value & 0x80) == 0x80) {
                            serial.Start();
                        }
                        break;
                    case 0x04: // Divider Register
                        Timer.DIV = 0;
                        break;
                    case 0x05: // Timer Counter
                        Timer.TIMA = value;
                        break;
                    case 0x06: // Timer Modulo
                        Timer.TMA = value;
                        break;
                    case 0x07: // Timer Control
                        Timer.TAC = value;
                        break;
                    case 0x0F: // Interrupt Flag
                        Interrupt.IF = value;
                        break;
                    case 0x10: // NR10 Channel 1 Sweep register
                        Audio.Channel1.SweepShift = value & 7;
                        Audio.Channel1.SweepDirection = (QuadChannel.SweepMode)((value >> 3) & 1);
                        Audio.Channel1.SweepTime = (value >> 4) & 7;
                        break;
                    case 0x11: // NR11 Channel 1 Sound length/Wave pattern duty
                        Audio.Channel1.SoundLengthData = value & 0x3F;
                        Audio.Channel1.WavePatternDuty = (value >> 6) & 3;
                        break;
                    case 0x12: // NR12 Channel 1 Volume Envelope
                        Audio.Channel1.EnvelopeSweep = value & 7;
                        Audio.Channel1.EnvelopeDirection = (QuadChannel.EnvelopeMode)((value >> 3) & 1);
                        Audio.Channel1.Volume = (value >> 4) & 0xF;
                        break;
                    case 0x13: // NR13 Channel 1 Frequency lo
                        Audio.Channel1.Frequency = (Audio.Channel1.Frequency & 0x700) | value;
                        break;
                    case 0x14: // NR14 Channel 1 Frequency hi
                        Audio.Channel1.Frequency = (Audio.Channel1.Frequency & 0xFF) | ((value & 7) << 8);
                        Audio.Channel1.StopOnLengthExpired = (value & 0x40) == 0x40;
                        if ((value & 0x80) == 0x80) {
                            Audio.Channel1.Restart();
                        }
                        break;
                    case 0x16: // NR16 Channel 2 Sound length/Wave pattern duty
                        Audio.Channel2.SoundLengthData = value & 0x3F;
                        Audio.Channel2.WavePatternDuty = (value >> 6) & 3;
                        break;
                    case 0x17: // NR17 Channel 2 Volume Envelope
                        Audio.Channel2.EnvelopeSweep = value & 7;
                        Audio.Channel2.EnvelopeDirection = (QuadChannel.EnvelopeMode)((value >> 3) & 1);
                        Audio.Channel2.Volume = (value >> 4) & 0xF;
                        break;
                    case 0x18: // NR18 Channel 2 Frequency lo
                        Audio.Channel2.Frequency = (Audio.Channel2.Frequency & 0x700) | value;
                        break;
                    case 0x19: // NR19 Channel 2 Frequency hi
                        Audio.Channel2.Frequency = (Audio.Channel2.Frequency & 0xFF) | ((value & 7) << 8);
                        Audio.Channel2.StopOnLengthExpired = (value & 0x40) == 0x40;
                        if ((value & 0x80) == 0x80) {
                            Audio.Channel2.Restart();
                        }
                        break;
                    case 0x1A: // NR30 Channel 3 Sound on / off
                        Audio.Channel3.On = (value & 0x80) == 0x80;
                        break;
                    case 0x1B: // NR31 Channel 3 Sound Length
                        Audio.Channel3.SoundLengthData = value;
                        break;
                    case 0x1c: // NR32 Channel 3 Select output level
                        Audio.Channel3.OutputLevel = (value >> 5) & 3;
                        break;
                    case 0x1D: // NR33 Channel 3 Frequency lo
                        Audio.Channel3.Frequency = (Audio.Channel3.Frequency & 0x700) | value;
                        break;
                    case 0x1E: // NR34 Channel 3 Frequency hi
                        Audio.Channel3.Frequency = (Audio.Channel3.Frequency & 0xFF) | ((value & 7) << 8);
                        Audio.Channel3.StopOnLengthExpired = (value & 0x40) == 0x40;
                        if ((value & 0x80) == 0x80) {
                            Audio.Channel3.Restart();
                        }
                        break;
                    case 0x20: // NR41 Channel 4 Sound Length
                        Audio.Channel4.SoundLengthData = value;
                        break;
                    case 0x21: // NR42 Channel 4 Volume Envelope
                        Audio.Channel4.EnvelopeSweep = value & 7;
                        Audio.Channel4.EnvelopeDirection = (NoiseChannel.EnvelopeMode)((value >> 3) & 1);
                        Audio.Channel4.Volume = (value >> 4) & 0xF;
                        break;
                    case 0x22: // NR43 Channel 4 Polynomial Counter
                        Audio.Channel4.ClockFrequency = value >> 4;
                        Audio.Channel4.CounterStep = (value & 8) == 8;
                        Audio.Channel4.Counter = Audio.Channel4.CounterStep ? 0x7F : 0x7FFF;
                        Audio.Channel4.DividingRatio = value & 7;
                        break;
                    case 0x23: // NR44 Channel 4 Counter/consecutive; Initial
                        Audio.Channel4.StopOnLengthExpired = (value & 0x40) == 0x40;
                        if ((value & 0x80) == 0x80) {
                            Audio.Channel4.Restart();
                        }
                        break;
                    // FF30-FF3F Wave Pattern RAM
                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                    case 0x38:
                    case 0x39:
                    case 0x3A:
                    case 0x3B:
                    case 0x3C:
                    case 0x3D:
                    case 0x3E:
                    case 0x3F:
                        Audio.Channel3.WaveRAM[(address & 0xF) * 2] = (byte)(value >> 4);
                        Audio.Channel3.WaveRAM[(address & 0xF) * 2 + 1] = (byte)(value & 0xF); 
                        break;
                    case 0x40: // LCDC
                        Video.LCDEnable = ((value >> 7) & 1) == 1;
                        Video.WindowTileMapSelect = ((value >> 6) & 1) == 1;
                        Video.WindowEnable = ((value >> 5) & 1) == 1;
                        Video.TileDataSelect = ((value >> 4) & 1) == 1;
                        Video.BackgroundTileMapSelect = ((value >> 3) & 1) == 1;
                        Video.ObjectSize = ((value >> 2) & 1) == 1;
                        Video.ObjectEnable = ((value >> 1) & 1) == 1;
                        Video.BackgroundEnable = (value & 1) == 1;
                        break;
                    case 0x41: // LCDC STAT
                        Video.CoincidenceInterrupt = ((value >> 6) & 1) == 1;
                        Video.OAMInterrupt = ((value >> 5) & 1) == 1;
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
                        int address2 = value << 8;
                        for (int i = 0; i < 0x9F; i++) {
                            WriteByte(0xFE00 + i, ReadByte(address2 + i));
                        }
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
                    case 0x4D: // Prepare Speed Switch
                        cpu.PrepareSpeedSwitch = (value & 1) == 1;
                        break;
                    case 0x4F: // VRAM bank
                        if (rom.HasColorFeatures) {
                            Video.VRAMBank = value & 1;
                        }
                        break;
                    case 0x51: // HDMA Source, High
                        hdma.SourceAddress = hdma.SourceAddress & 0xFF | (value << 8);
                        break;
                    case 0x52: // HDMA Source, Low
                        hdma.SourceAddress = hdma.SourceAddress & 0xFF00 | (value & 0xF0);
                        break;
                    case 0x53: // HDMA Destination, High
                        hdma.DestinationAddress = hdma.DestinationAddress & 0xFF | ((value & 0x1F) << 8);
                        break;
                    case 0x54: // HDMA Destination, Low
                        hdma.DestinationAddress = hdma.DestinationAddress & 0xFF00 | (value & 0xF0);
                        break;
                    case 0x55: // HDMA Length/Mode/Start
                        hdma.Length = ((value & 0x7F) + 1) * 0x10;
                        hdma.IsHBlank = (value & 0x80) == 0x80;
                        if (!hdma.IsHBlank) {
                            hdma.PerformGeneralPurpose();
                        }
                        break;
                    case 0x68: // Background Palette Index
                        Video.BackgroundPaletteIndex = value & 0x3F;
                        Video.BackgroundPaletteAI = (value & 0x80) == 0x80;
                        break;
                    case 0x69: // Background Palette Data
                        Video.WritePRAM(0, value);
                        break;
                    case 0x6A: // Object Palette Index
                        Video.ObjectPaletteIndex = value & 0x3F;
                        Video.ObjectPaletteAI = (value & 0x80) == 0x80;
                        break;
                    case 0x6B: // Object Palette Data
                        Video.WritePRAM(1, value);
                        break;
                    case 0x70: // WRAM bank
                        if (rom.HasColorFeatures) {
                            if (value == 0) {
                                value = 1;
                            }
                            wrambank = value & 7;
                        }
                        break;
                    default:
                        //throw new Exception(string.Format("Unknown IO port at 0x{0:X} (WRITE)", address));
                        break;
                }
            } else if (address <= 0xFFFE) {
                hram[address - 0xFF80] = value;
            } else {
                Interrupt.IE = value;
            }
        }

        public void Dispose()
        {
            soundout.Dispose();
        }
    }
}
