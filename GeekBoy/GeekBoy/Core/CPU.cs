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
using System.Runtime.CompilerServices;
using System.Diagnostics;
using GeekBoy.Observer;

namespace GeekBoy.Core
{
    /// <summary>
	/// The class "Cpu" represents the new cpu emulation core with delegates.
	/// </summary>
    public class Cpu : Subscribable
    {
        // Registers
        private int _a, _f, _b, _c, _d, _e, _h, _l;
        public int Sp { get; set; }
        public int Pc { get; set; }

        public int A //Maybe use byte instead of int (you wouldn't need the and operation and could use a short property)
        {
            get { return _a; }
            set { _a = value & 0xFF; }
        }
        public int F
        {
            get { return _f; }
            set { _f = value & 0xFF; }
        }
        public int B
        {
            get { return _b; }
            set { _b = value & 0xFF; }
        }
        public int C
        {
            get { return _c; }
            set { _c = value & 0xFF; }
        }
        public int D
        {
            get { return _d; }
            set { _d = value & 0xFF; }
        }
        public int E
        {
            get { return _e; }
            set { _e = value & 0xFF; }
        }
        public int H
        {
            get { return _h; }
            set { _h = value & 0xFF; }
        }
        public int L
        {
            get { return _l; }
            set { _l = value & 0xFF; }
        }

        // Misc
        public bool Pause { get; set; }

        // CPU Flags
        public bool FlagC { get; set; } // Carry
        public bool FlagHc { get; set; } // HalfCarry
        public bool FlagN { get; set; } // Substract
        public bool FlagZ { get; set; } // Zero
        
        // Interrupt Flags
        public bool Ime { get; set; } // Interrupt Master Enable
        public bool WaitForInterrupt { get; set; } // HALT-Flag
        
        // Memory
        public IMemoryDevice Memory { get; set; }

        // Cycle Handlig
        private bool _actionTaken = false;
        private int[] CycleTableAT = {4, 12, 8, 8, 4, 4, 8, 4, 20, 8, 8, 8, 4, 4, 8, 4,
                                      4, 12, 8, 8, 4, 4, 8, 4, 12, 8, 8, 8, 4, 4, 8, 4,
                                      12, 12, 8, 8, 4, 4, 8, 4, 12, 8, 8, 8, 4, 4, 8, 4,
                                      12, 12, 8, 8, 12, 12, 12, 4, 12, 8, 8, 8, 4, 4, 8, 4,
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                      4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                      4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                      20, 12, 16, 16, 24, 16, 8, 16, 20, 16, 16, 4, 24, 24, 8, 16,
                                      20, 12, 16, 16, 24, 16, 8, 16, 20, 16, 16, 4, 24, 24, 8, 16,
                                      12, 12, 8, 0, 0, 16, 8, 16, 16, 4, 16, 0, 0, 0, 8, 16, 
                                      12, 12, 8, 4, 0, 16, 8, 16, 12, 8, 16, 4, 0, 0, 8, 16};
        private int[] CycleTable = {4, 12, 8, 8, 4, 4, 8, 4, 20, 8, 8, 8, 4, 4, 8, 4,
                                    4, 12, 8, 8, 4, 4, 8, 4, 12, 8, 8, 8, 4, 4, 8, 4,
                                    8, 12, 8, 8, 4, 4, 8, 4, 8, 8, 8, 8, 4, 4, 8, 4,
                                    8, 12, 8, 8, 12, 12, 12, 4, 8, 8, 8, 8, 4, 4, 8, 4,
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                    4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8, 4, 
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                    4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 8, 4,
                                    8, 12, 12, 16, 12, 16, 8, 16, 8, 16, 12, 4, 12, 24, 8, 16,
                                    8, 12, 12, 16, 12, 16, 8, 16, 8, 16, 12, 4, 12, 24, 8, 16,
                                    12, 12, 8, 0, 0, 16, 8, 16, 16, 4, 16, 0, 0, 0, 8, 16, 
                                    12, 12, 8, 4, 0, 16, 8, 16, 12, 8, 16, 4, 0, 0, 8, 16};
        private int[] CycleTableCB = { 8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8, 
                                       8, 8, 8, 8, 8, 8, 16, 8, 8, 8, 8, 8, 8, 8, 16, 8};

        // Other Flags
        private bool Running = true;

        private delegate void Instruction();
        private Instruction[] opcode = new Instruction[256];
        private Instruction[] opcode_cb = new Instruction[256];

        private int cyclPrefixCB;
        private bool flagRegModifiedDirectly;

