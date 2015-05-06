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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using nanoboy.Core;

using System.Threading;
using System.Diagnostics;

namespace nanoboy
{
    public partial class frmNano : Form
    {

        private Nanoboy nano;
        private Thread gamethread;

        public frmNano()
        {
            InitializeComponent();
            frmNano.CheckForIllegalCrossThreadCalls = false;
        }

        #region "Menu"
        private void menuOpen_Click(object sender, EventArgs e)
        {
            if (gamethread != null) {
                gamethread.Abort();
                nano.Dispose();
            }
            if (openRom.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ROM rom = new ROM(openRom.FileName, "BATTERY\\" +  Path.GetFileNameWithoutExtension(openRom.FileName) + ".sav");
                nano = new Nanoboy(rom);
                //gameRunner.Start();
                gamethread = new Thread(delegate() {
                    Stopwatch stopwatch = new Stopwatch();
                    while (true) {
                        stopwatch.Reset();
                        stopwatch.Start();
                        nano.Frame();
                        this.gameView.Image = this.ResizeImage(nano.Image, gameView.Size, false);
                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds < 13) {
                            Thread.Sleep(13 - (int)stopwatch.ElapsedMilliseconds);
                        }
                    }
                });
                gamethread.Priority = ThreadPriority.Highest;
                gamethread.Start();
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
            nano.Frame();
            gameView.Image = this.ResizeImage(nano.Image, gameView.Size, false);
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
            if (nano != null) {
                nano.SetKey(e.KeyCode);
            }
        }

        private void frmNano_KeyUp(object sender, KeyEventArgs e)
        {
            if (nano != null) {
                nano.UnsetKey(e.KeyCode);
            }
        }
        #endregion

        private void frmNano_FormClosing(object sender, FormClosingEventArgs e)
        {
            gamethread.Abort();
        }

    }
}
