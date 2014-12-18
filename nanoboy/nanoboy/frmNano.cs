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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using nanoboy.Core;

namespace nanoboy
{
    public partial class frmNano : Form
    {

        private Gameboy gb;

        public frmNano()
        {
            InitializeComponent();
        }

        #region "Menu"
        private void menuOpen_Click(object sender, EventArgs e)
        {
            if (openRom.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Rom rom = new Rom(openRom.FileName, "BATTERY\\" +  Path.GetFileNameWithoutExtension(openRom.FileName) + ".sav");
                gb = new Gameboy(rom, false);
                gameRunner.Start();
            }
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }
        #endregion

        #region "Update"
        private void gameRunner_Tick(object sender, EventArgs e)
        {
            gameView.Image = this.ResizeImage(gb.GetVideo(), gameView.Size, false);
            gb.Step();
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
        #endregion

        #region Joypad
        private void frmNano_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (gb != null)
                gb.SetKey(e.KeyCode);
        }

        private void frmNano_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.gb != null)
                gb.ClearKey(e.KeyCode);
        }
        #endregion

    }
}
