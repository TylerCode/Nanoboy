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
using System.IO;

namespace nanoboy.Core
{
    public class NoMbc : IMemoryDevice
    {
        private const int BANK_SIZE = 0x4000;

        private Mbc _romType;
        private byte[] _ram;
        private byte[,] _rom;
        private string _saveFile;

        public NoMbc(byte[] fileData, Mbc romType, int romSize, string saveFile)
        {
            _saveFile = saveFile;
            _romType = romType;
            int romBanks = romSize / BANK_SIZE;
            _rom = new byte[romBanks, BANK_SIZE];
            _ram = LoadSave();
            for (int i = 0, k = 0; i < romBanks; i++)
            {
                for (int j = 0; j < BANK_SIZE; j++, k++)
                {
                    _rom[i, j] = fileData[k];
                }
            }
        }

        public byte ReadByte(int address)
        {
            if (address <= 0x3FFF)
            {
                return _rom[0, address];
            } else if (address <= 0x7FFF)
            {
                return _rom[1, address - 0x4000];
            } else if (address <= 0xBFFF)
            {
                return _ram[address - 0xA000];
            }
            throw new Exception(string.Format("Invalid cartridge address: {0}", address));
        }

        public void WriteByte(int address, byte value)
        {
            if (address >= 0xA000 && address <= 0xBFFF)
            {
                _ram[address - 0xA000] = value;
                WriteSave(address, value);
            }
        }

        private byte[] LoadSave()
        {
            if (File.Exists(_saveFile))
            {
                return File.ReadAllBytes(_saveFile);
            } else {
                return new byte[0x2000];
            }
        }

        private void Save()
        {
            File.WriteAllBytes(_saveFile, this._ram);
        }

        private void WriteSave(int address, byte value)
        {
            Stream s = File.Open(_saveFile, FileMode.OpenOrCreate);

            while (s.Length < 0x2000)
                s.WriteByte(0);
            s.Flush();

            s.Position = address - 0xA000;
            s.WriteByte(value);
            s.Flush();
            
            s.Close();
            s.Dispose();
        }

    }

    public class Mbc1 : IMemoryDevice
    {
        private const int BANK_SIZE = 0x4000;

        private Mbc _romType;
        private int _selectedRomBank = 1;
        private int _selectedRamBank = 0;
        private byte[,] _ram;
        private byte[,] _rom;
        private bool _ram_mode = false;
        private int romBanks;
        private string _saveFile;

        public Mbc1(byte[] fileData, Mbc romType, int romSize, string saveFile)
        {
            _saveFile = saveFile;
            _romType = romType;
            romBanks = romSize / BANK_SIZE;
            _rom = new byte[romBanks, BANK_SIZE];
            _ram = LoadSave();
            for (int i = 0, k = 0; i < romBanks; i++)
            {
                for (int j = 0; j < BANK_SIZE; j++, k++)
                {
                    _rom[i, j] = fileData[k];
                }
            }
        }

        public byte ReadByte(int address)
        {
            if (address <= 0x3FFF)
            {
                return _rom[0, address];
            }
            if (address <= 0x7FFF)
            {
                return _rom[_selectedRomBank, address - 0x4000];
            }
            if (address <= 0xBFFF)
            {
                return _ram[_selectedRamBank, address - 0xA000];
            }
            throw new Exception(string.Format("Invalid cartridge address: {0}", address));
        }

        public void WriteByte(int address, byte value)
        {
            if (address <= 0x1FFF)
            {
            } else if (address <= 0x3FFF) {
                _selectedRomBank = value & 0x1F;
            } else if (address <= 0x5FFF) {
                if (_ram_mode)
                {
                    _selectedRamBank = value & 0x03;
                } else {
                    _selectedRomBank = (_selectedRomBank & 0x1F) | ((value & 3) << 5);
                }
            } else if (address <= 0x7FFF) {
                _ram_mode = value == 1;
            } else if (address >= 0xA000 && address <= 0xBFFF) {
                _ram[_selectedRamBank, address - 0xA000] = value;
                WriteSave(address, value);
            } else { throw new Exception(string.Format("Invalid cartridge address: {0}", address)); }
        }