        public Cpu(IMemoryDevice memory)
        {
            Memory = memory;
            Ime = false;
            #region Opcode table filling
                opcode[0x00] = delegate() { // NOP
                    Pc++;
                    };
                opcode[0x01] = delegate() { // LD BC, d16
                    LD_R16_D16(ref _b, ref _c);
                    };
                opcode[0x02] = delegate() { // LD (BC), A
                    LD_PR16_R8(_b, _c, _a);
                    };
                opcode[0x03] = delegate() { // INC BC
                    INC_R16(ref _b, ref _c);
                    };
                opcode[0x04] = delegate() { // INC B
                    INC_R8(ref _b);
                    };
                opcode[0x05] = delegate() { // DEC B
                    DEC_R8(ref _b);
                    };
                opcode[0x06] = delegate() { // LD B, D8
                    LD_R8_D8(ref _b);
                    };
                opcode[0x07] = delegate() { // RLCA
                    Rlca();
                    };
                opcode[0x08] = delegate() { // LD (A16), SP
                    LD_A16_R16((Sp >> 8) & 0xFF, Sp & 0xFF);
                    };
                opcode[0x09] = delegate() { // ADD HL, BC
                    ADD_R16_R16(ref _h, ref _l, _b, _c);
                    };
                opcode[0x0A] = delegate() { // LD A, (BC)
                    LD_R8_PR16(ref _a, _b, _c);
                    };
                opcode[0x0B] = delegate() { // DEC BC
                    DEC_R16(ref _b, ref _c);
                    };
                opcode[0x0C] = delegate() { // INC C
                    INC_R8(ref _c);
                    };
                opcode[0x0D] = delegate() { // DEC C
                    DEC_R8(ref _c);
                    };
                opcode[0x0E] = delegate() { // LD C, D8
                    LD_R8_D8(ref _c);
                    };
                opcode[0x0F] = delegate() { // RRCA
                    Rrca();
                    };
                opcode[0x10] = delegate() { // STOP 0
                    //WaitForInterrupt = true;
                    Pc++;
                    };
                opcode[0x11] = delegate() { // LD DE, D16
                    LD_R16_D16(ref _d, ref _e);
                    };
                opcode[0x12] = delegate() { // LD (DE), A
                    LD_PR16_R8(_d, _e, _a);
                    };
                opcode[0x13] = delegate() { // INC DE
                    INC_R16(ref _d, ref _e);
                    };
                opcode[0x14] = delegate() { // INC D
                    INC_R8(ref _d);
                    };
                opcode[0x15] = delegate() { // DEC D
                    DEC_R8(ref _d);
                    };
                opcode[0x16] = delegate() { // LD D, D8
                    LD_R8_D8(ref _d);
                    };
                opcode[0x17] = delegate() { // RLA 
                    Rla();
                    };
                opcode[0x18] = delegate() { // JR S8
                    JR_S8();
                    };
                opcode[0x19] = delegate() { // ADD HL, DE
                    ADD_R16_R16(ref _h, ref _l, _d, _e);
                    };
                opcode[0x1A] = delegate() { // LD A, (DE)
                    LD_R8_PR16(ref _a, _d, _e);
                    };
                opcode[0x1B] = delegate() { // DEC DE
                    DEC_R16(ref _d, ref _e);
                    };
                opcode[0x1C] = delegate() { // INC E
                    INC_R8(ref _e);
                    };
                opcode[0x1D] = delegate() { // DEC E
                    DEC_R8(ref _e);
                    };
                opcode[0x1E] = delegate() { // LD E, D8
                    LD_R8_D8(ref _e);
                    };
                opcode[0x1F] = delegate() { // RRA
                    Rra();
                    };
                opcode[0x20] = delegate() { // JR NZ, S8
                    JRNZ_S8();
                    };
                opcode[0x21] = delegate() { // LD HL, D16
                    LD_R16_D16(ref _h, ref _l);
                    };
                opcode[0x22] = delegate() { // LD (HL+), A
                    LDI_PR16_R8(ref _h, ref _l, _a);
                    };
                opcode[0x23] = delegate() { // INC HL
                    INC_R16(ref _h, ref _l);
                    };
                opcode[0x24] = delegate() { // INC H
                    INC_R8(ref _h);
                    };
                opcode[0x25] = delegate() { // DEC H
                    DEC_R8(ref _h);
                    };
                opcode[0x26] = delegate() { // LD H, D8
                    LD_R8_D8(ref _h);
                    };
                opcode[0x27] = delegate() { // DAA
                    Daa();
                    };
                opcode[0x28] = delegate() { // JR Z, S8
                    JRZ_S8();
                    };
                opcode[0x29] = delegate() { // ADD HL, HL
                    ADD_R16_R16(ref _h, ref _l, _h, _l);
                    };
                opcode[0x2A] = delegate() { // LD A, (HL+)
                    LDI_R8_PR16(ref _a, ref _h, ref _l);
                    };
                opcode[0x2B] = delegate() { // DEC HL
                    DEC_R16(ref _h, ref _l);
                    };
                opcode[0x2C] = delegate() { // INC L
                    INC_R8(ref _l);
                    };
                opcode[0x2D] = delegate() { // DEC L
                    DEC_R8(ref _l);
                    };
                opcode[0x2E] = delegate() { // LD L, D8
                    LD_R8_D8(ref _l);
                    };
                opcode[0x2F] = delegate() { // CPL
                    Cpl();
                    };
                opcode[0x30] = delegate() { // JR NC, S8
                    JRNC_S8();
                    };
                opcode[0x31] = delegate() { // LD SP, D16
                    LD_SP_D16();
                    };
                opcode[0x32] = delegate() { // LD (HL-), A
                    LDD_PR16_R8(ref _h, ref _l, _a);
                    };
                opcode[0x33] = delegate() { // INC SP
                    INC_SP();
                    };
                opcode[0x34] = delegate() { // INC (HL)
                    INC_PR16(_h, _l);
                    };
                opcode[0x35] = delegate() { // DEC (HL)
                    DEC_PR16(_h, _l);
                    };
                opcode[0x36] = delegate() { // LD (HL), D8
                    LD_PR16_D8(_h, _l);
                    };
                opcode[0x37] = delegate() { // SCF
                    Scf();
                    };
                opcode[0x38] = delegate() { // JR C, S8
                    JRC_S8();
                    };
                opcode[0x39] = delegate() { // ADD HL, SP
                    ADD_R16_R16(ref _h, ref _l, Sp >> 8, Sp & 0xFF);
                    };
                opcode[0x3A] = delegate() { // LD A, (HL-)
                    LDD_R8_PR16(ref _a, ref _h, ref _l);
                    };
                opcode[0x3B] = delegate() { // DEC SP
                    Sp--;
                    Sp &= 0xFFFF;
                    Pc++;
                    };
                opcode[0x3C] = delegate() { // INC A
                    INC_R8(ref _a);
                    };
                opcode[0x3D] = delegate() { // DEC A
                    DEC_R8(ref _a);
                    };
                opcode[0x3E] = delegate() { // LD A, D8
                    LD_R8_D8(ref _a);
                    };
                opcode[0x3F] = delegate() { // CCF
                    Ccf();
                    };
                opcode[0x40] = delegate() { // LD B, B
                    LD_R8_R8(ref _b, _b);
                    };
                opcode[0x41] = delegate() { // LD B, C
                    LD_R8_R8(ref _b, _c);
                    };
                opcode[0x42] = delegate() { // LD B, D
                    LD_R8_R8(ref _b, _d);
                    };
                opcode[0x43] = delegate() { // LD B, E
                    LD_R8_R8(ref _b, _e);
                    };
                opcode[0x44] = delegate() { // LD B, H
                    LD_R8_R8(ref _b, _h);
                    };
                opcode[0x45] = delegate() { // LD B, L
                    LD_R8_R8(ref _b, _l);
                    };
                opcode[0x46] = delegate() { // LD B, (HL)
                    LD_R8_PR16(ref _b, _h, _l);
                    };
                opcode[0x47] = delegate() { // LD B, A
                    LD_R8_R8(ref _b, _a);
                    };
                opcode[0x48] = delegate() { // LD C, B
                    LD_R8_R8(ref _c, _b);
                    };
                opcode[0x49] = delegate() { // LD C, C
                    LD_R8_R8(ref _c, _c);
                    };
                opcode[0x4A] = delegate() { // LD C, D
                    LD_R8_R8(ref _c, _d);
                    };
                opcode[0x4B] = delegate() { // LD C, E
                    LD_R8_R8(ref _c, _e);
                    };
                opcode[0x4C] = delegate() { // LD C, H
                    LD_R8_R8(ref _c, _h);
                    };
                opcode[0x4D] = delegate() { // LD C, L
                    LD_R8_R8(ref _c, _l);
                    };
                opcode[0x4E] = delegate() { // LD C, (HL)
                    LD_R8_PR16(ref _c, _h, _l);
                    };
                opcode[0x4F] = delegate() { // LD C, A
                    LD_R8_R8(ref _c, _a);
                    };
                opcode[0x50] = delegate() { // LD D, B
                    LD_R8_R8(ref _d, _b);
                    };
                opcode[0x51] = delegate() { // LD D, C
                    LD_R8_R8(ref _d, _c);
                    };
                opcode[0x52] = delegate() { // LD D, D
                    LD_R8_R8(ref _d, _d);
                    };
                opcode[0x53] = delegate() { // LD D, E
                    LD_R8_R8(ref _d, _e);
                    };
                opcode[0x54] = delegate() { // LD D, H
                    LD_R8_R8(ref _d, _h);
                    };
                opcode[0x55] = delegate() { // LD D, L
                    LD_R8_R8(ref _d, _l);
                    };
                opcode[0x56] = delegate() { // LD D, (HL)
                    LD_R8_PR16(ref _d, _h, _l);
                    };
                opcode[0x57] = delegate() { // LD D, A
                    LD_R8_R8(ref _d, _a);
                    };
                opcode[0x58] = delegate() { // LD E, B
                    LD_R8_R8(ref _e, _b);
                    };
                opcode[0x59] = delegate() { // LD E, C
                    LD_R8_R8(ref _e, _c);
                    };
                opcode[0x5A] = delegate() { // LD E, D
                    LD_R8_R8(ref _e, _d);
                    };
                opcode[0x5B] = delegate() { // LD E, E
                    LD_R8_R8(ref _e, _e);
                    };
                opcode[0x5C] = delegate() { // LD E, H
                    LD_R8_R8(ref _e, _h);
                    };
                opcode[0x5D] = delegate() { // LD E, L
                    LD_R8_R8(ref _e, _l);
                    };
                opcode[0x5E] = delegate() { // LD E, (HL)
                    LD_R8_PR16(ref _e, _h, _l);
                    };
                opcode[0x5F] = delegate() { // LD E, A
                    LD_R8_R8(ref _e, _a);
                    };
                opcode[0x60] = delegate() { // LD H, B
                    LD_R8_R8(ref _h, _b);
                    };
                opcode[0x61] = delegate() { // LD H, C
                    LD_R8_R8(ref _h, _c);
                    };
                opcode[0x62] = delegate() { // LD H, D
                    LD_R8_R8(ref _h, _d);
                    };
                opcode[0x63] = delegate() { // LD H, E
                    LD_R8_R8(ref _h, _e);
                    };
                opcode[0x64] = delegate() { // LD H, H
                    LD_R8_R8(ref _h, _h);
                    };
                opcode[0x65] = delegate() { // LD H, L
                    LD_R8_R8(ref _h, _l);
                    };
                opcode[0x66] = delegate() { // LD H, (HL)
                    LD_R8_PR16(ref _h, _h, _l);
                    };
                opcode[0x67] = delegate() { // LD H, A
                    LD_R8_R8(ref _h, _a);
                    };
                opcode[0x68] = delegate() { // LD L, B
                    LD_R8_R8(ref _l, _b);
                    };
                opcode[0x69] = delegate() { // LD L, C
                    LD_R8_R8(ref _l, _c);
                    };
                opcode[0x6A] = delegate() { // LD L, D
                    LD_R8_R8(ref _l, _d);
                    };
                opcode[0x6B] = delegate() { // LD L, E
                    LD_R8_R8(ref _l, _e);
                    };
                opcode[0x6C] = delegate() { // LD L, H
                    LD_R8_R8(ref _l, _h);
                    };
                opcode[0x6D] = delegate() { // LD L, L
                    LD_R8_R8(ref _l, _l);
                    };
                opcode[0x6E] = delegate() { // LD L, (HL)
                    LD_R8_PR16(ref _l, _h, _l);
                    };
                opcode[0x6F] = delegate() { // LD L, A
                    LD_R8_R8(ref _l, _a);
                    };
                opcode[0x70] = delegate() { // LD (HL), B
                    LD_PR16_R8(_h, _l, _b);
                    };
                opcode[0x71] = delegate() { // LD (HL), C
                    LD_PR16_R8(_h, _l, _c);
                    };
                opcode[0x72] = delegate() { // LD (HL), D
                    LD_PR16_R8(_h, _l, _d);
                    };
                opcode[0x73] = delegate() { // LD (HL), E
                    LD_PR16_R8(_h, _l, _e);
                    };
                opcode[0x74] = delegate() { // LD (HL), H
                    LD_PR16_R8(_h, _l, _h);
                    };
                opcode[0x75] = delegate() { // LD (HL), L
                    LD_PR16_R8(_h, _l, _l);
                    };
                opcode[0x76] = delegate() { // HALT
                    Halt();
                    };
                opcode[0x77] = delegate() { // LD (HL), A
                    LD_PR16_R8(_h, _l, _a);
                    };
                opcode[0x78] = delegate() { // LD A, B
                    LD_R8_R8(ref _a, _b);
                    };
                opcode[0x79] = delegate() { // LD A, C
                    LD_R8_R8(ref _a, _c);
                    };
                opcode[0x7A] = delegate() { // LD A, D
                    LD_R8_R8(ref _a, _d);
                    };
                opcode[0x7B] = delegate() { // LD A, E
                    LD_R8_R8(ref _a, _e);
                    };
                opcode[0x7C] = delegate() { // LD A, H
                    LD_R8_R8(ref _a, _h);
                    };
                opcode[0x7D] = delegate() { // LD A, L
                    LD_R8_R8(ref _a, _l);
                    };
                opcode[0x7E] = delegate() { // LD A, (HL)
                    LD_R8_PR16(ref _a, _h, _l);
                    };
                opcode[0x7F] = delegate() { // LD A, A
                    LD_R8_R8(ref _a, _a);
                    };
                opcode[0x80] = delegate() { // ADD A, B
                    ADD_R8_R8(ref _a, _b);
                    };
                opcode[0x81] = delegate() { // ADD A, C
                    ADD_R8_R8(ref _a, _c);
                    };
                opcode[0x82] = delegate() { // ADD A, D
                    ADD_R8_R8(ref _a, _d);
                    };
                opcode[0x83] = delegate() { // ADD A, E
                    ADD_R8_R8(ref _a, _e);
                    };
                opcode[0x84] = delegate() { // ADD A, H
                    ADD_R8_R8(ref _a, _h);
                    };
                opcode[0x85] = delegate() { // ADD A, L
                    ADD_R8_R8(ref _a, _l);
                    };
                opcode[0x86] = delegate() { // ADD A, (HL)
                    ADD_R8_PR16(ref _a, _h, _l);
                    };
                opcode[0x87] = delegate() { // ADD A, A
                    ADD_R8_R8(ref _a, _a);
                    };
                opcode[0x88] = delegate() { // ADC A, B
                    ADC_R8_R8(ref _a, _b);
                    };
                opcode[0x89] = delegate() { // ADC A, C
                    ADC_R8_R8(ref _a, _c);
                    };
                opcode[0x8A] = delegate() { // ADC A, D
                    ADC_R8_R8(ref _a, _d);
                    };
                opcode[0x8B] = delegate() { // ADC A, E
                    ADC_R8_R8(ref _a, _e);
                    };
                opcode[0x8C] = delegate() { // ADC A, H
                    ADC_R8_R8(ref _a, _h);
                    };
                opcode[0x8D] = delegate() { // ADC A, L
                    ADC_R8_R8(ref _a, _l);
                    };
                opcode[0x8E] = delegate() { // ADC A, (HL)
                    ADC_R8_PR16(ref _a, _h, _l);
                    };
                opcode[0x8F] = delegate() { // ADC A, A
                    ADC_R8_R8(ref _a, _a);
                    };
                opcode[0x90] = delegate() { // SUB B
                    SUB_R8(_b);
                    };
                opcode[0x91] = delegate() { // SUB C
                    SUB_R8(_c);
                    };
                opcode[0x92] = delegate() { // SUB D
                    SUB_R8(_d);
                    };
                opcode[0x93] = delegate() { // SUB E
                    SUB_R8(_e);
                    };
                opcode[0x94] = delegate() { // SUB H
                    SUB_R8(_h);
                    };
                opcode[0x95] = delegate() { // SUB L
                    SUB_R8(_l);
                    };
                opcode[0x96] = delegate() { // SUB (HL)
                    SUB_PR16(_h, _l);
                    };
                opcode[0x97] = delegate() { // SUB A
                    SUB_R8(_a);
                    };
                opcode[0x98] = delegate() { // SBC A, B
                    SBC_R8_R8(ref _a, _b);
                    };
                opcode[0x99] = delegate() { // SBC A, C
                    SBC_R8_R8(ref _a, _c);
                    };
                opcode[0x9A] = delegate() { // SBC A, D
                    SBC_R8_R8(ref _a, _d);
                    };
                opcode[0x9B] = delegate() { // SBC A, E
                    SBC_R8_R8(ref _a, _e);
                    };
                opcode[0x9C] = delegate() { // SBC A, H
                    SBC_R8_R8(ref _a, _h);
                    };
                opcode[0x9D] = delegate() { // SBC A, L
                    SBC_R8_R8(ref _a, _l);
                    };
                opcode[0x9E] = delegate() { // SBC A, (HL)
                    SBC_R8_PR16(ref _a, _h, _l);
                    };
                opcode[0x9F] = delegate() { // SBC A, A
                    SBC_R8_R8(ref _a, _a);
                    };
                opcode[0xA0] = delegate() { // AND B
                    AND_R8(_b);
                    };
                opcode[0xA1] = delegate() { // AND C
                    AND_R8(_c);
                    };
                opcode[0xA2] = delegate() { // AND D
                    AND_R8(_d);
                    };
                opcode[0xA3] = delegate() { // AND E
                    AND_R8(_e);
                    };
                opcode[0xA4] = delegate() { // AND H
                    AND_R8(_h);
                    };
                opcode[0xA5] = delegate() { // AND L
                    AND_R8(_l);
                    };
                opcode[0xA6] = delegate() { // AND B
                    AND_PR16(_h, _l);
                    };
                opcode[0xA7] = delegate() { // AND A
                    AND_R8(_a);
                    };
                opcode[0xA8] = delegate() { // XOR B
                    XOR_R8(_b);
                    };
                opcode[0xA9] = delegate() { // XOR C
                    XOR_R8(_c);
                    };
                opcode[0xAA] = delegate() { // XOR D
                    XOR_R8(_d);
                    };
                opcode[0xAB] = delegate() { // XOR E
                    XOR_R8(_e);
                    };
                opcode[0xAC] = delegate() { // XOR H
                    XOR_R8(_h);
                    };
                opcode[0xAD] = delegate() { // XOR L
                    XOR_R8(_l);
                    };
                opcode[0xAE] = delegate() { // XOR (HL)
                    XOR_PR16(_h, _l);
                    };
                opcode[0xAF] = delegate() { // XOR A
                    XOR_R8(_a);
                    };
                opcode[0xB0] = delegate() { // OR B
                    OR_R8(_b);
                    };
                opcode[0xB1] = delegate() { // OR C
                    OR_R8(_c);
                    };
                opcode[0xB2] = delegate() { // OR D
                    OR_R8(_d);
                    };
                opcode[0xB3] = delegate() { // OR E
                    OR_R8(_e);
                    };
                opcode[0xB4] = delegate() { // OR H
                    OR_R8(_h);
                    };
                opcode[0xB5] = delegate() { // OR L
                    OR_R8(_l);
                    };
                opcode[0xB6] = delegate() { // OR (HL)
                    OR_PR16(_h, _l);
                    };
                opcode[0xB7] = delegate() { // OR A
                    OR_R8(_a);
                    };
                opcode[0xB8] = delegate() { // CP B
                    CP_R8(_b);
                    };
                opcode[0xB9] = delegate() { // CP C
                    CP_R8(_c);
                    };
                opcode[0xBA] = delegate() { // CP D
                    CP_R8(_d);
                    };
                opcode[0xBB] = delegate() { // CP E
                    CP_R8(_e);
                    };
                opcode[0xBC] = delegate() { // CP H
                    CP_R8(_h);
                    };
                opcode[0xBD] = delegate() { // CP L
                    CP_R8(_l);
                    };
                opcode[0xBE] = delegate() { // CP (HL)
                    CP_PR16(_h, _l);
                    };
                opcode[0xBF] = delegate() { // CP A
                    CP_R8(_a);
                    };
                opcode[0xC0] = delegate() { // RET NZ
                    Retnz();
                    };
                opcode[0xC1] = delegate() { // POP BC
                    Pop(ref _b, ref _c);
                    };
                opcode[0xC2] = delegate() { // JP NZ, A16
                    JPNZ_A16();
                    };
                opcode[0xC3] = delegate() { // JP A16
                    JP_A16();
                    };
                opcode[0xC4] = delegate() { // CALL NZ, A16
                    CALLNZ_A16();
                    };
                opcode[0xC5] = delegate() { // PUSH BC
                    Push(_b, _c);
                    };
                opcode[0xC6] = delegate() { // ADD A, D8
                    ADD_R8_D8(ref _a);
                    };
                opcode[0xC7] = delegate() { // RST 0x00
                    Rst(0x00);
                    };
                opcode[0xC8] = delegate() { // RET Z
                    Retz();
                    };
                opcode[0xC9] = delegate() { // RET
                    Ret();
                    };
                opcode[0xCA] = delegate() { // JP Z, A16
                    JPZ_A16();
                    };
                opcode[0xCB] = delegate() { // PREFIX CB
                    Pc++;
                    cyclPrefixCB = ExecutePrefixCb();
                    };
                opcode[0xCC] = delegate() { // CALL Z, A16
                    CALLZ_A16();
                    };
                opcode[0xCD] = delegate() { // CALL A16
                    CALL_A16();
                    };
                opcode[0xCE] = delegate() { // ADC A, D8
                    ADC_R8_D8(ref _a);
                    };
                opcode[0xCF] = delegate() { // RST 0x08
                    Rst(0x08);
                    };
                opcode[0xD0] = delegate() { // RET NC
                    Retnc();
                    };
                opcode[0xD1] = delegate() { // POP DE
                    Pop(ref _d, ref _e);
                    };
                opcode[0xD2] = delegate() { // JP NC, A16
                    JPNC_A16();
                    };
                opcode[0xD3] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xD4] = delegate() { // CALL NC, A16
                    CALLNC_A16();
                    };
                opcode[0xD5] = delegate() { // PUSH DE
                    Push(_d, _e);
                    };
                opcode[0xD6] = delegate() { // SUB D8
                    SUB_D8();
                    };
                opcode[0xD7] = delegate() { // RST 0x10
                    Rst(0x10);
                    };
                opcode[0xD8] = delegate() { // RET C
                    Retc();
                    };
                opcode[0xD9] = delegate() { // RETI
                    Reti();
                    };
                opcode[0xDA] = delegate() { // JP C, A16
                    JPC_A16();
                    };
                opcode[0xDB] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xDC] = delegate() { // CALL C, A16
                    CALLC_A16();
                    };
                opcode[0xDD] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xDE] = delegate() { // SBC A, D8
                    SBC_R8_D8(ref _a);
                    };
                opcode[0xDF] = delegate() { // RST 0x18
                    Rst(0x18);
                    };
                opcode[0xE0] = delegate() { // LD (A8), A
                    LD_A8_R8(_a);
                    };
                opcode[0xE1] = delegate() { // POP HL
                    Pop(ref _h, ref _l);
                    };
                opcode[0xE2] = delegate() { // LD (C), A
                    LD_PR8_R8(_c, _a);
                    };
                opcode[0xE3] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xE4] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xE5] = delegate() { // PUSH HL
                    Push(_h, _l);
                    };
                opcode[0xE6] = delegate() { // AND D8
                    AND_D8();
                    };
                opcode[0xE7] = delegate() { // RST 0x20
                    Rst(0x20);
                    };
                opcode[0xE8] = delegate() { // ADD SP, S8
                    ADD_SP_S8();
                    };
                opcode[0xE9] = delegate() { // JP HL
                    JP_R16(_h, _l);
                    };
                opcode[0xEA] = delegate() { // LD (A16), A
                    LD_A16_R8(_a);
                    };
                opcode[0xEB] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xEC] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xED] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xEE] = delegate() { // XOR D8
                    XOR_D8();
                    };
                opcode[0xEF] = delegate() { // RST 0x28
                    Rst(0x28);
                    };
                opcode[0xF0] = delegate() { // LD A, (A8)
                    LD_R8_A8(ref _a);
                    };
                opcode[0xF1] = delegate() { // POP AF
                    Pop(ref _a, ref _f);
                    flagRegModifiedDirectly = true;
                    };
                opcode[0xF2] = delegate() { // LD A, (C)
                    LD_R8_PR8(ref _a, _c);
                    };
                opcode[0xF3] = delegate() { // DI
                    Di();
                    };
                opcode[0xF4] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xF5] = delegate() { // PUSH AF
                    Push(_a, _f);
                    };
                opcode[0xF6] = delegate() { // OR D8
                    OR_D8();
                    };
                opcode[0xF7] = delegate() { // RST 0x30
                    Rst(0x30);
                    };
                opcode[0xF8] = delegate() { // LD HL, SP+S8
                    LD_R16_SP_S8(ref _h, ref _l);
                    };
                opcode[0xF9] = delegate() { // LD SP, HL
                    LD_SP_R16(_h, _l);
                    };
                opcode[0xFA] = delegate() { // LD A, (A16)
                    LD_R8_A16(ref _a);
                    };
                opcode[0xFB] = delegate() { // EI
                    Ei();
                    };
                opcode[0xFC] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xFD] = delegate() { // UNKNOWN OPCODE
                    Pc++;
                    };
                opcode[0xFE] = delegate() { // CP D8
                    CP_D8();
                    };
                opcode[0xFF] = delegate() { // RST 0x38
                    Rst(0x38);
                    };
            #endregion
            #region Opcode CB table filling
                opcode_cb[0x00] = delegate() { // RLC B
                    RLC_R8(ref _b);
                    };
                opcode_cb[0x01] = delegate() { // RLC C
                    RLC_R8(ref _c);
                    };
                opcode_cb[0x02] = delegate() { // RLC D
                    RLC_R8(ref _d);
                    };
                opcode_cb[0x03] = delegate() { // RLC E
                    RLC_R8(ref _e);
                    };
                opcode_cb[0x04] = delegate() { // RLC H
                    RLC_R8(ref _h);
                    };
                opcode_cb[0x05] = delegate() { // RLC L
                    RLC_R8(ref _l);
                    };
                opcode_cb[0x06] = delegate() { // RLC (HL)
                    RLC_PR16(_h, _l);
                    };
                opcode_cb[0x07] = delegate() { // RLC A
                    RLC_R8(ref _a);
                    };
                opcode_cb[0x08] = delegate() { // RRC B
                    RRC_R8(ref _b);
                    };
                opcode_cb[0x09] = delegate() { // RRC C
                    RRC_R8(ref _c);
                    };
                opcode_cb[0x0A] = delegate() { // RRC D
                    RRC_R8(ref _d);
                    };
                opcode_cb[0x0B] = delegate() { // RRC E
                    RRC_R8(ref _e);
                    };
                opcode_cb[0x0C] = delegate() { // RRC H
                    RRC_R8(ref _h);
                    };
                opcode_cb[0x0D] = delegate() { // RRC L
                    RRC_R8(ref _l);
                    };
                opcode_cb[0x0E] = delegate() { // RRC (HL)
                    RRC_PR16(_h, _l);
                    };
                opcode_cb[0x0F] = delegate() { // RRC A
                    RRC_R8(ref _a);
                    };
                opcode_cb[0x10] = delegate() { // RL B
                    RL_R8(ref _b);
                    };
                opcode_cb[0x11] = delegate() { // RL C
                    RL_R8(ref _c);
                    };
                opcode_cb[0x12] = delegate() { // RL D
                    RL_R8(ref _d);
                    };
                opcode_cb[0x13] = delegate() { // RL E
                    RL_R8(ref _e);
                    };
                opcode_cb[0x14] = delegate() { // RL H
                    RL_R8(ref _h);
                    };
                opcode_cb[0x15] = delegate() { // RL L
                    RL_R8(ref _l);
                    };
                opcode_cb[0x16] = delegate() { // RL (HL)
                    RL_PR16(_h, _l);
                    };
                opcode_cb[0x17] = delegate() { // RL A
                    RL_R8(ref _a);
                    };
                opcode_cb[0x18] = delegate() { // RR B
                    RR_R8(ref _b);
                    };
                opcode_cb[0x19] = delegate() { // RR C
                    RR_R8(ref _c);
                    };
                opcode_cb[0x1A] = delegate() { // RR D
                    RR_R8(ref _d);
                    };
                opcode_cb[0x1B] = delegate() { // RR E
                    RR_R8(ref _e);
                    };
                opcode_cb[0x1C] = delegate() { // RR H
                    RR_R8(ref _h);
                    };
                opcode_cb[0x1D] = delegate() { // RR L
                    RR_R8(ref _l);
                    };
                opcode_cb[0x1E] = delegate() { // RR (HL)
                    RR_PR16(_h, _l);
                    };
                opcode_cb[0x1F] = delegate() { // RR A
                    RR_R8(ref _a);
                    };
                opcode_cb[0x20] = delegate() { // SLA B
                    SLA_R8(ref _b);
                    };
                opcode_cb[0x21] = delegate() { // SLA C
                    SLA_R8(ref _c);
                    };
                opcode_cb[0x22] = delegate() { // SLA D
                    SLA_R8(ref _d);
                    };
                opcode_cb[0x23] = delegate() { // SLA E
                    SLA_R8(ref _e);
                    };
                opcode_cb[0x24] = delegate() { // SLA H
                    SLA_R8(ref _h);
                    };
                opcode_cb[0x25] = delegate() { // SLA L
                    SLA_R8(ref _l);
                    };
                opcode_cb[0x26] = delegate() { // SLA (HL)
                    SLA_PR16(_h, _l);
                    };
                opcode_cb[0x27] = delegate() { // SLA A
                    SLA_R8(ref _a);
                    };
                opcode_cb[0x28] = delegate() { // SRA B
                    SRA_R8(ref _b);
                    };
                opcode_cb[0x29] = delegate() { // SRA C
                    SRA_R8(ref _c);
                    };
                opcode_cb[0x2A] = delegate() { // SRA D
                    SRA_R8(ref _d);
                    };
                opcode_cb[0x2B] = delegate() { // SRA E
                    SRA_R8(ref _e);
                    };
                opcode_cb[0x2C] = delegate() { // SRA H
                    SRA_R8(ref _h);
                    };
                opcode_cb[0x2D] = delegate() { // SRA L
                    SRA_R8(ref _l);
                    };
                opcode_cb[0x2E] = delegate() { // SRA (HL)
                    SRA_PR16(_h, _l);
                    };
                opcode_cb[0x2F] = delegate() { // SRA A
                    SRA_R8(ref _a);
                    };
                opcode_cb[0x30] = delegate() { // SWAP B
                    SWAP_R8(ref _b);
                    };
                opcode_cb[0x31] = delegate() { // SWAP C
                    SWAP_R8(ref _c);
                    };
                opcode_cb[0x32] = delegate() { // SWAP D
                    SWAP_R8(ref _d);
                    };
                opcode_cb[0x33] = delegate() { // SWAP E
                    SWAP_R8(ref _e);
                    };
                opcode_cb[0x34] = delegate() { // SWAP H
                    SWAP_R8(ref _h);
                    };
                opcode_cb[0x35] = delegate() { // SWAP L
                    SWAP_R8(ref _l);
                    };
                opcode_cb[0x36] = delegate() { // SWAP (HL)
                    SWAP_PR16(_h, _l);
                    };
                opcode_cb[0x37] = delegate() { // SWAP A
                    SWAP_R8(ref _a);
                    };
                opcode_cb[0x38] = delegate() { // SRL B
                    SRL_R8(ref _b);
                    };
                opcode_cb[0x39] = delegate() { // SRL C
                    SRL_R8(ref _c);
                    };
                opcode_cb[0x3A] = delegate() { // SRL D
                    SRL_R8(ref _d);
                    };
                opcode_cb[0x3B] = delegate() { // SRL E
                    SRL_R8(ref _e);
                    };
                opcode_cb[0x3C] = delegate() { // SRL H
                    SRL_R8(ref _h);
                    };
                opcode_cb[0x3D] = delegate() { // SRL L
                    SRL_R8(ref _l);
                    };
                opcode_cb[0x3E] = delegate() { // SRL (HL)
                    SRL_PR16(_h, _l);
                    };
                opcode_cb[0x3F] = delegate() { // SRL A
                    SRL_R8(ref _a);
                    };
                opcode_cb[0x40] = delegate() { // BIT 0, B
                    BIT_R8(0, _b);
                    };
                opcode_cb[0x41] = delegate() { // BIT 0, C
                    BIT_R8(0, _c);
                    };
                opcode_cb[0x42] = delegate() { // BIT 0, D
                    BIT_R8(0, _d);
                    };
                opcode_cb[0x43] = delegate() { // BIT 0, E
                    BIT_R8(0, _e);
                    };
                opcode_cb[0x44] = delegate() { // BIT 0, H
                    BIT_R8(0, _h);
                    };
                opcode_cb[0x45] = delegate() { // BIT 0, L
                    BIT_R8(0, _l);
                    };
                opcode_cb[0x46] = delegate() { // BIT 0, (HL)
                    BIT_PR16(0, _h, _l);
                    };
                opcode_cb[0x47] = delegate() { // BIT 0, A
                    BIT_R8(0, _a);
                    };
                opcode_cb[0x48] = delegate() { // BIT 1, B
                    BIT_R8(1, _b);
                    };
                opcode_cb[0x49] = delegate() { // BIT 1, C
                    BIT_R8(1, _c);
                    };
                opcode_cb[0x4A] = delegate() { // BIT 1, D
                    BIT_R8(1, _d);
                    };
                opcode_cb[0x4B] = delegate() { // BIT 1, E
                    BIT_R8(1, _e);
                    };
                opcode_cb[0x4C] = delegate() { // BIT 1, H
                    BIT_R8(1, _h);
                    };
                opcode_cb[0x4D] = delegate() { // BIT 1, L
                    BIT_R8(1, _l);
                    };
                opcode_cb[0x4E] = delegate() { // BIT 1, (HL)
                    BIT_PR16(1, _h, _l);
                    };
                opcode_cb[0x4F] = delegate() { // BIT 1, A
                    BIT_R8(1, _a);
                    };
                opcode_cb[0x50] = delegate() { // BIT 2, B
                    BIT_R8(2, _b);
                    };
                opcode_cb[0x51] = delegate() { // BIT 2, C
                    BIT_R8(2, _c);
                    };
                opcode_cb[0x52] = delegate() { // BIT 2, D
                    BIT_R8(2, _d);
                    };
                opcode_cb[0x53] = delegate() { // BIT 2, E
                    BIT_R8(2, _e);
                    };
                opcode_cb[0x54] = delegate() { // BIT 2, H
                    BIT_R8(2, _h);
                    };
                opcode_cb[0x55] = delegate() { // BIT 2, L
                    BIT_R8(2, _l);
                    };
                opcode_cb[0x56] = delegate() { // BIT 2, (HL)
                    BIT_PR16(2, _h, _l);
                    };
                opcode_cb[0x57] = delegate() { // BIT 2, A
                    BIT_R8(2, _a);
                    };
                opcode_cb[0x58] = delegate() { // BIT 3, B
                    BIT_R8(3, _b);
                    };
                opcode_cb[0x59] = delegate() { // BIT 3, C
                    BIT_R8(3, _c);
                    };
                opcode_cb[0x5A] = delegate() { // BIT 3, D
                    BIT_R8(3, _d);
                    };
                opcode_cb[0x5B] = delegate() { // BIT 3, E
                    BIT_R8(3, _e);
                    };
                opcode_cb[0x5C] = delegate() { // BIT 3, H
                    BIT_R8(3, _h);
                    };
                opcode_cb[0x5D] = delegate() { // BIT 3, L
                    BIT_R8(3, _l);
                    };
                opcode_cb[0x5E] = delegate() { // BIT 3, (HL)
                    BIT_PR16(3, _h, _l);
                    };
                opcode_cb[0x5F] = delegate() { // BIT 3, A
                    BIT_R8(3, _a);
                    };
                opcode_cb[0x60] = delegate() { // BIT 4, B
                    BIT_R8(4, _b);
                    };
                opcode_cb[0x61] = delegate() { // BIT 4, C
                    BIT_R8(4, _c);
                    };
                opcode_cb[0x62] = delegate() { // BIT 4, D
                    BIT_R8(4, _d);
                    };
                opcode_cb[0x63] = delegate() { // BIT 4, E
                    BIT_R8(4, _e);
                    };
                opcode_cb[0x64] = delegate() { // BIT 4, H
                    BIT_R8(4, _h);
                    };
                opcode_cb[0x65] = delegate() { // BIT 4, L
                    BIT_R8(4, _l);
                    };
                opcode_cb[0x66] = delegate() { // BIT 4, (HL)
                    BIT_PR16(4, _h, _l);
                    };
                opcode_cb[0x67] = delegate() { // BIT 4, A
                    BIT_R8(4, _a);
                    };
                opcode_cb[0x68] = delegate() { // BIT 5, B
                    BIT_R8(5, _b);
                    };
                opcode_cb[0x69] = delegate() { // BIT 5, C
                    BIT_R8(5, _c);
                    };
                opcode_cb[0x6A] = delegate() { // BIT 5, D
                    BIT_R8(5, _d);
                    };
                opcode_cb[0x6B] = delegate() { // BIT 5, E
                    BIT_R8(5, _e);
                    };
                opcode_cb[0x6C] = delegate() { // BIT 5, H
                    BIT_R8(5, _h);
                    };
                opcode_cb[0x6D] = delegate() { // BIT 5, L
                    BIT_R8(5, _l);
                    };
                opcode_cb[0x6E] = delegate() { // BIT 5, (HL)
                    BIT_PR16(5, _h, _l);
                    };
                opcode_cb[0x6F] = delegate() { // BIT 5, A
                    BIT_R8(5, _a);
                    };
                opcode_cb[0x70] = delegate() { // BIT 6, B
                    BIT_R8(6, _b);
                    };
                opcode_cb[0x71] = delegate() { // BIT 6, C
                    BIT_R8(6, _c);
                    };
                opcode_cb[0x72] = delegate() { // BIT 6, D
                    BIT_R8(6, _d);
                    };
                opcode_cb[0x73] = delegate() { // BIT 6, E
                    BIT_R8(6, _e);
                    };
                opcode_cb[0x74] = delegate() { // BIT 6, H
                    BIT_R8(6, _h);
                    };
                opcode_cb[0x75] = delegate() { // BIT 6, L
                    BIT_R8(6, _l);
                    };
                opcode_cb[0x76] = delegate() { // BIT 6, (HL)
                    BIT_PR16(6, _h, _l);
                    };
                opcode_cb[0x77] = delegate() { // BIT 6, A
                    BIT_R8(6, _a);
                    };
                opcode_cb[0x78] = delegate() { // BIT 7, B
                    BIT_R8(7, _b);
                    };
                opcode_cb[0x79] = delegate() { // BIT 7, C
                    BIT_R8(7, _c);
                    };
                opcode_cb[0x7A] = delegate() { // BIT 7, D
                    BIT_R8(7, _d);
                    };
                opcode_cb[0x7B] = delegate() { // BIT 7, E
                    BIT_R8(7, _e);
                    };
                opcode_cb[0x7C] = delegate() { // BIT 7, H
                    BIT_R8(7, _h);
                    };
                opcode_cb[0x7D] = delegate() { // BIT 7, L
                    BIT_R8(7, _l);
                    };
                opcode_cb[0x7E] = delegate() { // BIT 7, (HL)
                    BIT_PR16(7, _h, _l);
                    };
                opcode_cb[0x7F] = delegate() { // BIT 7, A
                    BIT_R8(7, _a);
                    };
                opcode_cb[0x80] = delegate() { // RES 0, B
                    RES_R8(0, ref _b);
                    };
                opcode_cb[0x81] = delegate() { // RES 0, C
                    RES_R8(0, ref _c);
                    };
                opcode_cb[0x82] = delegate() { // RES 0, D
                    RES_R8(0, ref _d);
                    };
                opcode_cb[0x83] = delegate() { // RES 0, E
                    RES_R8(0, ref _e);
                    };
                opcode_cb[0x84] = delegate() { // RES 0, H
                    RES_R8(0, ref _h);
                    };
                opcode_cb[0x85] = delegate() { // RES 0, L
                    RES_R8(0, ref _l);
                    };
                opcode_cb[0x86] = delegate() { // RES 0, (HL)
                    RES_PR16(0, _h, _l);
                    };
                opcode_cb[0x87] = delegate() { // RES 0, A
                    RES_R8(0, ref _a);
                    };
                opcode_cb[0x88] = delegate() { // RES 1, B
                    RES_R8(1, ref _b);
                    };
                opcode_cb[0x89] = delegate() { // RES 1, C
                    RES_R8(1, ref _c);
                    };
                opcode_cb[0x8A] = delegate() { // RES 1, D
                    RES_R8(1, ref _d);
                    };
                opcode_cb[0x8B] = delegate() { // RES 1, E
                    RES_R8(1, ref _e);
                    };
                opcode_cb[0x8C] = delegate() { // RES 1, H
                    RES_R8(1, ref _h);
                    };
                opcode_cb[0x8D] = delegate() { // RES 1, L
                    RES_R8(1, ref _l);
                    };
                opcode_cb[0x8E] = delegate() { // RES 1, (HL)
                    RES_PR16(1, _h, _l);
                    };
                opcode_cb[0x8F] = delegate() { // RES 1, A
                    RES_R8(1, ref _a);
                    };
                opcode_cb[0x90] = delegate() { // RES 2, B
                    RES_R8(2, ref _b);
                    };
                opcode_cb[0x91] = delegate() { // RES 2, C
                    RES_R8(2, ref _c);
                    };
                opcode_cb[0x92] = delegate() { // RES 2, D
                    RES_R8(2, ref _d);
                    };
                opcode_cb[0x93] = delegate() { // RES 2, E
                    RES_R8(2, ref _e);
                    };
                opcode_cb[0x94] = delegate() { // RES 2, H
                    RES_R8(2, ref _h);
                    };
                opcode_cb[0x95] = delegate() { // RES 2, L
                    RES_R8(2, ref _l);
                    };
                opcode_cb[0x96] = delegate() { // RES 2, (HL)
                    RES_PR16(2, _h, _l);
                    };
                opcode_cb[0x97] = delegate() { // RES 2, A
                    RES_R8(2, ref _a);
                    };
                opcode_cb[0x98] = delegate() { // RES 3, B
                    RES_R8(3, ref _b);
                    };
                opcode_cb[0x99] = delegate() { // RES 3, C
                    RES_R8(3, ref _c);
                    };
                opcode_cb[0x9A] = delegate() { // RES 3, D
                    RES_R8(3, ref _d);
                    };
                opcode_cb[0x9B] = delegate() { // RES 3, E
                    RES_R8(3, ref _e);
                    };
                opcode_cb[0x9C] = delegate() { // RES 3, H
                    RES_R8(3, ref _h);
                    };
                opcode_cb[0x9D] = delegate() { // RES 3, L
                    RES_R8(3, ref _l);
                    };
                opcode_cb[0x9E] = delegate() { // RES 3, (HL)
                    RES_PR16(3, _h, _l);
                    };
                opcode_cb[0x9F] = delegate() { // RES 3, A
                    RES_R8(3, ref _a);
                    };
                opcode_cb[0xA0] = delegate() { // RES 4, B
                    RES_R8(4, ref _b);
                    };
                opcode_cb[0xA1] = delegate() { // RES 4, C
                    RES_R8(4, ref _c);
                    };
                opcode_cb[0xA2] = delegate() { // RES 4, D
                    RES_R8(4, ref _d);
                    };
                opcode_cb[0xA3] = delegate() { // RES 4, E
                    RES_R8(4, ref _e);
                    };
                opcode_cb[0xA4] = delegate() { // RES 4, H
                    RES_R8(4, ref _h);
                    };
                opcode_cb[0xA5] = delegate() { // RES 4, L
                    RES_R8(4, ref _l);
                    };
                opcode_cb[0xA6] = delegate() { // RES 4, (HL)
                    RES_PR16(4, _h, _l);
                    };
                opcode_cb[0xA7] = delegate() { // RES 4, A
                    RES_R8(4, ref _a);
                    };
                opcode_cb[0xA8] = delegate() { // RES 5, B
                    RES_R8(5, ref _b);
                    };
                opcode_cb[0xA9] = delegate() { // RES 5, C
                    RES_R8(5, ref _c);
                    };
                opcode_cb[0xAA] = delegate() { // RES 5, D
                    RES_R8(5, ref _d);
                    };
                opcode_cb[0xAB] = delegate() { // RES 5, E
                    RES_R8(5, ref _e);
                    };
                opcode_cb[0xAC] = delegate() { // RES 5, H
                    RES_R8(5, ref _h);
                    };
                opcode_cb[0xAD] = delegate() { // RES 5, L
                    RES_R8(5, ref _l);
                    };
                opcode_cb[0xAE] = delegate() { // RES 5, (HL)
                    RES_PR16(5, _h, _l);
                    };
                opcode_cb[0xAF] = delegate() { // RES 5, A
                    RES_R8(5, ref _a);
                    };
                opcode_cb[0xB0] = delegate() { // RES 6, B
                    RES_R8(6, ref _b);
                    };
                opcode_cb[0xB1] = delegate() { // RES 6, C
                    RES_R8(6, ref _c);
                    };
                opcode_cb[0xB2] = delegate() { // RES 6, D
                    RES_R8(6, ref _d);
                    };
                opcode_cb[0xB3] = delegate() { // RES 6, E
                    RES_R8(6, ref _e);
                    };
                opcode_cb[0xB4] = delegate() { // RES 6, H
                    RES_R8(6, ref _h);
                    };
                opcode_cb[0xB5] = delegate() { // RES 6, L
                    RES_R8(6, ref _l);
                    };
                opcode_cb[0xB6] = delegate() { // RES 6, (HL)
                    RES_PR16(6, _h, _l);
                    };
                opcode_cb[0xB7] = delegate() { // RES 6, A
                    RES_R8(6, ref _a);
                    };
                opcode_cb[0xB8] = delegate() { // RES 7, B
                    RES_R8(7, ref _b);
                    };
                opcode_cb[0xB9] = delegate() { // RES 7, C
                    RES_R8(7, ref _c);
                    };
                opcode_cb[0xBA] = delegate() { // RES 7, D
                    RES_R8(7, ref _d);
                    };
                opcode_cb[0xBB] = delegate() { // RES 7, E
                    RES_R8(7, ref _e);
                    };
                opcode_cb[0xBC] = delegate() { // RES 7, H
                    RES_R8(7, ref _h);
                    };
                opcode_cb[0xBD] = delegate() { // RES 7, L
                    RES_R8(7, ref _l);
                    };
                opcode_cb[0xBE] = delegate() { // RES 7, (HL)
                    RES_PR16(7, _h, _l);
                    };
                opcode_cb[0xBF] = delegate() { // RES 7, A
                    RES_R8(7, ref _a);
                    };
                opcode_cb[0xC0] = delegate() { // SET 0, B
                    SET_R8(0, ref _b);
                    };
                opcode_cb[0xC1] = delegate() { // SET 0, C
                    SET_R8(0, ref _c);
                    };
                opcode_cb[0xC2] = delegate() { // SET 0, D
                    SET_R8(0, ref _d);
                    };
                opcode_cb[0xC3] = delegate() { // SET 0, E
                    SET_R8(0, ref _e);
                    };
                opcode_cb[0xC4] = delegate() { // SET 0, H
                    SET_R8(0, ref _h);
                    };
                opcode_cb[0xC5] = delegate() { // SET 0, L
                    SET_R8(0, ref _l);
                    };
                opcode_cb[0xC6] = delegate() { // SET 0, (HL)
                    SET_PR16(0, _h, _l);
                    };
                opcode_cb[0xC7] = delegate() { // SET 0, A
                    SET_R8(0, ref _a);
                    };
                opcode_cb[0xC8] = delegate() { // SET 1, B
                    SET_R8(1, ref _b);
                    };
                opcode_cb[0xC9] = delegate() { // SET 1, C
                    SET_R8(1, ref _c);
                    };
                opcode_cb[0xCA] = delegate() { // SET 1, D
                    SET_R8(1, ref _d);
                    };
                opcode_cb[0xCB] = delegate() { // SET 1, E
                    SET_R8(1, ref _e);
                    };
                opcode_cb[0xCC] = delegate() { // SET 1, H
                    SET_R8(1, ref _h);
                    };
                opcode_cb[0xCD] = delegate() { // SET 1, L
                    SET_R8(1, ref _l);
                    };
                opcode_cb[0xCE] = delegate() { // SET 1, (HL)
                    SET_PR16(1, _h, _l);
                    };
                opcode_cb[0xCF] = delegate() { // SET 1, A
                    SET_R8(1, ref _a);
                    };
                opcode_cb[0xD0] = delegate() { // SET 2, B
                    SET_R8(2, ref _b);
                    };
                opcode_cb[0xD1] = delegate() { // SET 2, C
                    SET_R8(2, ref _c);
                    };
                opcode_cb[0xD2] = delegate() { // SET 2, D
                    SET_R8(2, ref _d);
                    };
                opcode_cb[0xD3] = delegate() { // SET 2, E
                    SET_R8(2, ref _e);
                    };
                opcode_cb[0xD4] = delegate() { // SET 2, H
                    SET_R8(2, ref _h);
                    };
                opcode_cb[0xD5] = delegate() { // SET 2, L
                    SET_R8(2, ref _l);
                    };
                opcode_cb[0xD6] = delegate() { // SET 2, (HL)
                    SET_PR16(2, _h, _l);
                    };
                opcode_cb[0xD7] = delegate() { // SET 2, A
                    SET_R8(2, ref _a);
                    };
                opcode_cb[0xD8] = delegate() { // SET 3, B
                    SET_R8(3, ref _b);
                    };
                opcode_cb[0xD9] = delegate() { // SET 3, C
                    SET_R8(3, ref _c);
                    };
                opcode_cb[0xDA] = delegate() { // SET 3, D
                    SET_R8(3, ref _d);
                    };
                opcode_cb[0xDB] = delegate() { // SET 3, E
                    SET_R8(3, ref _e);
                    };
                opcode_cb[0xDC] = delegate() { // SET 3, H
                    SET_R8(3, ref _h);
                    };
                opcode_cb[0xDD] = delegate() { // SET 3, L
                    SET_R8(3, ref _l);
                    };
                opcode_cb[0xDE] = delegate() { // SET 3, (HL)
                    SET_PR16(3, _h, _l);
                    };
                opcode_cb[0xDF] = delegate() { // SET 3, A
                    SET_R8(3, ref _a);
                    };
                opcode_cb[0xE0] = delegate() { // SET 4, B
                    SET_R8(4, ref _b);
                    };
                opcode_cb[0xE1] = delegate() { // SET 4, C
                    SET_R8(4, ref _c);
                    };
                opcode_cb[0xE2] = delegate() { // SET 4, D
                    SET_R8(4, ref _d);
                    };
                opcode_cb[0xE3] = delegate() { // SET 4, E
                    SET_R8(4, ref _e);
                    };
                opcode_cb[0xE4] = delegate() { // SET 4, H
                    SET_R8(4, ref _h);
                    };
                opcode_cb[0xE5] = delegate() { // SET 4, L
                    SET_R8(4, ref _l);
                    };
                opcode_cb[0xE6] = delegate() { // SET 4, (HL)
                    SET_PR16(4, _h, _l);
                    };
                opcode_cb[0xE7] = delegate() { // SET 4, A
                    SET_R8(4, ref _a);
                    };
                opcode_cb[0xE8] = delegate() { // SET 5, B
                    SET_R8(5, ref _b);
                    };
                opcode_cb[0xE9] = delegate() { // SET 5, C
                    SET_R8(5, ref _c);
                    };
                opcode_cb[0xEA] = delegate() { // SET 5, D
                    SET_R8(5, ref _d);
                    };
                opcode_cb[0xEB] = delegate() { // SET 5, E
                    SET_R8(5, ref _e);
                    };
                opcode_cb[0xEC] = delegate() { // SET 5, H
                    SET_R8(5, ref _h);
                    };
                opcode_cb[0xED] = delegate() { // SET 5, L
                    SET_R8(5, ref _l);
                    };
                opcode_cb[0xEE] = delegate() { // SET 5, (HL)
                    SET_PR16(5, _h, _l);
                    };
                opcode_cb[0xEF] = delegate() { // SET 5, A
                    SET_R8(5, ref _a);
                    };
                opcode_cb[0xF0] = delegate() { // SET 6, B
                    SET_R8(6, ref _b);
                    };
                opcode_cb[0xF1] = delegate() { // SET 6, C
                    SET_R8(6, ref _c);
                    };
                opcode_cb[0xF2] = delegate() { // SET 6, D
                    SET_R8(6, ref _d);
                    };
                opcode_cb[0xF3] = delegate() { // SET 6, E
                    SET_R8(6, ref _e);
                    };
                opcode_cb[0xF4] = delegate() { // SET 6, H
                    SET_R8(6, ref _h);
                    };
                opcode_cb[0xF5] = delegate() { // SET 6, L
                    SET_R8(6, ref _l);
                    };
                opcode_cb[0xF6] = delegate() { // SET 6, (HL)
                    SET_PR16(6, _h, _l);
                    };
                opcode_cb[0xF7] = delegate() { // SET 6, A
                    SET_R8(6, ref _a);
                    };
                opcode_cb[0xF8] = delegate() { // SET 7, B
                    SET_R8(7, ref _b);
                    };
                opcode_cb[0xF9] = delegate() { // SET 7, C
                    SET_R8(7, ref _c);
                    };
                opcode_cb[0xFA] = delegate() { // SET 7, D
                    SET_R8(7, ref _d);
                    };
                opcode_cb[0xFB] = delegate() { // SET 7, E
                    SET_R8(7, ref _e);
                    };
                opcode_cb[0xFC] = delegate() { // SET 7, H
                    SET_R8(7, ref _h);
                    };
                opcode_cb[0xFD] = delegate() { // SET 7, L
                    SET_R8(7, ref _l);
                    };
                opcode_cb[0xFE] = delegate() { // SET 7, (HL)
                    SET_PR16(7, _h, _l);
                    };
                opcode_cb[0xFF] = delegate() { // SET 7, A
                    SET_R8(7, ref _a);
                    };                
            #endregion
        }

        public int ExecuteOp()
        {
            byte op = ReadByte(Pc);
            if (!Pause) NotifyAll(new NotifyData("CPU_EXECUTE", Pc));

            cyclPrefixCB = 0;
            flagRegModifiedDirectly = false;

            if (!Running || WaitForInterrupt || Pause) return 4;

            _f = (FlagZ ? 0x80 : 0) | (FlagN ? 0x40 : 0) | (FlagHc ? 0x20 : 0) | (FlagC ? 0x10 : 0);
            _actionTaken = false;

            Pc &= 0xFFFF;

            opcode[op]();

            if (flagRegModifiedDirectly)
            {
                FlagZ = ((_f >> 7) & 1) == 1;
                FlagN = ((_f >> 6) & 1) == 1;
                FlagHc = ((_f >> 5) & 1) == 1;
                FlagC = ((_f >> 4) & 1) == 1;
            }

            if (_actionTaken)
                return CycleTableAT[op] + cyclPrefixCB;
            return CycleTable[op] + cyclPrefixCB;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExecutePrefixCb()
        {
            int op = ReadByte(Pc);
            opcode_cb[op]();
            Pc++;
            return CycleTableCB[op];
        }

        // Stack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void push_internal(int v)
        {
            Sp -= 2;
            WriteShort(Sp, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int pop_internal()
        {
            int v = ReadShort(Sp);
            Sp += 2;
            return v;
        }

        // Interrupting
        public void Interrupt(int address)
        {
            WaitForInterrupt = false;
            Ime = false;
            push_internal(Pc);
            Pc = address;
        }

        // Memory Helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadByte(int address)
        {
            NotifyAll(new NotifyData("CPU_READ", address));
            return Memory.ReadByte(address);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort ReadShort(int address)
        {
            return (ushort)((Memory.ReadByte(address + 1) << 8) + Memory.ReadByte(address));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteByte(int address, int value)
        {
            NotifyAll(new NotifyData("CPU_WRITE", address));
            Memory.WriteByte(address, (byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteShort(int address, int value)
        {
            WriteByte(address, value & 0xFF);
            WriteByte(address + 1, value >> 8);
        }

        #region Opcode implementation

        // LEGENDE:
        // ~ R8   : 8-bit Register
        // ~ R16  : 16-bit Register
        // ~ PR8  : 0xFF00 + Pointer aus 8-bit Register
        // ~ PR16 : Pointer aus 16-bit Register
        // ~ D8   : Byte Data
        // ~ D16  : Short Data
        // ~ A8   : 8-bit Pointer (0xFF00 + A8)
        // ~ A16  : 16-bit Pointer
        // ~ S8   : 8-bit signed relative data

        #region Interrupts

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reti()
        {
            Ime = true;
            Pc = pop_internal();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Ei()
        {
            Ime = true;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Di()
        {
            Ime = false;
            Pc++;
        }

        #endregion
        //OK
        #region Stack

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(int rh, int rl)
        {
            push_internal((rh << 8) + rl);
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Pop(ref int rh, ref int rl) //May be realized with out statements
        {
            int v = pop_internal();
            rh = v >> 8;
            rl = v & 0xFF;
            Pc++;
        }

        #endregion // OK
        //OK
        #region Compare

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CP_R8(int ra)
        {
            FlagN = true;
            FlagHc = (_a & 0xF) < (ra & 0xF);
            FlagC = ra > _a;
            FlagZ = ((_a - ra) & 0xFF) == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CP_PR16(int rah, int ral)
        {
            int v = ReadByte((rah << 8) + ral);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            FlagZ = ((_a - v) & 0xFF) == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CP_D8()
        {
            int v = ReadByte(Pc + 1);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            FlagZ = ((_a - v) & 0xFF) == 0;
            Pc += 2;
        }

        #endregion
        //OK
        #region Program control (Jumps, etc.)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JR_S8()
        {
            int relAddr = ReadByte(Pc + 1);
            if (relAddr > 0x7F)
            {
                relAddr -= 256;
            }
            Pc += relAddr + 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JRNZ_S8()
        {
            if (!FlagZ)
            {
                _actionTaken = true;
                JR_S8();
            } else {
                Pc += 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JRZ_S8()
        {
            if (FlagZ)
            {
                _actionTaken = true;
                JR_S8();
            } else {
                Pc += 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JRNC_S8()
        {
            if (!FlagC)
            {
                _actionTaken = true;
                JR_S8();
            } else {
                Pc += 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JRC_S8()
        {
            if (FlagC)
            {
                _actionTaken = true;
                JR_S8();
            } else {
                Pc += 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JP_A16()
        {
            Pc = ReadShort(Pc + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JPNZ_A16()
        {
            if (!FlagZ)
            {
                _actionTaken = true;
                JP_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JPZ_A16()
        {
            if (FlagZ)
            {
                _actionTaken = true;
                JP_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JPNC_A16()
        {
            if (!FlagC)
            {
                _actionTaken = true;
                JP_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JPC_A16()
        {
            if (FlagC)
            {
                _actionTaken = true;
                JP_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JP_R16(int rh, int rl)
        {
            Pc = (rh << 8) + rl;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CALL_A16()
        {
            push_internal(Pc + 3);
            Pc = ReadShort(Pc + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CALLNZ_A16()
        {
            if (!FlagZ)
            {
                _actionTaken = true;
                CALL_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CALLZ_A16()
        {
            if (FlagZ)
            {
                _actionTaken = true;
                CALL_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CALLNC_A16()
        {
            if (!FlagC)
            {
                _actionTaken = true;
                CALL_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CALLC_A16()
        {
            if (FlagC)
            {
                _actionTaken = true;
                CALL_A16();
            } else {
                Pc += 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Halt()
        {
            WaitForInterrupt = true;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Ret()
        {
            Pc = pop_internal();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Retnz()
        {
            if (!FlagZ)
            {
                _actionTaken = true;
                Ret();
            } else {
                Pc++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Retz()
        {
            if (FlagZ)
            {
                _actionTaken = true;
                Ret();
            } else {
                Pc++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Retnc()
        {
            if (!FlagC)
            {
                _actionTaken = true;
                Ret();
            } else {
                Pc++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Retc()
        {
            if (FlagC)
            {
                _actionTaken = true;
                Ret();
            } else {
                Pc++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rst(int address)
        {
            push_internal(Pc + 1);
            Pc = address;
        }

        #endregion

        #region LOAD

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R8_R8(ref int ra, int rb)
        {
            ra = rb;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R16_D16(ref int rh, ref int rl)
        {
            rh = ReadByte(Pc + 2);
            rl = ReadByte(Pc + 1);
            Pc += 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_PR16_R8(int rah, int ral, int rb)
        {
            WriteByte((rah << 8) + ral, rb);
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R8_D8(ref int r)
        {
            r = ReadByte(Pc + 1);
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_A16_R8(int r)
        {
            WriteByte(ReadShort(Pc + 1), r);
            Pc += 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_A16_R16(int rh, int rl)
        {
            WriteShort(ReadShort(Pc + 1), (rh << 8) + rl);
            Pc += 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R8_A16(ref int r)
        {
            r = ReadByte(ReadShort(Pc + 1));
            Pc += 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R8_PR16(ref int r, int rbh, int rbl)
        {
            r = ReadByte((rbh << 8) + rbl);
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_PR16_D8(int rah, int ral)
        {
            WriteByte((rah << 8) + ral, ReadByte(Pc + 1));
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_SP_D16()
        {
            Sp = ReadShort(Pc + 1);
            Pc += 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_SP_R16(int rh, int rl)
        {
            Sp = (rh << 8) + rl;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R16_SP_S8(ref int rh, ref int rl)
        {
            int v = ReadByte(Pc + 1);
            if (v > 0x7F)
            {
                v -= 256;
            }

            if (v >= 0) // Addition
            {
                FlagHc = (Sp & 0xF) + (v & 0xF) > 0xF;
                FlagC = (Sp & 0xFF) + v > 0xFF;
                FlagN = false;
            } else { // Substraction
                int vp = v * -1;
                FlagHc = (Sp & 0xF) < (vp & 0xF);
                FlagC = vp > (Sp & 0xFF);
                FlagN = true;
            }

            int result = (Sp + v) & 0xFFFF;
            rl = result & 0xFF;
            rh = result >> 8;

            /*rl = (Sp & 0xFF) + v;
            //FlagHc = (Sp & 0xF) + (v & 0xF) > 0xF;
            bool overflow = rl > 0xFF;
            rl &= 0xFF;
            rh = (Sp >> 8) + (overflow ? 1 : 0);
            //FlagC = rh > 0xFF;
            rh &= 0xFF;*/
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LDI_PR16_R8(ref int rah, ref int ral, int rb)
        {
            LD_PR16_R8(rah, ral, rb);
            INC_R16(ref rah, ref ral);
            Pc--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LDI_R8_PR16(ref int ra, ref int rbh, ref int rbl)
        {
            LD_R8_PR16(ref ra, rbh, rbl);
            INC_R16(ref rbh, ref rbl);
            Pc--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LDD_PR16_R8(ref int rah, ref int ral, int rb)
        {
            LD_PR16_R8(rah, ral, rb);
            DEC_R16(ref rah, ref ral);
            Pc--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LDD_R8_PR16(ref int ra, ref int rbh, ref int rbl)
        {
            LD_R8_PR16(ref ra, rbh, rbl);
            DEC_R16(ref rbh, ref rbl);
            Pc--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_A8_R8(int ra)
        {
            WriteByte(0xFF00 + ReadByte(Pc + 1), ra);
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R8_A8(ref int ra)
        {
            ra = ReadByte(0xFF00 + ReadByte(Pc + 1));
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_PR8_R8(int ra, int rb)
        {
            WriteByte(0xFF00 + ra, rb);
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LD_R8_PR8(ref int ra, int rb)
        {
            ra = ReadByte(0xFF00 + rb);
            Pc++;
        }

        #endregion

        #region MATH

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void INC_R16(ref int rh, ref int rl)
        {
            if (rl == 0xFF)
            {
                rh = (rh + 1) & 0xFF;
                rl = 0;
            } else {
                rl++;
            }
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void INC_R8(ref int r)
        {
            FlagN = false;
            FlagHc = (r & 0xF) == 0xF;
            r++;
            r &= 0xFF;
            FlagZ = r == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void INC_PR16(int rh, int rl)
        {
            int addr = (rh << 8) + rl;
            int vOrig = ReadByte(addr);
            int v = (vOrig + 1) & 0xFF;
            WriteByte(addr, v);
            FlagZ = v == 0;
            FlagN = false;
            FlagHc = (vOrig & 0xF) == 0xF;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void INC_SP()
        {
            if (Sp == 0xFFFF)
            {
                Sp = 0;
            } else {
                Sp++;
            }
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DEC_R16(ref int RH, ref int RL)
        {
            if (RL == 0)
            {
                RH = (RH - 1) & 0xFF;
                RL = 0xFF;
            } else {
                RL--;
            }
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DEC_R8(ref int R)
        {
            FlagN = true;
            FlagHc = (R & 0xF) == 0x0;
            R--;
            R &= 0xFF;
            FlagZ = R == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DEC_PR16(int rh, int rl)
        {
            int addr = (rh << 8) + rl;
            int vOrig = ReadByte(addr);
            int v = (vOrig - 1) & 0xFF;
            WriteByte(addr, v);
            FlagZ = v == 0;
            FlagN = true;
            FlagHc = (vOrig & 0xF) == 0x0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADD_R16_R16(ref int rah, ref int ral, int rbh, int rbl)
        {
            FlagN = false;
            ral += rbl;
            int carry = (ral > 0xFF) ? 1 : 0; //Moved declaration to assignment
            ral &= 0xFF;
            FlagHc = carry + (rah & 0xF) + (rbh & 0xF) > 0xF;
            rah += rbh + carry;
            FlagC = rah > 0xFF;
            rah &= 0xFF;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADD_R8_R8(ref int ra, int rb)
        {
            FlagN = false;
            FlagHc = (ra & 0xF) + (rb & 0xF) > 0xF;
            ra += rb;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADD_R8_PR16(ref int ra, int rbh, int rbl)
        {
            int v = ReadByte((rbh << 8) + rbl);
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) > 0xF;
            ra += v;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADD_R8_D8(ref int ra)
        {
            int v = ReadByte(Pc + 1);
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) > 0xF;
            ra += v;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADD_SP_S8()
        {
            int v = ReadByte(Pc + 1);
            if (v > 0x7F)
            {
                v -= 256;
            }
            FlagZ = (Sp + v) == 0;
            FlagN = v < 0;
            FlagHc = (Sp & 0xF) + (v & 0xF) > 0xF;
            FlagC = (Sp & 0xFF) + v > 0xFF;
            Sp += v;
            Sp &= 0xFFFF;
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADC_R8_R8(ref int ra, int rb)
        {
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = (ra & 0xF) + (rb & 0xF) + carryBit > 0xF;
            ra += rb + carryBit;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADC_R8_PR16(ref int ra, int rbh, int rbl)
        {
            int v = ReadByte((rbh << 8) + rbl);
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) + carryBit > 0xF;
            ra += v + carryBit;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ADC_R8_D8(ref int ra)
        {
            int v = ReadByte(Pc + 1);
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = (ra & 0xF) + (v & 0xF) + carryBit > 0xF;
            ra += v + carryBit;
            FlagC = ra > 0xFF;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SUB_R8(int ra)
        {
            FlagN = true;
            FlagHc = (_a & 0xF) < (ra & 0xF);
            FlagC = ra > _a;
            _a -= ra;
            _a &= 0xFF;
            FlagZ = _a == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SUB_PR16(int rah, int ral)
        {
            int v = ReadByte((rah << 8) + ral);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            _a -= v;
            _a &= 0xFF;
            FlagZ = _a == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SUB_D8()
        {
            int v = ReadByte(Pc + 1);
            FlagN = true;
            FlagHc = (_a & 0xF) < (v & 0xF);
            FlagC = v > _a;
            _a -= v;
            _a &= 0xFF;
            FlagZ = _a == 0;
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SBC_R8_R8(ref int ra, int rb)
        {
            int carryBit = FlagC ? 1 : 0;
            FlagN = true;
            FlagHc = (ra & 0xF) < (rb & 0xF) + carryBit;
            FlagC = rb + carryBit > ra;
            ra -= rb + carryBit;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SBC_R8_PR16(ref int ra, int rah, int ral)
        {
            int carryBit = FlagC ? 1 : 0;
            int v = ReadByte((rah << 8) + ral);
            FlagN = true;
            FlagHc = (ra & 0xF) < ((v & 0xF) + carryBit);
            FlagC = v + carryBit > ra;
            ra -= v + carryBit;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SBC_R8_D8(ref int ra)
        {
            int carryBit = FlagC ? 1 : 0;
            int v = ReadByte(Pc + 1);
            FlagN = true;
            FlagHc = (ra & 0xF) < (v & 0xF) + carryBit;
            FlagC = v + carryBit > ra;
            ra -= v + carryBit;
            ra &= 0xFF;
            FlagZ = ra == 0;
            Pc += 2;
        }

        #endregion

        #region BIT OPERATIONS

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rlca()
        {
            int highBit = _a >> 7;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = highBit == 1;
            _a = ((_a << 1) & 0xFF) | highBit;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rrca()
        {
            int lowBit = _a & 1;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = lowBit == 1;
            _a = (_a >> 1) | (lowBit << 7);
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rla()
        {
            int carryBit = FlagC ? 1 : 0;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = (_a >> 7) == 1;
            _a = ((_a << 1) & 0xFF) | carryBit;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rra()
        {
            int carryBit = FlagC ? 0x80 : 0x00;
            FlagZ = false;
            FlagN = false;
            FlagHc = false;
            FlagC = (_a & 1) == 1;
            _a = (_a >> 1) | carryBit;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Cpl()
        {
            FlagHc = true;
            FlagN = true;
            _a ^= 0xFF;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AND_R8(int ra)
        {
            _a &= ra;
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = true;
            FlagC = false;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AND_PR16(int rah, int ral)
        {
            _a &= ReadByte((rah << 8) + ral);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = true;
            FlagC = false;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AND_D8()
        {
            _a &= ReadByte(Pc + 1);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = true;
            FlagC = false;
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void XOR_R8(int ra)
        {
            _a ^= ra;
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void XOR_PR16(int rah, int ral)
        {
            _a ^= ReadByte((rah << 8) + ral);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void XOR_D8()
        {
            _a ^= ReadByte(Pc + 1);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OR_R8(int ra)
        {
            _a |= ra;
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OR_PR16(int rah, int ral)
        {
            _a |= ReadByte((rah << 8) + ral);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OR_D8()
        {
            _a |= ReadByte(Pc + 1);
            FlagZ = _a == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            Pc += 2;
        }

        #endregion

        #region Flags

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Scf()
        {
            FlagN = false;
            FlagHc = false;
            FlagC = true;
            Pc++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Ccf()
        {
            FlagC = !FlagC;
            FlagHc = false;
            FlagN = false;
            Pc++;
        }

        #endregion

        #region BCD

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Daa()
        {
            if (!FlagN)
            {
                if (FlagHc || (_a & 0xF) > 9)
                    _a += 0x6;
                if (FlagC || _a >> 8 > 0x9)
                    _a += 0x60;
            } else {
                if (FlagHc)
                    _a = (_a - 6) & 0xFF;
                if (FlagC)
                    _a -= 0x60;
            }

            FlagC = _a > 0xFF;
            FlagHc = false;

            _a &= 0xFF;
            FlagZ = _a == 0;

            Pc++;
        }

        #endregion

        #region Prefix CB

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RLC_R8(ref int r)
        {
            int highBit = r >> 7;
            FlagN = false;
            FlagHc = false;
            FlagC = highBit == 1;
            r = ((r << 1) & 0xFF) | highBit;
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RLC_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int highBit = v >> 7;
            FlagN = false;
            FlagHc = false;
            FlagC = highBit == 1;
            v = ((v << 1) & 0xFF) | highBit;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RRC_R8(ref int r)
        {
            int lowBit = r & 1;
            FlagN = false;
            FlagHc = false;
            FlagC = lowBit == 1;
            r = (r >> 1) | (lowBit << 7);
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RRC_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int lowBit = v & 1;
            FlagN = false;
            FlagHc = false;
            FlagC = lowBit == 1;
            v = (v >> 1) | (lowBit << 7);
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RL_R8(ref int r)
        {
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = false;
            FlagC = (r >> 7) == 1;
            r = ((r << 1) & 0xFF) | carryBit;
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RL_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int carryBit = FlagC ? 1 : 0;
            FlagN = false;
            FlagHc = false;
            FlagC = (v >> 7) == 1;
            v = ((v << 1) & 0xFF) | carryBit;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RR_R8(ref int r)
        {
            int carryBit = FlagC ? 0x80 : 0x00;
            FlagN = false;
            FlagHc = false;
            FlagC = (r & 1) == 1;
            r = (r >> 1) | carryBit;
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RR_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            int carryBit = FlagC ? 0x80 : 0x00;
            FlagN = false;
            FlagHc = false;
            FlagC = (v & 1) == 1;
            v = (v >> 1) | carryBit;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SLA_R8(ref int r)
        {
            FlagN = false;
            FlagHc = false;
            FlagC = (r >> 7) == 1;
            r <<= 1;
            r &= 0xFF;
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SLA_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            FlagN = false;
            FlagHc = false;
            FlagC = (v >> 7) == 1;
            v <<= 1;
            v &= 0xFF;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SRA_R8(ref int r)
        {
            FlagN = false;
            FlagHc = false;
            FlagC = (r & 0x01) == 1;
            r = (r & 0x80) | r >> 1;
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SRA_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            FlagN = false;
            FlagHc = false;
            FlagC = (v & 0x01) == 1;
            v = (v & 0x80) | v >> 1;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SRL_R8(ref int r)
        {
            FlagN = false;
            FlagHc = false;
            FlagC = (r & 1) == 1;
            r >>= 1;
            r &= 0xFF;
            FlagZ = r == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SRL_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            FlagN = false;
            FlagHc = false;
            FlagC = (v & 1) == 1;
            v >>= 1;
            v &= 0xFF;
            FlagZ = v == 0;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SWAP_R8(ref int r)
        {
            r = ((r << 4) | (r >> 4)) & 0xFF;
            FlagZ = r == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SWAP_PR16(int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            v = ((v << 4) | (v >> 4)) & 0xFF;
            FlagZ = v == 0;
            FlagN = false;
            FlagHc = false;
            FlagC = false;
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BIT_R8(int n, int r)
        {
            FlagZ = (r & (1 << n)) == 0;
            FlagN = false;
            FlagHc = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BIT_PR16(int n, int rh, int rl)
        {
            FlagZ = (ReadByte((rh << 8) + rl) & (1 << n)) == 0;
            FlagN = false;
            FlagHc = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RES_R8(int n, ref int r)
        {
            r = r & (0xFF - (1 << n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RES_PR16(int n, int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            v = v & (0xFF - (1 << n));
            WriteByte(address, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SET_R8(int n, ref int r)
        {
            r |= (1 << n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SET_PR16(int n, int rh, int rl)
        {
            int address = (rh << 8) + rl;
            int v = ReadByte(address);
            v = v | (1 << n);
            WriteByte(address, v);
        }

        #endregion

        #endregion

    }
}
