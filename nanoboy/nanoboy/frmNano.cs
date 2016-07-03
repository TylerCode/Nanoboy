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
        private NanoboySettings settings;
        private Thread gamethread;
        private frmAudioTool audiotoolwindow;
        private bool loadedgl;
        private int textureid = -1;
        private bool speedup = false;
        private bool glerror = false;

        public frmNano()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            settings = new NanoboySettings();
            LoadConfiguration(); // set checkboxes according to the settings
        }

        #region "Menu"
        private void menuOpen_Click(object sender, EventArgs e)
        {
            if (gamethread != null) {
                updateTimer.Stop();
                gamethread.Abort();
                nano.Dispose();
            }
            if (openRom.ShowDialog() == DialogResult.OK)
            {
                ROM rom = new ROM(openRom.FileName, Path.ChangeExtension(openRom.FileName, "sav"));

                nano?.Dispose();
                nano = new Nanoboy(rom);
                nano.SetSettings(settings);

                if (audiotoolwindow != null) {
                    audiotoolwindow.Nanoboy = nano;
                }

                gamethread = new Thread(delegate() {
                    Stopwatch stopwatch = new Stopwatch();
                    while (true) {
                        stopwatch.Reset();
                        stopwatch.Start();
                        nano.Frame();
                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds < 16 && !speedup) {
                            Thread.Sleep(16 - (int)stopwatch.ElapsedMilliseconds);
                        }
                    }
                });
                gamethread.Priority = ThreadPriority.Highest;
                gamethread.Start();
                updateTimer.Start();
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
            ResizeWindow(1);
            LoadConfiguration();
        }

        private void menuSize2_Click(object sender, EventArgs e)
        {
            ResizeWindow(2);
            LoadConfiguration();
        }

        private void menuSize3_Click(object sender, EventArgs e)
        {
            ResizeWindow(3);
            LoadConfiguration();
        }

        private void menuSize4_Click(object sender, EventArgs e)
        {
            ResizeWindow(4);
            LoadConfiguration();
        }

        private void menuSizeFull_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.FromControl(this).Bounds.Width, Screen.FromControl(this).Bounds.Height);
        }

        private void menuAudioC1_Click(object sender, EventArgs e)
        {
            menuAudioC1.Checked = !menuAudioC1.Checked;
            settings.Channel1Enable = menuAudioC1.Checked;
            UpdateEmulatorSettings();
        }

        private void menuAudioC2_Click(object sender, EventArgs e)
        {
            menuAudioC2.Checked = !menuAudioC2.Checked;
            settings.Channel2Enable = menuAudioC2.Checked;
            UpdateEmulatorSettings();
        }

        private void menuAudioC3_Click(object sender, EventArgs e)
        {
            menuAudioC3.Checked = !menuAudioC3.Checked;
            settings.Channel3Enable = menuAudioC3.Checked;
            UpdateEmulatorSettings();
        }

        private void menuAudioC4_Click(object sender, EventArgs e)
        {
            menuAudioC4.Checked = !menuAudioC4.Checked;
            settings.Channel4Enable = menuAudioC4.Checked;
            UpdateEmulatorSettings();
        }

        private void menuAudioOn_Click(object sender, EventArgs e)
        {
            menuAudioOn.Checked = !menuAudioOn.Checked;
            settings.AudioEnable = menuAudioOn.Checked;
            UpdateEmulatorSettings();
        }

        private void menuFrameSkip0_Click(object sender, EventArgs e)
        {
            settings.Frameskip = 0;
            UpdateEmulatorSettings();
            LoadConfiguration();
        }

        private void menuFrameSkip1_Click(object sender, EventArgs e)
        {
            settings.Frameskip = 1;
            UpdateEmulatorSettings();
            LoadConfiguration();
        }

        private void menuFrameSkip2_Click(object sender, EventArgs e)
        {
            settings.Frameskip = 2;
            UpdateEmulatorSettings();
            LoadConfiguration();
        }

        private void menuFrameSkip3_Click(object sender, EventArgs e)
        {
            settings.Frameskip = 3;
            UpdateEmulatorSettings();
            LoadConfiguration();
        }

        private void menuFrameSkip4_Click(object sender, EventArgs e)
        {
            settings.Frameskip = 4;
            UpdateEmulatorSettings();
            LoadConfiguration();
        }

        private void menuControls_Click(object sender, EventArgs e)
        {
            frmControls controls = new frmControls(settings);
            controls.ShowDialog();
        }

        private void menuAudioQ1_Click(object sender, EventArgs e)
        {
            settings.SampleRate = 0;
            LoadConfiguration();
        }

        private void menuAudioQ2_Click(object sender, EventArgs e)
        {
            settings.SampleRate = 1;
            LoadConfiguration();
        }

        private void menuAudioQ3_Click(object sender, EventArgs e)
        {
            settings.SampleRate = 2;
            LoadConfiguration();
        }

        private void menuAudioQ4_Click(object sender, EventArgs e)
        {
            settings.SampleRate = 3;
            LoadConfiguration();
        }

        private void menuAudioInspector_Click(object sender, EventArgs e)
        {
            audiotoolwindow = new frmAudioTool();
            audiotoolwindow.Nanoboy = nano;
            audiotoolwindow.Show();
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

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            gameView.Refresh();
        }

        private void gameView_Paint(object sender, PaintEventArgs e)
        {
            if (loadedgl && nano != null) {
                if (!glerror) {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();
                    textureid = TextureFromArray(nano.Memory.Video.Screen, textureid);
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
                } else {
                    Bitmap screen_original = new Bitmap(160, 140, 160 * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, nano.Memory.Video.Screen);
                    Image screen_resized = ResizeImage(screen_original, new Size(gameView.Width, gameView.Height), false);
                    e.Graphics.DrawImage(screen_resized, new Point(0, 0));
                }
            }
        }

        public int TextureFromArray(IntPtr arrayptr, int texturexid = -1)
        {
            int id = texturexid == -1 ? GL.GenTexture() : texturexid;
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 160, 144, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, arrayptr);

            /* On some systems InvalidValue gets thrown. Fallback to GDI then */
            if (GL.GetError() != ErrorCode.NoError) {
                glerror = true;
            }
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
                if (e.KeyCode == Keys.Space) {
                    speedup = true;
                }
                nano.SetKey(e.KeyCode);
            }
        }

        private void gameView_KeyUp(object sender, KeyEventArgs e)
        {
            if (nano != null) {
                if (e.KeyCode == Keys.Space) {
                    speedup = true;
                }
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

        private void ResizeWindow(int size)
        {
            int diffwidth;
            int diffheight;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            diffwidth = this.Width - gameView.Width;
            diffheight = this.Height - gameView.Height;
            this.Size = new Size(160 * size + diffwidth, 144 * size + diffheight);
            settings.VideoScaleFactor = size;
            UpdateEmulatorSettings();
        }

        private void LoadConfiguration()
        {
            menuAudioC1.Checked = settings.Channel1Enable;
            menuAudioC2.Checked = settings.Channel2Enable;
            menuAudioC3.Checked = settings.Channel3Enable;
            menuAudioC4.Checked = settings.Channel4Enable;
            menuAudioQ1.Checked = settings.SampleRate == 0;
            menuAudioQ2.Checked = settings.SampleRate == 1;
            menuAudioQ3.Checked = settings.SampleRate == 2;
            menuAudioQ4.Checked = settings.SampleRate == 3;
            menuAudioOn.Checked = settings.AudioEnable;
            menuSize1.Checked = settings.VideoScaleFactor == 1;
            menuSize2.Checked = settings.VideoScaleFactor == 2;
            menuSize3.Checked = settings.VideoScaleFactor == 3;
            menuSize4.Checked = settings.VideoScaleFactor == 4;
            ResizeWindow(settings.VideoScaleFactor);
            menuFrameSkip0.Checked = settings.Frameskip == 0;
            menuFrameSkip1.Checked = settings.Frameskip == 1;
            menuFrameSkip2.Checked = settings.Frameskip == 2;
            menuFrameSkip3.Checked = settings.Frameskip == 3;
            menuFrameSkip4.Checked = settings.Frameskip == 4;
        }

        private void UpdateEmulatorSettings()
        {
            nano?.SetSettings(settings);
        }
    }
}
