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

namespace nanoboy
{
    partial class frmNano
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNano));
            this.nanoMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuFile = new System.Windows.Forms.MenuItem();
            this.menuOpen = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.menuClose = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuAudioOn = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuAudioC1 = new System.Windows.Forms.MenuItem();
            this.menuAudioC2 = new System.Windows.Forms.MenuItem();
            this.menuAudioC3 = new System.Windows.Forms.MenuItem();
            this.menuAudioC4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuAudioQ1 = new System.Windows.Forms.MenuItem();
            this.menuAudioQ2 = new System.Windows.Forms.MenuItem();
            this.menuAudioQ3 = new System.Windows.Forms.MenuItem();
            this.menuAudioQ4 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.menuFrameSkip0 = new System.Windows.Forms.MenuItem();
            this.menuFrameSkip1 = new System.Windows.Forms.MenuItem();
            this.menuFrameSkip2 = new System.Windows.Forms.MenuItem();
            this.menuFrameSkip3 = new System.Windows.Forms.MenuItem();
            this.menuFrameSkip4 = new System.Windows.Forms.MenuItem();
            this.menuItem19 = new System.Windows.Forms.MenuItem();
            this.menuSize1 = new System.Windows.Forms.MenuItem();
            this.menuSize2 = new System.Windows.Forms.MenuItem();
            this.menuSize3 = new System.Windows.Forms.MenuItem();
            this.menuSize4 = new System.Windows.Forms.MenuItem();
            this.menuSizeFull = new System.Windows.Forms.MenuItem();
            this.menuItem20 = new System.Windows.Forms.MenuItem();
            this.menuControls = new System.Windows.Forms.MenuItem();
            this.menuItem21 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuAbout = new System.Windows.Forms.MenuItem();
            this.openRom = new System.Windows.Forms.OpenFileDialog();
            this.gameView = new OpenTK.GLControl();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.menuAudioInspector = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // nanoMenu
            // 
            this.nanoMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFile,
            this.menuItem1,
            this.menuItem21,
            this.menuItem4});
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuOpen,
            this.menuItem12,
            this.menuClose});
            this.menuFile.Text = "File";
            // 
            // menuOpen
            // 
            this.menuOpen.Index = 0;
            this.menuOpen.Text = "&Open";
            this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 1;
            this.menuItem12.Text = "-";
            // 
            // menuClose
            // 
            this.menuClose.Index = 2;
            this.menuClose.Text = "Close";
            this.menuClose.Click += new System.EventHandler(this.menuClose_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.menuItem3,
            this.menuControls});
            this.menuItem1.Text = "Options";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAudioOn,
            this.menuItem7,
            this.menuAudioC1,
            this.menuAudioC2,
            this.menuAudioC3,
            this.menuAudioC4,
            this.menuItem5});
            this.menuItem2.Text = "Audio";
            // 
            // menuAudioOn
            // 
            this.menuAudioOn.Index = 0;
            this.menuAudioOn.Text = "On";
            this.menuAudioOn.Click += new System.EventHandler(this.menuAudioOn_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            this.menuItem7.Text = "-";
            // 
            // menuAudioC1
            // 
            this.menuAudioC1.Index = 2;
            this.menuAudioC1.Text = "Channel 1";
            this.menuAudioC1.Click += new System.EventHandler(this.menuAudioC1_Click);
            // 
            // menuAudioC2
            // 
            this.menuAudioC2.Index = 3;
            this.menuAudioC2.Text = "Channel 2";
            this.menuAudioC2.Click += new System.EventHandler(this.menuAudioC2_Click);
            // 
            // menuAudioC3
            // 
            this.menuAudioC3.Index = 4;
            this.menuAudioC3.Text = "Channel 3";
            this.menuAudioC3.Click += new System.EventHandler(this.menuAudioC3_Click);
            // 
            // menuAudioC4
            // 
            this.menuAudioC4.Index = 5;
            this.menuAudioC4.Text = "Channel 4";
            this.menuAudioC4.Click += new System.EventHandler(this.menuAudioC4_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 6;
            this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAudioQ1,
            this.menuAudioQ2,
            this.menuAudioQ3,
            this.menuAudioQ4});
            this.menuItem5.Text = "Quality";
            // 
            // menuAudioQ1
            // 
            this.menuAudioQ1.Index = 0;
            this.menuAudioQ1.Text = "8192 Hz";
            this.menuAudioQ1.Click += new System.EventHandler(this.menuAudioQ1_Click);
            // 
            // menuAudioQ2
            // 
            this.menuAudioQ2.Index = 1;
            this.menuAudioQ2.Text = "16384 Hz";
            this.menuAudioQ2.Click += new System.EventHandler(this.menuAudioQ2_Click);
            // 
            // menuAudioQ3
            // 
            this.menuAudioQ3.Index = 2;
            this.menuAudioQ3.Text = "32768 Hz";
            this.menuAudioQ3.Click += new System.EventHandler(this.menuAudioQ3_Click);
            // 
            // menuAudioQ4
            // 
            this.menuAudioQ4.Index = 3;
            this.menuAudioQ4.Text = "44100 Hz";
            this.menuAudioQ4.Click += new System.EventHandler(this.menuAudioQ4_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem13,
            this.menuItem19});
            this.menuItem3.Text = "Video";
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 0;
            this.menuItem13.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFrameSkip0,
            this.menuFrameSkip1,
            this.menuFrameSkip2,
            this.menuFrameSkip3,
            this.menuFrameSkip4});
            this.menuItem13.Text = "Frameskip";
            // 
            // menuFrameSkip0
            // 
            this.menuFrameSkip0.Index = 0;
            this.menuFrameSkip0.RadioCheck = true;
            this.menuFrameSkip0.Text = "None";
            this.menuFrameSkip0.Click += new System.EventHandler(this.menuFrameSkip0_Click);
            // 
            // menuFrameSkip1
            // 
            this.menuFrameSkip1.Index = 1;
            this.menuFrameSkip1.RadioCheck = true;
            this.menuFrameSkip1.Text = "1";
            this.menuFrameSkip1.Click += new System.EventHandler(this.menuFrameSkip1_Click);
            // 
            // menuFrameSkip2
            // 
            this.menuFrameSkip2.Index = 2;
            this.menuFrameSkip2.RadioCheck = true;
            this.menuFrameSkip2.Text = "2";
            this.menuFrameSkip2.Click += new System.EventHandler(this.menuFrameSkip2_Click);
            // 
            // menuFrameSkip3
            // 
            this.menuFrameSkip3.Index = 3;
            this.menuFrameSkip3.RadioCheck = true;
            this.menuFrameSkip3.Text = "3";
            this.menuFrameSkip3.Click += new System.EventHandler(this.menuFrameSkip3_Click);
            // 
            // menuFrameSkip4
            // 
            this.menuFrameSkip4.Index = 4;
            this.menuFrameSkip4.RadioCheck = true;
            this.menuFrameSkip4.Text = "4";
            this.menuFrameSkip4.Click += new System.EventHandler(this.menuFrameSkip4_Click);
            // 
            // menuItem19
            // 
            this.menuItem19.Index = 1;
            this.menuItem19.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuSize1,
            this.menuSize2,
            this.menuSize3,
            this.menuSize4,
            this.menuSizeFull,
            this.menuItem20});
            this.menuItem19.Text = "Size";
            // 
            // menuSize1
            // 
            this.menuSize1.Index = 0;
            this.menuSize1.RadioCheck = true;
            this.menuSize1.Text = "1x";
            this.menuSize1.Click += new System.EventHandler(this.menuSize1_Click);
            // 
            // menuSize2
            // 
            this.menuSize2.Index = 1;
            this.menuSize2.RadioCheck = true;
            this.menuSize2.Text = "2x";
            this.menuSize2.Click += new System.EventHandler(this.menuSize2_Click);
            // 
            // menuSize3
            // 
            this.menuSize3.Index = 2;
            this.menuSize3.RadioCheck = true;
            this.menuSize3.Text = "3x";
            this.menuSize3.Click += new System.EventHandler(this.menuSize3_Click);
            // 
            // menuSize4
            // 
            this.menuSize4.Index = 3;
            this.menuSize4.RadioCheck = true;
            this.menuSize4.Text = "4x";
            this.menuSize4.Click += new System.EventHandler(this.menuSize4_Click);
            // 
            // menuSizeFull
            // 
            this.menuSizeFull.Index = 4;
            this.menuSizeFull.RadioCheck = true;
            this.menuSizeFull.Text = "Fullscreen";
            this.menuSizeFull.Click += new System.EventHandler(this.menuSizeFull_Click);
            // 
            // menuItem20
            // 
            this.menuItem20.Index = 5;
            this.menuItem20.Text = "-";
            // 
            // menuControls
            // 
            this.menuControls.Index = 2;
            this.menuControls.Text = "Controls";
            this.menuControls.Click += new System.EventHandler(this.menuControls_Click);
            // 
            // menuItem21
            // 
            this.menuItem21.Index = 2;
            this.menuItem21.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAudioInspector});
            this.menuItem21.Text = "Tools";
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 3;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAbout});
            this.menuItem4.Text = "?";
            // 
            // menuAbout
            // 
            this.menuAbout.Index = 0;
            this.menuAbout.Text = "About";
            this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // openRom
            // 
            this.openRom.Filter = "Gameboy ROMs (*.gb, *.gbc)|*.gb;*.gbc";
            this.openRom.InitialDirectory = "./ROMS";
            // 
            // gameView
            // 
            this.gameView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameView.BackColor = System.Drawing.Color.Black;
            this.gameView.Location = new System.Drawing.Point(0, 0);
            this.gameView.Name = "gameView";
            this.gameView.Size = new System.Drawing.Size(320, 288);
            this.gameView.TabIndex = 0;
            this.gameView.VSync = false;
            this.gameView.Load += new System.EventHandler(this.gameView_Load);
            this.gameView.Paint += new System.Windows.Forms.PaintEventHandler(this.gameView_Paint);
            this.gameView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gameView_KeyUp);
            this.gameView.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.gameView_PreviewKeyDown);
            this.gameView.Resize += new System.EventHandler(this.gameView_Resize);
            // 
            // updateTimer
            // 
            this.updateTimer.Interval = 16;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // menuAudioInspector
            // 
            this.menuAudioInspector.Index = 0;
            this.menuAudioInspector.Text = "Audio Inspector";
            this.menuAudioInspector.Click += new System.EventHandler(this.menuAudioInspector_Click);
            // 
            // frmNano
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 288);
            this.Controls.Add(this.gameView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.nanoMenu;
            this.Name = "frmNano";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nanoboy v1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmNano_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu nanoMenu;
        private System.Windows.Forms.MenuItem menuFile;
        private System.Windows.Forms.MenuItem menuOpen;
        private System.Windows.Forms.MenuItem menuClose;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.OpenFileDialog openRom;
        private System.Windows.Forms.MenuItem menuAbout;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuControls;
        private System.Windows.Forms.MenuItem menuAudioOn;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuAudioC1;
        private System.Windows.Forms.MenuItem menuAudioC2;
        private System.Windows.Forms.MenuItem menuAudioC3;
        private System.Windows.Forms.MenuItem menuAudioC4;
        private System.Windows.Forms.MenuItem menuItem12;
        private System.Windows.Forms.MenuItem menuItem13;
        private System.Windows.Forms.MenuItem menuFrameSkip0;
        private System.Windows.Forms.MenuItem menuFrameSkip1;
        private System.Windows.Forms.MenuItem menuFrameSkip2;
        private System.Windows.Forms.MenuItem menuFrameSkip3;
        private System.Windows.Forms.MenuItem menuFrameSkip4;
        private System.Windows.Forms.MenuItem menuItem19;
        private System.Windows.Forms.MenuItem menuSize1;
        private System.Windows.Forms.MenuItem menuSize2;
        private System.Windows.Forms.MenuItem menuSize3;
        private System.Windows.Forms.MenuItem menuSize4;
        private System.Windows.Forms.MenuItem menuSizeFull;
        private System.Windows.Forms.MenuItem menuItem20;
        private OpenTK.GLControl gameView;
        private System.Windows.Forms.MenuItem menuItem21;
        private System.Windows.Forms.MenuItem menuAudioQ1;
        private System.Windows.Forms.MenuItem menuAudioQ2;
        private System.Windows.Forms.MenuItem menuAudioQ3;
        private System.Windows.Forms.MenuItem menuAudioQ4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.MenuItem menuAudioInspector;
    }
}