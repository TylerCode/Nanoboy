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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GeekBoy
{
    /// <summary>
	/// The class "Video" represents the store for the video i/o registers and also the video renderer.
	/// </summary>
    public class Video
    {
        // LCD Control 0xFF40
        public bool LcdEnable { get; set; } // FALSE=OFF TRUE=ON
        public bool WindowTileMapSelect { get; set; } // FALSE=9800-9BFF TRUE=9C00-9FFF
        public bool WindowEnable { get; set; } // FALSE=OFF TRUE=ON
        public bool TileDataSelect { get; set; } // FALSE=8800-97FF TRUE=8000-8FFF
        public bool BgTileMapSelect { get; set; } // FALSE=9800-9BFF TRUE=9C00-9FFF
        public bool ObjSize { get; set; } // FALSE=8x8 TRUE=8x16
        public bool ObjEnable { get; set; } // FALSE=OFF TRUE=ON
        public bool BgEnable { get; set; } // FALSE=OFF TRUE=ON

        // STAT LCDC Status 0xFF41
        public bool CoincidenceInterrupt { get; set; }
        public bool OamInterrupt { get; set; }
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

        // Video & OAM RAM
        public byte[] VRAM { get; set; }
        public byte[] OAM { get; set; }

        // Buffers
        public Bitmap Buffer { get {
            GCHandle _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            IntPtr _pointer = Marshal.UnsafeAddrOfPinnedArrayElement(_buffer, 0);
            return new Bitmap(160, 144, 160 * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, _pointer);
        } }
        private uint[] _buffer;

        public MemoryRouter MemoryRouter;

        // Cycling
        public int ModeClock = 0;

        public Color[] Colors { get; set; }

        private struct OamEntry
        {
            public int x;
            public int y;
            public byte tileNumber;
            public byte attributes;
            public int tableIndex;
        }

        public Video()
        {
            VRAM = new byte[0x2000];
            OAM = new byte[0xA0];
            Colors = new Color[] {
            	Color.White, 
            	Color.FromArgb(102, 102, 102),
            	Color.FromArgb(68, 68, 68),
            	Color.Black
            };
            _buffer = new uint[160 * 144];
        }

        public void RenderLine()
        {
            if (LcdEnable)
            {
                if (BgEnable) RenderBg();
                if (WindowEnable) RenderWindow();
                if (ObjEnable) RenderObj();
            }
        }

        private void RenderBg() 
        {
            int SLY = LY + SCY; // Scrolled Y
            if (SLY >= 256) SLY -= 256;  // Adjust scrolled Y
            int ty = SLY % 8; // y-Axis in the tile
            int y = (SLY - ty) / 8; // Current column
            int tmap = BgTileMapSelect ? 0x1C00 : 0x1800; // Tilemap address
            uint[] lbuffer = new uint[256]; // Buffer for tilemap line
            int overflow = SCX + 160 - 256; // How much do we overflow in the tilemap?
            tmap += y * 32; // Render from the current column

            if (overflow < 0) overflow = 0; 

            for (int x = 0; x < 32; x++)
            {
                int addr; // Tile address
                int value = VRAM[tmap + x]; // Tile id
                if (!TileDataSelect)
                {
                    if (value > 0x7F)
                        value -= 256;
                    addr = 0x1000 + value * 16;
                } else {
                    addr = value * 16;
                }
                DrawLine(ref lbuffer, GetTileLine(addr, ty, BGP), x * 8, 0);
            }

            System.Buffer.BlockCopy(lbuffer, SCX * 4, _buffer, LY * 160 * 4, (160 - overflow) * 4);
            System.Buffer.BlockCopy(lbuffer, 0, _buffer, (LY * 160 + 160 - overflow) * 4, overflow * 4);
        }

        private void RenderWindow() 
        {
            if (LY >= WY) // Have we reached the window starting point, yet?
            {
                int ty = (LY - WY) % 8; // y-Axis in the tile
                int y = ((LY - WY) - ty) / 8; // Current column
                int tmap = WindowTileMapSelect ? 0x1C00 : 0x1800; // Tilemap address
                uint[] lbuffer = new uint[256]; // Buffer for tilemap line
                int _wx = WX - 7; // Real window X
                tmap += y * 32; // Render from the current column

                for (int x = 0; x < 32; x++)
                {
                    int addr; // Tile address
                    int value = VRAM[tmap + x]; // Tile id
                    if (!TileDataSelect)
                    {
                        if (value > 0x7F)
                            value -= 256;
                        addr = 0x1000 + value * 16;
                    } else {
                        addr = value * 16;
                    }
                    DrawLine(ref lbuffer, GetTileLine(addr, ty, BGP), x * 8, 0);
                }

                if (_wx < 0) // Move window to left
                {
                    System.Buffer.BlockCopy(lbuffer, _wx * -4, _buffer, LY * 160 * 4, (160 - (_wx * -1)) * 4);
                } else { // Move window to right, or don't move at all
                    System.Buffer.BlockCopy(lbuffer, 0, _buffer, (LY * 160 + _wx) * 4, (160 - _wx) * 4);
                }
            }
        }

        private void RenderObj() 
        { 
            OamEntry[] entries = new OamEntry[40];
            List<OamEntry> list = new List<OamEntry>();
            int tolerance = ObjSize ? 16 : 8;

            for (int i = 0; i < 40; i++)
            {
                entries[i].y = OAM[i * 4] - 16; 
                entries[i].x = OAM[i * 4 + 1] - 8; 
                entries[i].tileNumber = OAM[i * 4 + 2];
                entries[i].attributes = OAM[i * 4 + 3];
                entries[i].tableIndex = i;
            }

            // Throw out all OAMs that are outside the screen or renderable area
            for (int i = 0; i < 40; i++)
            {
                if (entries[i].x < 160 && entries[i].y < 144 && entries[i].y > LY - tolerance && entries[i].y < LY + 1)
                {
                    list.Add(entries[i]);
                }
            }

            entries = list.ToArray();
            list.Clear();

            // Order OAMs by x-Axis
            for (int x = 0; x < 160; x++)
            {
                foreach (OamEntry en in entries)
                {
                    if (en.x == x)
                        list.Add(en);
                }
            }

            entries = list.ToArray();
            list.Clear();

            // DRAW!!!111elf (experimental)
            for (int i = 0; i < entries.Length; i++)
            {
                int pal = (entries[i].attributes & 0x10) == 0x10 ? OBP1 : OBP0;
                int ty = LY - entries[i].y;
                int tn = entries[i].tileNumber;
                bool behindBg = (entries[i].attributes & 0x80) == 0x80;
                uint[] data;

                if (ObjSize) // obj is 8x16
                {
                    if (ty > 7) // second tile
                    {
                        tn = entries[i].tileNumber | 1;
                        ty -= 8;
                    } else { // first tile
                        tn = entries[i].tileNumber & 0xFE;
                    }
                }

                if ((entries[i].attributes & 0x40) == 0x40) // Flip Y
                    ty = 7 - ty;
                data = GetTileLine(tn * 16, ty, pal, true);
                if ((entries[i].attributes & 0x20) == 0x20) // Flip X
                    Array.Reverse(data);
                DrawLine(ref _buffer, data, entries[i].x, LY, 160 - (uint)entries[i].x, behindBg, pal); // Draw Magic!
            }
        }

        private uint[] GetTileLine(int addr, int y, int pal, bool transparent = false)
        {
            uint[] tbuffer = new uint[8]; // Tile line  buffer
            byte b1 = VRAM[addr + y * 2]; // Line encoding byte #1
            byte b2 = VRAM[addr + y * 2 + 1]; // Line encoding byte #2

            for (int x = 0; x < 8; x++ )
            {
                int color = (((b2 >> (7 - x)) & 1) << 1) + ((b1 >> (7 - x)) & 1);
                if (color != 0 || !transparent)
                    tbuffer[x] = ColorFromPal(color, pal);
            }

            return tbuffer;
        }

        private void DrawLine(ref uint[] dst, uint[] src, int x, int y, uint maxLen = 0xFFFFFFFF, bool behindBg = false, int pal = 0x0)
        {
            for (int i = 0; i < src.Length & i < maxLen; i++)
            {
            	if (src[i] != 0 && !( behindBg && src[i] != ColorFromPal(0, pal) && dst[y * 160 + x + i] != ColorFromPal(0, BGP )))
                    dst[y * 160 + x + i] = src[i];
            }
        }

        private uint ColorFromPal(int color, int pal)
        {
            return (uint)Colors[(pal >> (color * 2)) & 3].ToArgb();
        }

    }
}
