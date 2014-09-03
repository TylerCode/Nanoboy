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
using GeekBoy.Observer;

namespace GeekBoy
{

    public partial class frmDebugger : Form, IObserver
    {
        private Gameboy _gameboy;
        private List<Breakpoint> _breakpoints = new List<Breakpoint>();
        private bool _step1 = false, _step2 = false;

        public frmDebugger(Gameboy target)
        {
            InitializeComponent();
            _gameboy = target;
            Log("Debugger Form launched.");
            _gameboy.Cpu.Subscribe(this);
            _gameboy.Memory.Subscribe(this);
            Log("Targets subscribed.");
        }

        private void Debugger_Load(object sender, EventArgs e)
        {
            for (int i = 0; i <= 0xFFFF; i++)
            {
                disassembly.Items.Add(i.ToString("X4"));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listRegisters.Items[0].SubItems[1].Text = ((_gameboy.Cpu.A << 8) | _gameboy.Cpu.F).ToString("X4");
            listRegisters.Items[0].SubItems[2].Text = Offset2Description((_gameboy.Cpu.A << 8) | _gameboy.Cpu.F);
            listRegisters.Items[1].SubItems[1].Text = ((_gameboy.Cpu.B << 8) | _gameboy.Cpu.C).ToString("X4");
            listRegisters.Items[1].SubItems[2].Text = Offset2Description((_gameboy.Cpu.B << 8) | _gameboy.Cpu.C);
            listRegisters.Items[2].SubItems[1].Text = ((_gameboy.Cpu.D << 8) | _gameboy.Cpu.E).ToString("X4");
            listRegisters.Items[2].SubItems[2].Text = Offset2Description((_gameboy.Cpu.D << 8) | _gameboy.Cpu.E);
            listRegisters.Items[3].SubItems[1].Text = ((_gameboy.Cpu.H << 8) | _gameboy.Cpu.L).ToString("X4");
            listRegisters.Items[3].SubItems[2].Text = Offset2Description((_gameboy.Cpu.H << 8) | _gameboy.Cpu.L);
            listRegisters.Items[4].SubItems[1].Text = _gameboy.Cpu.Pc.ToString("X4");
            listRegisters.Items[5].SubItems[1].Text = _gameboy.Cpu.Sp.ToString("X4");

            IME.Checked = _gameboy.Cpu.Ime;
            WaitForInterrupt.Checked = _gameboy.Cpu.WaitForInterrupt;
            C.Checked = _gameboy.Cpu.FlagC;
            H.Checked = _gameboy.Cpu.FlagHc;
            N.Checked = _gameboy.Cpu.FlagN;
            Z.Checked = _gameboy.Cpu.FlagZ;

            disassembly.SelectedIndex = _gameboy.Cpu.Pc;
            disassembly.Items[disassembly.SelectedIndex] = string.Format("{0:X4}: {1}", disassembly.SelectedIndex, _gameboy.Disassembler.Disassemble(disassembly.SelectedIndex, 1)[0]);
        }

        private string Offset2Description(int value)
        {
            if (value <= 0x7FFF)
            {
                return "ROM -> " + _gameboy.Memory.ReadByte(value);
            }
            if (value <= 0x9FFF)
            {
                return "VRAM";
            }
            if (value <= 0xBFFF)
            {
                return "SRAM";
            }
            if (value <= 0xFDFF)
            {
                return "WRAM";
            }
            if (value >= 0xFF00 && value <= 0xFF7F)
            {
                return "IO -> " + _gameboy.Memory.ReadByte(value);
            }
            return string.Empty;
        }

        public void Notify(NotifyData notifyData)
        {
            switch (notifyData.Type)
            {
                case "MEMORY_IO_READ":
                    //Log(string.Format("IO READ ON: {0:X4}", (int)notifyData.Data));
                    break;
                case "MEMORY_IO_WRITE":
                    //Log(string.Format("IO WRITE ON: {0:X4}", (int)notifyData.Data));
                    break;
                case "CPU_EXECUTE":
                    if (_step1) 
                    {
                        _step1 = false;
                        _step2 = true;
                        return;
                    } else if (_step2) {
                        _gameboy.Cpu.Pause = true;
                        _step2 = false;
                    }
                    foreach (Breakpoint breakpoint in _breakpoints)
                    {
                        if (breakpoint.Address == (int)notifyData.Data && (breakpoint.TypeFlags & (int)BreakpointFlag.Execute) != 0)
                        {
                            _gameboy.Cpu.Pause = true;
                            Log(string.Format("DEBUGGER: Hit breakpoint ON EXECUTE @ {0:X4}", breakpoint.Address));
                        }
                    }
                    break;
                case "CPU_READ":
                    foreach (Breakpoint breakpoint in _breakpoints)
                    {
                        if (breakpoint.Address == (int)notifyData.Data && (breakpoint.TypeFlags & (int)BreakpointFlag.Read) != 0)
                        {
                            _gameboy.Cpu.Pause = true;
                            Log(string.Format("DEBUGGER: Hit breakpoint ON READ @ {0:X4}", breakpoint.Address));
                        }
                    }
                    break;
                case "CPU_WRITE":
                    foreach (Breakpoint breakpoint in _breakpoints)
                    {
                        if (breakpoint.Address == (int)notifyData.Data && (breakpoint.TypeFlags & (int)BreakpointFlag.Write) != 0)
                        {
                            _gameboy.Cpu.Pause = true;
                            Log(string.Format("DEBUGGER: Hit breakpoint ON WRITE @ {0:X4}", breakpoint.Address));
                        }
                    }
                    break;
            }
        }

        private void Log(string log)
        {
            this.textlog.Text += log + Environment.NewLine;
            this.textlog.SelectionStart = this.textlog.Text.Length;
            this.textlog.ScrollToCaret();
        }

        private void autoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = autoUpdate.Checked;
        }

        private void dbgPause_Click(object sender, EventArgs e)
        {
            _gameboy.Cpu.Pause = true;
        }

        private void dbgResume_Click(object sender, EventArgs e)
        {
            _gameboy.Cpu.Pause = false;
        }

        private void dbgStep_Click(object sender, EventArgs e)
        {
            _gameboy.Cpu.Pause = false;
            _step1 = true;
        }

        private void dbgBreakpoint_Click(object sender, EventArgs e)
        {
            frmBreakpoint addbp = new frmBreakpoint();
            addbp.ShowDialog();
            if (addbp.Success)
            {
                _breakpoints.Add(addbp.Breakpoint);
            }
            addbp.Dispose();
        }

    }
}
