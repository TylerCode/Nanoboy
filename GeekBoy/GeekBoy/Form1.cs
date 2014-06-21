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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace GeekBoy
{
    public partial class Form1 : Form
    {
        Rom rom;
        Gameboy gameboy;
        Thread debugger;
        bool enableDebugger = false, emulateBios = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG
            Console.WriteLine("DEBUG: MainForm launched.");
#endif
        }

        private void Debugger()
        {
            while (true)
            {
                if (enableDebugger)
                {
                    string cmd;
                    string[] tokens;
                    int address, count;
                    Console.Write("root@debugger$> ");
                    cmd = Console.ReadLine(); // read next command
                    tokens = cmd.Split(' '); // tokenize command

                    if (tokens.Length > 0)
                    {
                        switch (tokens[0])
                        {
                            case "reg":
                                Console.WriteLine("AF: 0x{0:X} BC: 0x{1:X} DE: 0x{2:X} HL: 0x{3:X} SP: 0x{4:X} PC: 0x{5:X}",
                                                   (gameboy.Cpu.A << 8) + gameboy.Cpu.F,
                                                   (gameboy.Cpu.B << 8) + gameboy.Cpu.C,
                                                   (gameboy.Cpu.D << 8) + gameboy.Cpu.E,
                                                   (gameboy.Cpu.H << 8) + gameboy.Cpu.L,
                                                   gameboy.Cpu.Sp,
                                                   gameboy.Cpu.Pc);
                                break;
                            case "mem":
                                address = int.Parse(tokens[1].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                                count = int.Parse(tokens[2].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                                for (int y = 0; y <= count / 16; y++)
                                {
                                    for (int x = 0; x < 16 && y * 16 + x < count; x++)
                                    {
                                        Console.Write("{0:X2} ", gameboy.MemoryRouter.ReadByte(address + y * 16 + x));
                                    }
                                    Console.WriteLine();
                                }
                                break;
                            case "dis":
                                address = int.Parse(tokens[1].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                                count = int.Parse(tokens[2].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                                foreach (string opcode in gameboy.Disassembler.Disassemble(address, count))
                                    Console.WriteLine(opcode);
                                break;
                            default:
                                Console.WriteLine("Unrecognized command '{0}'", tokens[0]);
                                break;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = ResizeImage(gameboy.Video.Buffer, new Size(pictureBox1.Width, pictureBox1.Height), false);
        }

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                gameboy.Joypad.HandleInput(e.KeyCode, true);
            } catch {
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                gameboy.Joypad.HandleInput(e.KeyCode, false);
            } catch {
            }
        }

        private string ExtractFilename(string path)
        {
            string[] s = path.Split('\\');
            return s[s.Length - 1];
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            openRom.ShowDialog();
            if (File.Exists(openRom.FileName))
            {
                try
                {
                    // Load ROM
                    rom = new Rom(openRom.FileName);
                    gameboy = new Gameboy(rom, !emulateBios);
                    timer1.Start();
                    gameboy.MainCycle();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine("\nException: {0}", ex.Message);
#else
                    MessageBox.Show(string.Format("\nException: {0}", ex.Message), "GeekBoy - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                }
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void menuDebugger_Click(object sender, EventArgs e)
        {
            enableDebugger = !enableDebugger;
            menuDebugger.Checked = enableDebugger;
            if (enableDebugger)
            { debugger = new Thread(Debugger); debugger.Start(); }
            else debugger.Abort();
        }

        private void menuBIOS_Click(object sender, EventArgs e)
        {
            emulateBios = !emulateBios;
            menuBIOS.Checked = emulateBios;
        }

    }
}
