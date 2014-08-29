using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeekBoy.Core;

namespace GeekBoy
{
    public partial class frmBreakpoint : Form
    {
        public Breakpoint Breakpoint { get; set; }
        public bool Success { get; set; }

        public frmBreakpoint()
        {
            InitializeComponent();
        }

        private void AddBreakpoint_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int address = int.Parse(textBox1.Text, System.Globalization.NumberStyles.HexNumber);
            int flags = checkBox1.Checked ? (int)BreakpointFlag.Read : 0;
            flags += checkBox2.Checked ? (int)BreakpointFlag.Write : 0;
            flags += checkBox3.Checked ? (int)BreakpointFlag.Execute : 0;
            Breakpoint = new Breakpoint(address, flags);
            Success = true;
            Close();
        }
    }
}
