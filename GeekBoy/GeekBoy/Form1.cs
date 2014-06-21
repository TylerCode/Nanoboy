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
        bool emulateBios = true;

        public Form1()
        {
            InitializeComponent();
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
            if (gameboy != null)
                gameboy.Joypad.HandleInput(e.KeyCode, true);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (gameboy != null)
                gameboy.Joypad.HandleInput(e.KeyCode, false);
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
                // Load ROM
                rom = new Rom(openRom.FileName);
                gameboy = new Gameboy(rom, !emulateBios);
                timer1.Start();
                gameboy.MainCycle();
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void menuBIOS_Click(object sender, EventArgs e)
        {
            emulateBios = !emulateBios;
            menuBIOS.Checked = emulateBios;
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            frmAbout abt = new frmAbout();
            abt.ShowDialog();
        }

    }
}
