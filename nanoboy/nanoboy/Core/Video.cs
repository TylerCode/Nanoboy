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
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace nanoboy.Core
{
    public sealed class Video
    {

        public IntPtr Frame
        {
            get
            {
                GCHandle handle = GCHandle.Alloc(frame, GCHandleType.Pinned);
                IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(frame, 0);
                return pointer;
                //return new Bitmap(160, 144, 160 * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, pointer);
            }
        }

        public bool FrameReady { get; set; }

        // #######################
        // #      Registers      #
        // #######################

        // LCD Control 0xFF40
        public bool LCDEnable { get; set; } // FALSE=OFF TRUE=ON
        public bool WindowTileMapSelect { get; set; } // FALSE=9800-9BFF TRUE=9C00-9FFF
        public bool WindowEnable { get; set; } // FALSE=OFF TRUE=ON
        public bool TileDataSelect { get; set; } // FALSE=8800-97FF TRUE=8000-8FFF
        public bool BackgroundTileMapSelect { get; set; } // FALSE=9800-9BFF TRUE=9C00-9FFF
        public bool ObjectSize { get; set; } // FALSE=8x8 TRUE=8x16
        public bool ObjectEnable { get; set; } // FALSE=OFF TRUE=ON
        public bool BackgroundEnable { get; set; } // FALSE=OFF TRUE=ON

        // STAT LCDC Status 0xFF41
        public bool CoincidenceInterrupt { get; set; }
        public bool OAMInterrupt { get; set; }
        public bool VBlankInterrupt { get; set; }
        public bool HBlankInterrupt { get; set; }
        public bool CoincidenceFlag { get; set; }
        public int ModeFlag { get; set; }

        // LCD Position and Scrolling
        public int SCY { get; set; } // 0xFF42
        public int SCX { get; set; } // 0xFF43
        public int LY { get; set; } // 0xFF44
        public int LYC { get; set; } // 0xFF45
        public int WY { get; set; } // 0xFF4A
        public int WX { get; set; } // 0xFF4B

        // LCD Monochrome Palettes
        public int BGP { get; set; } // 0xFF47
        public int OBP0 { get; set; } // 0xFF48
        public int OBP1 { get; set; } // 0xFF49

        // LCD Color Palettes (CGB only)
        // BCPS/BGPI 0xFF68
        public bool BackgroundPaletteAI { get; set; }
        public int BackgroundPaletteIndex { get; set; }
        // OCPS/OBPI 0xFF6A
        public bool ObjectPaletteAI { get; set; }
        public int ObjectPaletteIndex { get; set; }

        // VRAM Control
        public int VRAMBank { get; set; } // 0xFF4F

        // Memory
        private byte[,] vram;
        private byte[] oam;
        private byte[] pram1;
        private byte[] pram2;

        // Other
        private ROM rom;
        private Interrupt interrupt;
        private HDMA hdma;
        private int clock;
        private Color[] monochromepalette;
        private uint[] frame;

        private struct SpriteEntry
        {
            public int X;
            public int Y;
            public byte TileNumber;
            public byte Attributes;
            public int TableIndex;
        }

        public Video(Interrupt interrupt, HDMA hdma, ROM rom)
        {
            this.rom = rom;
            this.interrupt = interrupt;
            this.hdma = hdma;
            vram = new byte[2, 0x2000];
            oam = new byte[0xA0];
            pram1 = new byte[0x40];
            pram2 = new byte[0x40];
            clock = 0;
            monochromepalette = new Color[] {
            	Color.WhiteSmoke, 
            	Color.FromArgb(102, 102, 102),
            	Color.FromArgb(68, 68, 68),
            	Color.Black
            };
            frame = new uint[160 * 144];
            ModeFlag = 2;
            VRAMBank = 0;
        }

        /* Adapted from http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-GPU-Timings */
        public void Tick()
        {
            CoincidenceInterrupt = CoincidenceFlag && (LY == LYC) || !CoincidenceFlag && (LY != LYC);
            clock++;
            // Update hardware
            switch (ModeFlag) 
            {
                // Scanline (OAM)
                case 2:
                    if (OAMInterrupt) {
                        //interrupt.IF |= 2;
                    }
                    if (clock >= 80) {
                        ModeFlag = 3;
                        clock = 0;
                    }
                    break;
                // Scanline (VRAM)
                case 3:
                    if (clock >= 172) {
                        // Enter HBlank
                        if (hdma.IsHBlank) {
                            hdma.PerformHBlank();
                        }
                        ModeFlag = 0;
                        clock = 0;
                        // Render
                        RenderLine();
                    }
                    break;
                // HBlank
                case 0:
                    if (HBlankInterrupt) {
                        //interrupt.IF |= 2;
                    }
                    if (clock >= 204) {
                        clock = 0;
                        LY++;
                        if (LY == 144) {
                            // Enter VBlank
                            ModeFlag = 1;
                            // Eventually output?
                            FrameReady = true;
                        } else {
                            // Next scanline
                            ModeFlag = 2;
                        }
                    }
                    break;
                // VBlank
                case 1:
                    if (VBlankInterrupt) {
                        //interrupt.IF |= 2;
                    }
                    if (clock >= 456) {
                        clock = 0;
                        LY++;
                        if (LY == 145) {
                            // Enter VBlank Interrupt
                            interrupt.IF |= 1;
                        }
                        if (LY > 153) {
                            // Restart scanning modes
                            ModeFlag = 2;
                            LY = 0;
                        }
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderLine()
        {
            if (LCDEnable && LY <= 144) {
                if (BackgroundEnable) {
                    RenderBackgroundLine();
                }
                if (WindowEnable && LY >= WY) {
                    RenderWindowLine();
                }
                if (ObjectEnable) {
                    RenderSpriteLine();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderBackgroundLine()
        {
            int mapaddress = BackgroundTileMapSelect ? 0x1C00 : 0x1800;
            int wrapx = SCX + 160 > 256 ? (SCX + 160) - 256 : 0;
            int wrapy = (LY + SCY) % 256;
            int displacementy = wrapy % 8;
            int row = (wrapy - displacementy) / 8;
            uint[] mapline = RenderTilemapLine(mapaddress, row, displacementy);
            Buffer.BlockCopy(mapline, SCX * 4, frame, LY * 160 * 4, (160 - wrapx) * 4);
            Buffer.BlockCopy(mapline, 0, frame, (LY * 160 + 160 - wrapx) * 4, wrapx * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderWindowLine()
        {
            int mapaddress = WindowTileMapSelect ? 0x1C00 : 0x1800;
            int difference = LY - WY;
            int displacementy = difference % 8;
            int row = (difference - displacementy) / 8;
            uint[] mapline = RenderTilemapLine(mapaddress, row, displacementy);
            int wx = WX - 7;
            if (wx < 0) { // move to left
                Buffer.BlockCopy(mapline, wx * -4, frame, LY * 160 * 4, (160 - (wx * -1)) * 4);
            } else { // move to right
                Buffer.BlockCopy(mapline, 0,  frame, (LY * 160 + wx) * 4, (160 - wx) * 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint[] RenderTilemapLine(int mapaddress, int row, int displacement)
        {
            uint[] mapline = new uint[256];
            for (int x = 0; x < 32; x++) {
                int address = mapaddress + 32 * row + x;
                int tileindex = vram[0, address];
                int tileaddress;
                uint[] tiledata;
                if (TileDataSelect) {
                    tileaddress = tileindex * 16;
                } else {
                    sbyte signedindex = (sbyte)tileindex;
                    tileaddress = 0x1000 + signedindex * 16;
                }
                if (rom.HasColorFeatures) {
                    int tileattributes = vram[1, address];
                    tiledata = ReadTileLineColor(tileaddress, displacement, tileattributes, pram1);
                } else {
                    tiledata = ReadTileLine(tileaddress, displacement, BGP);
                }
                Buffer.BlockCopy(tiledata, 0, mapline, x * 32, 32);
            }
            return mapline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderSpriteLine()
        {
            List<SpriteEntry> entries = new List<SpriteEntry>();
            int tolerance = ObjectSize ? 16 : 8;

            // Get all entries from OAM ram
            for (int i = 0; i < 40; i++) {
                SpriteEntry entry = new SpriteEntry();
                entry.Y = oam[i * 4] - 16;
                entry.X = oam[i * 4 + 1] - 8;
                entry.TileNumber = oam[i * 4 + 2];
                entry.Attributes = oam[i * 4 + 3];
                entry.TableIndex = i;
                // Is this sprite in the renderable area?
                if (entry.X < 160 && entry.Y < 144 && entry.Y > LY - tolerance && entry.Y < LY + 1) {
                    entries.Add(entry);
                }
            }
            // TODO: Sort (out)
            // Render
            foreach (SpriteEntry entry in entries) {
                int displacementy = LY - entry.Y;
                int colorpalette = entry.Attributes & 7;
                int tilenumber = entry.TileNumber;
                int tilebank = (entry.Attributes >> 3) & 1;
                int palette = (entry.Attributes & 0x10) == 0x10 ? OBP1 : OBP0;
                bool flipx = (entry.Attributes & 0x20) == 0x20;
                bool flipy = (entry.Attributes & 0x40) == 0x40;
                bool behind = (entry.Attributes & 0x80) == 0x80;
                uint[] tileline;
                if (ObjectSize) { // size is 8 * 16
                    if (displacementy > 7) {
                        // use the second tile
                        tilenumber |= 1;
                        displacementy -= 8;
                    } else {
                        // use the first tile
                        tilenumber &= 0xFE;
                    }
                }
                if (flipy) {
                    displacementy = 7 - displacementy;
                }
                if (rom.HasColorFeatures) {
                    int attributes = colorpalette | (tilebank << 3);
                    tileline = ReadTileLineColor(tilenumber * 16, displacementy, attributes, pram2, true);
                } else {
                    tileline = ReadTileLine(tilenumber * 16, displacementy, palette, true);
                }
                if (flipx) {
                    Array.Reverse(tileline);
                }
                DrawTile(tileline, entry.X, LY);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawTile(uint[] tileline, int x, int y, bool behindbackground = false)
        {
            int start = x < 0 ? 0 - x : 0;
            int length = x > 152 ? 160 - x : 8;
            for (int i = start; i < length - start; i++) {
                if (tileline[i] != 0) {
                    try {
                        frame[y * 160 + x + i - start] = tileline[i];
                    } catch (Exception ex) {
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint[] ReadTileLine(int tileaddress, int displacement, int palette, bool alpha = false)
        {
            byte byte1 = vram[0, tileaddress + displacement * 2];
            byte byte2 = vram[0, tileaddress + displacement * 2 + 1];
            uint[] data = new uint[8];
            for (int x = 0; x < 8; x++) {
                int color = (((byte2 >> (7 - x)) & 1) << 1) + ((byte1 >> (7 - x)) & 1);
                if (!alpha || color != 0) {
                    data[x] = GetPaletteEntry(color, palette);
                }
            }
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint[] ReadTileLineColor(int tileaddress, int displacement, int attributes, byte[] paletteram, bool alpha = false)
        {
            int bank = (attributes >> 3) & 1;
            bool hflip = (attributes & 0x20) == 0x20;
            bool vflip = (attributes & 0x40) == 0x40;
            int palette = attributes & 7;
            byte byte1;
            byte byte2;
            uint[] data = new uint[8];
            if (vflip) {
                displacement = 7 - displacement;
            }
            byte1 = vram[bank, tileaddress + displacement * 2];
            byte2 = vram[bank, tileaddress + displacement * 2 + 1];
            for (int x = 0; x < 8; x++) {
                int color = (((byte2 >> (7 - x)) & 1) << 1) + ((byte1 >> (7 - x)) & 1);
                if (!alpha || color != 0) {
                    data[x] = GetPaletteEntryColor(color, paletteram, palette);
                }
            }
            if (hflip) {
                Array.Reverse(data);
            }
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetPaletteEntry(int index, int palette)
        {
            return (uint)monochromepalette[(palette >> (index * 2)) & 3].ToArgb();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetPaletteEntryColor(int index, byte[] ram, int palette)
        {
            int position = palette * 8 + index * 2;
            uint entry = (uint)(ram[position] |
                               (ram[position + 1] << 8));
            uint value = 0xFF000000;
            value |= ((entry & 0x1F) * 8) << 16;
            value |= (((entry >> 5) & 0x1F) * 8) << 8;
            value |= ((entry >> 10) & 0x1F) * 8;
            return value;
        }

        public byte ReadVRAM(int address)
        {
            return vram[VRAMBank, address];
        }

        public void WriteVRAM(int address, byte value)
        {
            vram[VRAMBank, address] = value;
        }

        public byte ReadOAM(int address)
        {
            return oam[address];
        }

        public void WriteOAM(int address, byte value)
        {
            oam[address] = value;
        }

        public byte ReadPRAM(int index)
        {
            if (index == 0) {
                return pram1[BackgroundPaletteIndex & 0x3F];
            }
            return pram2[ObjectPaletteIndex & 0x3F];
        }

        public void WritePRAM(int index, byte value)
        {
            if (index == 0) {
                pram1[BackgroundPaletteIndex & 0x3F] = value;
                if (BackgroundPaletteAI) {
                    BackgroundPaletteIndex++;
                }
            } else {
                pram2[ObjectPaletteIndex] = value;
                if (ObjectPaletteAI) {
                    ObjectPaletteIndex++;
                }
            }
        }

    }
}
