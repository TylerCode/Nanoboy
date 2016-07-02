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
    public sealed class HDMA
    {
        // #######################
        // #      Registers      #
        // #######################
        public int SourceAddress;
        public int DestinationAddress;
        public int Length;
        public bool IsHBlank;

        private Memory memory;
        private bool inhblankdma;
        private int hblankremaining;
        private int hblankprogress;

        public HDMA(Memory memory)
        {
            this.memory = memory;
        }

        public void PerformGeneralPurpose()
        {
            // Copy data
            for (int i = 0; i < Length; i++) {
                memory.WriteByte(0x8000 + DestinationAddress, memory.ReadByte(SourceAddress));
            }

            // 0xFF55 now contains 0xFF
            IsHBlank = true;
            Length = 0x7F;
        }

        public void PerformHBlank()
        {
            if (!inhblankdma) {
                inhblankdma = true;
                hblankremaining = Length;
                hblankprogress = 0;
            }

            for (int i = 0; i < 0x10; i++) {
                memory.WriteByte(0x8000 + DestinationAddress + hblankprogress + i,
                                 memory.ReadByte(SourceAddress + hblankprogress + i));
            }

            hblankprogress += 0x10;

            if (hblankprogress == hblankremaining) {
                IsHBlank = false;
                inhblankdma = false;
            }
        }

    }
}
