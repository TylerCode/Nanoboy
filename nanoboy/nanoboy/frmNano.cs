using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using nanoboy.Core;

namespace nanoboy
{
    public partial class frmNano : Form
    {

        private Gameboy _gb;

        public frmNano()
        {
            InitializeComponent();
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            if (openRom.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Rom rom = new Rom(openRom.FileName);
                this._gb = new Gameboy(rom, false);
                gameRunner.Start();
            }
        }

        private void gameRunner_Tick(object sender, EventArgs e)
        {
            gameView.Image = this.ResizeImage(this._gb.Video.Buffer, gameView.Size, false);
            this._gb.Step();
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

        private void frmNano_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (this._gb != null)
                this._gb.Joypad.HandleInput(e.KeyCode, false);
        }

        private void frmNano_KeyUp(object sender, KeyEventArgs e)
        {
            if (this._gb != null)
                this._gb.Joypad.HandleInput(e.KeyCode, true);
        }

    }
}
