namespace GeekBoy
{
    partial class frmDebugger
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "AF",
            "",
            ""}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "BC",
            "",
            ""}, -1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "DE",
            "",
            ""}, -1);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
            "HL",
            "",
            ""}, -1);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new string[] {
            "PC",
            "",
            "Program Counter"}, -1);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem(new string[] {
            "SP",
            "",
            "Stack Pointer"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDebugger));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.autoUpdate = new System.Windows.Forms.CheckBox();
            this.closablePanel3 = new GeekBoy.ClosablePanel();
            this.dbgBreakpoint = new System.Windows.Forms.Button();
            this.dbgStep = new System.Windows.Forms.Button();
            this.dbgResume = new System.Windows.Forms.Button();
            this.dbgPause = new System.Windows.Forms.Button();
            this.disassembly = new System.Windows.Forms.ListBox();
            this.closablePanel2 = new GeekBoy.ClosablePanel();
            this.textlog = new System.Windows.Forms.TextBox();
            this.closablePanel1 = new GeekBoy.ClosablePanel();
            this.Z = new System.Windows.Forms.CheckBox();
            this.N = new System.Windows.Forms.CheckBox();
            this.H = new System.Windows.Forms.CheckBox();
            this.C = new System.Windows.Forms.CheckBox();
            this.WaitForInterrupt = new System.Windows.Forms.CheckBox();
            this.IME = new System.Windows.Forms.CheckBox();
            this.listRegisters = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.closablePanel3.SuspendLayout();
            this.closablePanel2.SuspendLayout();
            this.closablePanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // autoUpdate
            // 
            this.autoUpdate.AutoSize = true;
            this.autoUpdate.Checked = true;
            this.autoUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoUpdate.Location = new System.Drawing.Point(12, 267);
            this.autoUpdate.Name = "autoUpdate";
            this.autoUpdate.Size = new System.Drawing.Size(86, 17);
            this.autoUpdate.TabIndex = 3;
            this.autoUpdate.Text = "Auto Update";
            this.autoUpdate.UseVisualStyleBackColor = true;
            this.autoUpdate.CheckedChanged += new System.EventHandler(this.autoUpdate_CheckedChanged);
            // 
            // closablePanel3
            // 
            this.closablePanel3.Controls.Add(this.dbgBreakpoint);
            this.closablePanel3.Controls.Add(this.dbgStep);
            this.closablePanel3.Controls.Add(this.dbgResume);
            this.closablePanel3.Controls.Add(this.dbgPause);
            this.closablePanel3.Controls.Add(this.disassembly);
            this.closablePanel3.Location = new System.Drawing.Point(260, 12);
            this.closablePanel3.Name = "closablePanel3";
            this.closablePanel3.Size = new System.Drawing.Size(403, 272);
            this.closablePanel3.TabIndex = 4;
            this.closablePanel3.Title = "Disassembly";
            // 
            // dbgBreakpoint
            // 
            this.dbgBreakpoint.Location = new System.Drawing.Point(298, 236);
            this.dbgBreakpoint.Name = "dbgBreakpoint";
            this.dbgBreakpoint.Size = new System.Drawing.Size(89, 23);
            this.dbgBreakpoint.TabIndex = 6;
            this.dbgBreakpoint.Text = "Add Breakpoint";
            this.dbgBreakpoint.UseVisualStyleBackColor = true;
            this.dbgBreakpoint.Click += new System.EventHandler(this.dbgBreakpoint_Click);
            // 
            // dbgStep
            // 
            this.dbgStep.Location = new System.Drawing.Point(203, 236);
            this.dbgStep.Name = "dbgStep";
            this.dbgStep.Size = new System.Drawing.Size(89, 23);
            this.dbgStep.TabIndex = 5;
            this.dbgStep.Text = "Step";
            this.dbgStep.UseVisualStyleBackColor = true;
            this.dbgStep.Click += new System.EventHandler(this.dbgStep_Click);
            // 
            // dbgResume
            // 
            this.dbgResume.Location = new System.Drawing.Point(108, 236);
            this.dbgResume.Name = "dbgResume";
            this.dbgResume.Size = new System.Drawing.Size(89, 23);
            this.dbgResume.TabIndex = 4;
            this.dbgResume.Text = "Resume";
            this.dbgResume.UseVisualStyleBackColor = true;
            this.dbgResume.Click += new System.EventHandler(this.dbgResume_Click);
            // 
            // dbgPause
            // 
            this.dbgPause.Location = new System.Drawing.Point(13, 236);
            this.dbgPause.Name = "dbgPause";
            this.dbgPause.Size = new System.Drawing.Size(89, 23);
            this.dbgPause.TabIndex = 3;
            this.dbgPause.Text = "Pause";
            this.dbgPause.UseVisualStyleBackColor = true;
            this.dbgPause.Click += new System.EventHandler(this.dbgPause_Click);
            // 
            // disassembly
            // 
            this.disassembly.FormattingEnabled = true;
            this.disassembly.Location = new System.Drawing.Point(9, 28);
            this.disassembly.Name = "disassembly";
            this.disassembly.ScrollAlwaysVisible = true;
            this.disassembly.Size = new System.Drawing.Size(384, 199);
            this.disassembly.TabIndex = 2;
            // 
            // closablePanel2
            // 
            this.closablePanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.closablePanel2.Controls.Add(this.textlog);
            this.closablePanel2.Location = new System.Drawing.Point(0, 290);
            this.closablePanel2.Name = "closablePanel2";
            this.closablePanel2.Size = new System.Drawing.Size(675, 183);
            this.closablePanel2.TabIndex = 1;
            this.closablePanel2.Title = "Log";
            // 
            // textlog
            // 
            this.textlog.BackColor = System.Drawing.Color.Black;
            this.textlog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.textlog.Location = new System.Drawing.Point(11, 31);
            this.textlog.Multiline = true;
            this.textlog.Name = "textlog";
            this.textlog.ReadOnly = true;
            this.textlog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textlog.Size = new System.Drawing.Size(652, 139);
            this.textlog.TabIndex = 0;
            // 
            // closablePanel1
            // 
            this.closablePanel1.Controls.Add(this.Z);
            this.closablePanel1.Controls.Add(this.N);
            this.closablePanel1.Controls.Add(this.H);
            this.closablePanel1.Controls.Add(this.C);
            this.closablePanel1.Controls.Add(this.WaitForInterrupt);
            this.closablePanel1.Controls.Add(this.IME);
            this.closablePanel1.Controls.Add(this.listRegisters);
            this.closablePanel1.Location = new System.Drawing.Point(12, 12);
            this.closablePanel1.Name = "closablePanel1";
            this.closablePanel1.Size = new System.Drawing.Size(242, 203);
            this.closablePanel1.TabIndex = 0;
            this.closablePanel1.Title = "CPU Status";
            // 
            // Z
            // 
            this.Z.AutoSize = true;
            this.Z.Enabled = false;
            this.Z.Location = new System.Drawing.Point(127, 183);
            this.Z.Name = "Z";
            this.Z.Size = new System.Drawing.Size(33, 17);
            this.Z.TabIndex = 4;
            this.Z.Text = "Z";
            this.Z.UseVisualStyleBackColor = true;
            // 
            // N
            // 
            this.N.AutoSize = true;
            this.N.Enabled = false;
            this.N.Location = new System.Drawing.Point(87, 183);
            this.N.Name = "N";
            this.N.Size = new System.Drawing.Size(34, 17);
            this.N.TabIndex = 3;
            this.N.Text = "N";
            this.N.UseVisualStyleBackColor = true;
            // 
            // H
            // 
            this.H.AutoSize = true;
            this.H.Enabled = false;
            this.H.Location = new System.Drawing.Point(47, 183);
            this.H.Name = "H";
            this.H.Size = new System.Drawing.Size(34, 17);
            this.H.TabIndex = 2;
            this.H.Text = "H";
            this.H.UseVisualStyleBackColor = true;
            // 
            // C
            // 
            this.C.AutoSize = true;
            this.C.Enabled = false;
            this.C.Location = new System.Drawing.Point(8, 183);
            this.C.Name = "C";
            this.C.Size = new System.Drawing.Size(33, 17);
            this.C.TabIndex = 1;
            this.C.Text = "C";
            this.C.UseVisualStyleBackColor = true;
            // 
            // WaitForInterrupt
            // 
            this.WaitForInterrupt.AutoSize = true;
            this.WaitForInterrupt.Enabled = false;
            this.WaitForInterrupt.Location = new System.Drawing.Point(59, 163);
            this.WaitForInterrupt.Name = "WaitForInterrupt";
            this.WaitForInterrupt.Size = new System.Drawing.Size(102, 17);
            this.WaitForInterrupt.TabIndex = 1;
            this.WaitForInterrupt.Text = "WaitForInterrupt";
            this.WaitForInterrupt.UseVisualStyleBackColor = true;
            // 
            // IME
            // 
            this.IME.AutoSize = true;
            this.IME.Enabled = false;
            this.IME.Location = new System.Drawing.Point(8, 163);
            this.IME.Name = "IME";
            this.IME.Size = new System.Drawing.Size(45, 17);
            this.IME.TabIndex = 1;
            this.IME.Text = "IME";
            this.IME.UseVisualStyleBackColor = true;
            // 
            // listRegisters
            // 
            this.listRegisters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listRegisters.GridLines = true;
            this.listRegisters.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6});
            this.listRegisters.Location = new System.Drawing.Point(8, 28);
            this.listRegisters.Name = "listRegisters";
            this.listRegisters.Size = new System.Drawing.Size(226, 130);
            this.listRegisters.TabIndex = 1;
            this.listRegisters.UseCompatibleStateImageBehavior = false;
            this.listRegisters.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Register";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Comment";
            this.columnHeader3.Width = 102;
            // 
            // Debugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 471);
            this.Controls.Add(this.closablePanel3);
            this.Controls.Add(this.autoUpdate);
            this.Controls.Add(this.closablePanel2);
            this.Controls.Add(this.closablePanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Debugger";
            this.Text = "GeekBoy 2.5 - Debugger";
            this.Load += new System.EventHandler(this.Debugger_Load);
            this.closablePanel3.ResumeLayout(false);
            this.closablePanel2.ResumeLayout(false);
            this.closablePanel2.PerformLayout();
            this.closablePanel1.ResumeLayout(false);
            this.closablePanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ClosablePanel closablePanel1;
        private System.Windows.Forms.ListView listRegisters;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox WaitForInterrupt;
        private System.Windows.Forms.CheckBox IME;
        private System.Windows.Forms.CheckBox Z;
        private System.Windows.Forms.CheckBox N;
        private System.Windows.Forms.CheckBox H;
        private System.Windows.Forms.CheckBox C;
        private ClosablePanel closablePanel2;
        private System.Windows.Forms.ListBox disassembly;
        private System.Windows.Forms.CheckBox autoUpdate;
        private System.Windows.Forms.TextBox textlog;
        private ClosablePanel closablePanel3;
        private System.Windows.Forms.Button dbgBreakpoint;
        private System.Windows.Forms.Button dbgStep;
        private System.Windows.Forms.Button dbgResume;
        private System.Windows.Forms.Button dbgPause;
    }
}