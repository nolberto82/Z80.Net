namespace Z80.Net
{
    partial class Debugger
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
            this.btnStepInto = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.lvDisassembly = new System.Windows.Forms.ListView();
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvRegisters = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRun = new System.Windows.Forms.Button();
            this.lvBreakpoints = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.conLvDasmMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuGoto = new System.Windows.Forms.ToolStripMenuItem();
            this.hexMemory = new Be.Windows.Forms.HexBox();
            this.lblCycles = new System.Windows.Forms.Label();
            this.lvTests = new System.Windows.Forms.ListView();
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblTestStatus = new System.Windows.Forms.Label();
            this.btnTrace = new System.Windows.Forms.Button();
            this.conLvDasmMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStepInto
            // 
            this.btnStepInto.Location = new System.Drawing.Point(121, 13);
            this.btnStepInto.Margin = new System.Windows.Forms.Padding(4);
            this.btnStepInto.Name = "btnStepInto";
            this.btnStepInto.Size = new System.Drawing.Size(100, 28);
            this.btnStepInto.TabIndex = 1;
            this.btnStepInto.Text = "Step Into";
            this.btnStepInto.UseVisualStyleBackColor = true;
            this.btnStepInto.Click += new System.EventHandler(this.btnStepInto_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(238, 13);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 28);
            this.btnReset.TabIndex = 2;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // lvDisassembly
            // 
            this.lvDisassembly.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.ColumnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvDisassembly.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvDisassembly.FullRowSelect = true;
            this.lvDisassembly.HideSelection = false;
            this.lvDisassembly.Location = new System.Drawing.Point(12, 48);
            this.lvDisassembly.Name = "lvDisassembly";
            this.lvDisassembly.Size = new System.Drawing.Size(443, 369);
            this.lvDisassembly.TabIndex = 4;
            this.lvDisassembly.UseCompatibleStateImageBehavior = false;
            this.lvDisassembly.View = System.Windows.Forms.View.Details;
            this.lvDisassembly.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvDisassembly_MouseClick);
            this.lvDisassembly.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvDisassembly_MouseDoubleClick);
            // 
            // columnHeader9
            // 
            this.columnHeader9.Width = 30;
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "ColumnHeader";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 83;
            // 
            // lvRegisters
            // 
            this.lvRegisters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            this.lvRegisters.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvRegisters.FullRowSelect = true;
            this.lvRegisters.HideSelection = false;
            this.lvRegisters.Location = new System.Drawing.Point(461, 47);
            this.lvRegisters.Name = "lvRegisters";
            this.lvRegisters.Size = new System.Drawing.Size(140, 370);
            this.lvRegisters.TabIndex = 5;
            this.lvRegisters.UseCompatibleStateImageBehavior = false;
            this.lvRegisters.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "ColumnHeader";
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(12, 13);
            this.btnRun.Margin = new System.Windows.Forms.Padding(4);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(100, 28);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lvBreakpoints
            // 
            this.lvBreakpoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
            this.columnHeader7});
            this.lvBreakpoints.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvBreakpoints.FullRowSelect = true;
            this.lvBreakpoints.HideSelection = false;
            this.lvBreakpoints.Location = new System.Drawing.Point(461, 423);
            this.lvBreakpoints.Name = "lvBreakpoints";
            this.lvBreakpoints.Size = new System.Drawing.Size(140, 226);
            this.lvBreakpoints.TabIndex = 7;
            this.lvBreakpoints.UseCompatibleStateImageBehavior = false;
            this.lvBreakpoints.View = System.Windows.Forms.View.Details;
            this.lvBreakpoints.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvBreakpoints_MouseDoubleClick);
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "ColumnHeader";
            // 
            // conLvDasmMenu
            // 
            this.conLvDasmMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuGoto});
            this.conLvDasmMenu.Name = "conLvDasmMenu";
            this.conLvDasmMenu.Size = new System.Drawing.Size(104, 26);
            // 
            // menuGoto
            // 
            this.menuGoto.Name = "menuGoto";
            this.menuGoto.Size = new System.Drawing.Size(103, 22);
            this.menuGoto.Text = "Go to";
            this.menuGoto.Click += new System.EventHandler(this.menuGoto_Click);
            // 
            // hexMemory
            // 
            this.hexMemory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.hexMemory.Location = new System.Drawing.Point(12, 423);
            this.hexMemory.Name = "hexMemory";
            this.hexMemory.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexMemory.Size = new System.Drawing.Size(443, 226);
            this.hexMemory.TabIndex = 8;
            this.hexMemory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.hexMemory_KeyPress);
            // 
            // lblCycles
            // 
            this.lblCycles.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lblCycles.Location = new System.Drawing.Point(462, 13);
            this.lblCycles.Name = "lblCycles";
            this.lblCycles.Size = new System.Drawing.Size(417, 28);
            this.lblCycles.TabIndex = 9;
            this.lblCycles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvTests
            // 
            this.lvTests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8});
            this.lvTests.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvTests.FullRowSelect = true;
            this.lvTests.HideSelection = false;
            this.lvTests.Location = new System.Drawing.Point(607, 134);
            this.lvTests.Name = "lvTests";
            this.lvTests.Size = new System.Drawing.Size(272, 515);
            this.lvTests.TabIndex = 10;
            this.lvTests.UseCompatibleStateImageBehavior = false;
            this.lvTests.View = System.Windows.Forms.View.Details;
            this.lvTests.DoubleClick += new System.EventHandler(this.lvTests_DoubleClick);
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Tests";
            this.columnHeader8.Width = 120;
            // 
            // lblTestStatus
            // 
            this.lblTestStatus.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lblTestStatus.Location = new System.Drawing.Point(610, 47);
            this.lblTestStatus.Name = "lblTestStatus";
            this.lblTestStatus.Size = new System.Drawing.Size(269, 84);
            this.lblTestStatus.TabIndex = 11;
            // 
            // btnTrace
            // 
            this.btnTrace.Location = new System.Drawing.Point(346, 13);
            this.btnTrace.Margin = new System.Windows.Forms.Padding(4);
            this.btnTrace.Name = "btnTrace";
            this.btnTrace.Size = new System.Drawing.Size(100, 28);
            this.btnTrace.TabIndex = 12;
            this.btnTrace.Text = "Trace";
            this.btnTrace.UseVisualStyleBackColor = true;
            this.btnTrace.Click += new System.EventHandler(this.btnTrace_Click);
            // 
            // Debugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 661);
            this.Controls.Add(this.btnTrace);
            this.Controls.Add(this.lblTestStatus);
            this.Controls.Add(this.lvTests);
            this.Controls.Add(this.lblCycles);
            this.Controls.Add(this.hexMemory);
            this.Controls.Add(this.lvBreakpoints);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.lvRegisters);
            this.Controls.Add(this.lvDisassembly);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnStepInto);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Debugger";
            this.Text = "Debugger";
            this.Load += new System.EventHandler(this.Debugger_Load);
            this.conLvDasmMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnStepInto;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ListView lvDisassembly;
        private System.Windows.Forms.ColumnHeader ColumnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView lvRegisters;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.ListView lvBreakpoints;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ContextMenuStrip conLvDasmMenu;
        private System.Windows.Forms.ToolStripMenuItem menuGoto;
        private Be.Windows.Forms.HexBox hexMemory;
        private System.Windows.Forms.Label lblCycles;
        private System.Windows.Forms.ListView lvTests;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label lblTestStatus;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.Button btnTrace;
    }
}