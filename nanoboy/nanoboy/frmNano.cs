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

using OpenTK.Graphics.OpenGL;

namespace nanoboy
{
    public partial class frmNano : Form
    {

        private Nanoboy nano;
        private Thread gamethread;
        private bool preserveaspectratio;
        private bool loadedgl;
        private int textureid = -1;

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
                if (nano != null) {
                    nano.Dispose();
                }
                nano = new Nanoboy(rom);
                gamethread = new Thread(delegate() {
                    Stopwatch stopwatch = new Stopwatch();
                    while (true) {
                        stopwatch.Reset();
                        stopwatch.Start();
                        nano.Frame();
                        gameView.Refresh();
                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds < 16) {
                            Thread.Sleep(16 - (int)stopwatch.ElapsedMilliseconds);
                        }
                    }
                });
                gamethread.Priority = ThreadPriority.Highest;
                gamethread.Start();
            }
        }

        private void menuClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }


        private void menuSize1_Click(object sender, EventArgs e)
        {
            int diffwidth;
            int diffheight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            diffwidth = this.Width - gameView.Width;
            diffheight = this.Height - gameView.Height;
            this.Size = new Size(160 + diffwidth, 144 + diffheight);
        }

        private void menuSize2_Click(object sender, EventArgs e)
        {
            int diffwidth;
            int diffheight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            diffwidth = this.Width - gameView.Width;
            diffheight = this.Height - gameView.Height;
            this.Size = new Size(160 * 2 + diffwidth, 144 * 2 + diffheight);
        }

        private void menuSize3_Click(object sender, EventArgs e)
        {
            int diffwidth;
            int diffheight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            diffwidth = this.Width - gameView.Width;
            diffheight = this.Height - gameView.Height;
            this.Size = new Size(160 * 3 + diffwidth, 144 * 3 + diffheight);
        }

        private void menuSize4_Click(object sender, EventArgs e)
        {
            int diffwidth;
            int diffheight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            diffwidth = this.Width - gameView.Width;
            diffheight = this.Height - gameView.Height;
            this.Size = new Size(160 * 4 + diffwidth, 144 * 4 + diffheight);
        }

        private void menuSizeFull_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.FromControl(this).Bounds.Width, Screen.FromControl(this).Bounds.Height);
        }
        #endregion

        #region "Update"
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

        private void gameView_Load(object sender, EventArgs e)
        {
            loadedgl = true;
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            SetupViewport();
        }

        private void SetupViewport()
        {
            int width = gameView.Width;
            int height = gameView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, height, 0, -1, 1);
            GL.Viewport(0, 0, width, height);
        }

        private void gameView_Paint(object sender, PaintEventArgs e)
        {
            if (loadedgl && nano != null) {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                textureid = TextureFromArray(nano.Image, textureid);
                GL.BindTexture(TextureTarget.Texture2D, textureid);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.Begin(PrimitiveType.Quads);
                {
                    GL.TexCoord2(0f, 0f);
                    GL.Vertex2(0f, 0f);
                    GL.TexCoord2(1f, 0f);
                    GL.Vertex2(gameView.Width, 0f);
                    GL.TexCoord2(1f, 1f);
                    GL.Vertex2(gameView.Width, gameView.Height);
                    GL.TexCoord2(0f, 1f);
                    GL.Vertex2(0f, gameView.Height);
                }
                GL.End();

                gameView.SwapBuffers();
            }
        }

        public static int TextureFromArray(IntPtr arrayptr, int texturexid = -1)
        {
            int id = texturexid == -1 ? GL.GenTexture() : texturexid;
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 160, 144, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, arrayptr);
            return id;
        }

        private void gameView_Resize(object sender, EventArgs e)
        {
            SetupViewport();
        }
        #endregion

        #region Joypad
        private void gameView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (nano != null) {
                nano.SetKey(e.KeyCode);
            }
        }

        private void gameView_KeyUp(object sender, KeyEventArgs e)
        {
            if (nano != null) {
                nano.UnsetKey(e.KeyCode);
            }
        }
        #endregion

        private void frmNano_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (gamethread != null) {
                gamethread.Abort();
            }
            if (nano != null) {
                nano.Dispose();
            }
        }

        private void menuPreserveAspect_Click(object sender, EventArgs e)
        {
            preserveaspectratio = !preserveaspectratio;
            menuPreserveAspect.Checked = preserveaspectratio;
        }
    }
}
