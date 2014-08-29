using System.Drawing;
using System.Windows.Forms;

namespace GeekBoy
{
    class ClosablePanel : Panel
    {
        public string Title { get; set; }
        private static Bitmap _close_button = Properties.Resources.close;
        private static Pen _border = new Pen(Color.FromArgb(185, 185, 185));
        private static Brush _fill = new SolidBrush(Color.FromArgb(219, 219, 219));

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.DrawRectangle(_border, new Rectangle(0, 0, this.Width - 1, 20));
            g.FillRectangle(_fill, new Rectangle(1, 1, this.Width - 2, 19));
            g.DrawString(this.Title, this.Font, Brushes.Black, new Point(2, 4));
            g.DrawImage(_close_button, new Point(this.Width - 16, 7));
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.X >= this.Width - 16 && e.X <= this.Width - 8 && e.Y >= 7 && e.Y <= 16)
            {
                this.Dispose();
            }
        }

    }
}