        private byte[,] LoadSave()
        {
            byte[,] sdat = new byte[4, 0x2000];


            if (File.Exists(_saveFile))
            {
                byte[] data = File.ReadAllBytes(_saveFile);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0x0; j < 0x2000; j++)
                    {
                        sdat[i, j] = data[i * 0x2000 + j];
                    }
                }
            }

            return sdat;
        }

        private void WriteSave(int address, byte value)
        {
            Stream s = File.Open(_saveFile, FileMode.OpenOrCreate);

            while (s.Length < 0x8000)
                s.WriteByte(0);
            s.Flush();

            s.Position = _selectedRamBank * 0x2000 + address - 0xA000;
            s.WriteByte(value);

            s.Close();
            s.Dispose();
        }

    }

    public class Mbc3 : IMemoryDevice
    {
        private const int BANK_SIZE = 0x4000;

        private Mbc _romType;
        private int _selectedRomBank = 1;
        private int _selectedRamBank = 0;
        private byte[,] _ram;
        private byte[,] _rom;
        private bool _ramTimerEnable = false;
        private string _saveFile;

        public Mbc3(byte[] fileData, Mbc romType, int romSize, string saveFile)
        {
            int romBanks = romSize / BANK_SIZE;
            _saveFile = saveFile;
            _romType = romType;
            _rom = new byte[romBanks, BANK_SIZE];
            _ram = LoadSave();
            for (int i = 0, k = 0; i < romBanks; i++)
            {
                for (int j = 0; j < BANK_SIZE; j++, k++)
                {
                    _rom[i, j] = fileData[k];
                }
            }
        }

        public byte ReadByte(int address)
        {
            if (address >= 0 && address <= 0x3FFF)
            {
                return _rom[0, address];
            }
            if (address >= 0x4000 && address <= 0x7FFF)
            {
                try
                { 
                    return _rom[_selectedRomBank, address - 0x4000];
                } catch {
                    return 0;
                }
            }
            if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_ramTimerEnable && _selectedRamBank < 4) return _ram[_selectedRamBank, address - 0xA000];
                // IMPLEMENT RTC HERE
                return 0;
            }
            throw new Exception(string.Format("Invalid cartridge address: {0}", address));
        }

        public void WriteByte(int address, byte value)
        {
            if (address >= 0x0 && address <= 0x1FFF)
            {
                _ramTimerEnable = value == 0x0A;
            } else if (address >= 0x2000 && address <= 0x3FFF) {
                _selectedRomBank = value & 0x7F;
            } else if (address >= 0x4000 && address <= 0x5FFF) {
                _selectedRamBank = value;
            } else if (address >= 0x6000 && address <= 0x7FFF) {
                // IMPLEMENT RTC HERE
            } else if (address >= 0xA000 && address <= 0xBFFF) {
                if (_ramTimerEnable && _selectedRamBank < 4)
                {
                    _ram[_selectedRamBank, address - 0xA000] = value;
                    WriteSave(address, value);
                }
                // IMPLEMENT RTC HERE
            }
        }

        private byte[,] LoadSave()
        {
            byte[,] sdat = new byte[4, 0x2000];


            if (File.Exists(_saveFile))
            {
                byte[] data = File.ReadAllBytes(_saveFile);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0x0; j < 0x2000; j++)
                    {
                        sdat[i, j] = data[i * 0x2000 + j];
                    }
                }
            }

            return sdat;
        }

        private void WriteSave(int address, byte value)
        {
            Stream s = File.Open(_saveFile, FileMode.OpenOrCreate);

            while (s.Length < 0x8000)
                s.WriteByte(0);
            s.Flush();

            s.Position = _selectedRamBank * 0x2000 + address - 0xA000;
            s.WriteByte(value);

            s.Close();
            s.Dispose();
        }

    }


}
