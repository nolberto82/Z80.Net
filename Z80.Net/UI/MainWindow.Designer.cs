namespace Z80.Net
{
    partial class MainWindow
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dIPSwitchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.freeplayMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.coin1PerGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coin1Per2GamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coins2PerGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.live1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lifes2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lives3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lives5ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.extraLife10000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extraLife15000PointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extraLife20000PointsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.extraLifeNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.difficultyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ghostNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.emulatorToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.dIPSwitchesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(555, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openRomToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openRomToolStripMenuItem
            // 
            this.openRomToolStripMenuItem.Name = "openRomToolStripMenuItem";
            this.openRomToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.openRomToolStripMenuItem.Text = "Open Rom";
            this.openRomToolStripMenuItem.Click += new System.EventHandler(this.openRomToolStripMenuItem_Click);
            // 
            // emulatorToolStripMenuItem
            // 
            this.emulatorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToolStripMenuItem});
            this.emulatorToolStripMenuItem.Name = "emulatorToolStripMenuItem";
            this.emulatorToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.emulatorToolStripMenuItem.Text = "Emulator";
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debuggerToolStripMenuItem,
            this.tilesToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.toolsToolStripMenuItem.Text = "Debug";
            // 
            // debuggerToolStripMenuItem
            // 
            this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
            this.debuggerToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.debuggerToolStripMenuItem.Text = "Debugger";
            this.debuggerToolStripMenuItem.Click += new System.EventHandler(this.debuggerToolStripMenuItem_Click);
            // 
            // tilesToolStripMenuItem
            // 
            this.tilesToolStripMenuItem.Name = "tilesToolStripMenuItem";
            this.tilesToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.tilesToolStripMenuItem.Text = "Tiles";
            this.tilesToolStripMenuItem.Click += new System.EventHandler(this.tilesToolStripMenuItem_Click);
            // 
            // dIPSwitchesToolStripMenuItem
            // 
            this.dIPSwitchesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.freeplayMenuItem1,
            this.coin1PerGameToolStripMenuItem,
            this.coin1Per2GamesToolStripMenuItem,
            this.coins2PerGameToolStripMenuItem,
            this.toolStripMenuItem1,
            this.live1ToolStripMenuItem,
            this.lifes2ToolStripMenuItem,
            this.lives3ToolStripMenuItem,
            this.lives5ToolStripMenuItem1,
            this.toolStripMenuItem2,
            this.extraLife10000ToolStripMenuItem,
            this.extraLife15000PointsToolStripMenuItem,
            this.extraLife20000PointsToolStripMenuItem1,
            this.extraLifeNoneToolStripMenuItem,
            this.toolStripMenuItem3,
            this.difficultyToolStripMenuItem,
            this.ghostNamesToolStripMenuItem});
            this.dIPSwitchesToolStripMenuItem.Name = "dIPSwitchesToolStripMenuItem";
            this.dIPSwitchesToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.dIPSwitchesToolStripMenuItem.Text = "DIP Switches";
            // 
            // freeplayMenuItem1
            // 
            this.freeplayMenuItem1.CheckOnClick = true;
            this.freeplayMenuItem1.Name = "freeplayMenuItem1";
            this.freeplayMenuItem1.Size = new System.Drawing.Size(196, 22);
            this.freeplayMenuItem1.Text = "Free Play";
            this.freeplayMenuItem1.Click += new System.EventHandler(this.freeplayMenuItem1_Click);
            // 
            // coin1PerGameToolStripMenuItem
            // 
            this.coin1PerGameToolStripMenuItem.Name = "coin1PerGameToolStripMenuItem";
            this.coin1PerGameToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.coin1PerGameToolStripMenuItem.Text = "1 Coin Per Game";
            this.coin1PerGameToolStripMenuItem.Click += new System.EventHandler(this.coin1PerGameToolStripMenuItem_Click);
            // 
            // coin1Per2GamesToolStripMenuItem
            // 
            this.coin1Per2GamesToolStripMenuItem.Name = "coin1Per2GamesToolStripMenuItem";
            this.coin1Per2GamesToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.coin1Per2GamesToolStripMenuItem.Text = "1 Coin Per 2 Games";
            this.coin1Per2GamesToolStripMenuItem.Click += new System.EventHandler(this.coin1Per2GamesToolStripMenuItem_Click);
            // 
            // coins2PerGameToolStripMenuItem
            // 
            this.coins2PerGameToolStripMenuItem.Name = "coins2PerGameToolStripMenuItem";
            this.coins2PerGameToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.coins2PerGameToolStripMenuItem.Text = "2 Coins Per Game";
            this.coins2PerGameToolStripMenuItem.Click += new System.EventHandler(this.coins2PerGameToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 6);
            // 
            // live1ToolStripMenuItem
            // 
            this.live1ToolStripMenuItem.Name = "live1ToolStripMenuItem";
            this.live1ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.live1ToolStripMenuItem.Text = "1 Life";
            this.live1ToolStripMenuItem.Click += new System.EventHandler(this.live1ToolStripMenuItem_Click);
            // 
            // lifes2ToolStripMenuItem
            // 
            this.lifes2ToolStripMenuItem.Name = "lifes2ToolStripMenuItem";
            this.lifes2ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.lifes2ToolStripMenuItem.Text = "2 Lives";
            this.lifes2ToolStripMenuItem.Click += new System.EventHandler(this.lifes2ToolStripMenuItem_Click);
            // 
            // lives3ToolStripMenuItem
            // 
            this.lives3ToolStripMenuItem.Name = "lives3ToolStripMenuItem";
            this.lives3ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.lives3ToolStripMenuItem.Text = "3 Lives";
            this.lives3ToolStripMenuItem.Click += new System.EventHandler(this.lives3ToolStripMenuItem_Click);
            // 
            // lives5ToolStripMenuItem1
            // 
            this.lives5ToolStripMenuItem1.Name = "lives5ToolStripMenuItem1";
            this.lives5ToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            this.lives5ToolStripMenuItem1.Text = "5 Lives";
            this.lives5ToolStripMenuItem1.Click += new System.EventHandler(this.lives5ToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(193, 6);
            // 
            // extraLife10000ToolStripMenuItem
            // 
            this.extraLife10000ToolStripMenuItem.Name = "extraLife10000ToolStripMenuItem";
            this.extraLife10000ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.extraLife10000ToolStripMenuItem.Text = "Extra life - 10000 Points";
            this.extraLife10000ToolStripMenuItem.Click += new System.EventHandler(this.extraLife10000ToolStripMenuItem_Click);
            // 
            // extraLife15000PointsToolStripMenuItem
            // 
            this.extraLife15000PointsToolStripMenuItem.Name = "extraLife15000PointsToolStripMenuItem";
            this.extraLife15000PointsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.extraLife15000PointsToolStripMenuItem.Text = "Extra life - 15000 Points";
            this.extraLife15000PointsToolStripMenuItem.Click += new System.EventHandler(this.extraLife15000PointsToolStripMenuItem_Click);
            // 
            // extraLife20000PointsToolStripMenuItem1
            // 
            this.extraLife20000PointsToolStripMenuItem1.Name = "extraLife20000PointsToolStripMenuItem1";
            this.extraLife20000PointsToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            this.extraLife20000PointsToolStripMenuItem1.Text = "Extra life - 20000 Points";
            this.extraLife20000PointsToolStripMenuItem1.Click += new System.EventHandler(this.extraLife20000PointsToolStripMenuItem1_Click);
            // 
            // extraLifeNoneToolStripMenuItem
            // 
            this.extraLifeNoneToolStripMenuItem.Name = "extraLifeNoneToolStripMenuItem";
            this.extraLifeNoneToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.extraLifeNoneToolStripMenuItem.Text = "Extra life - None";
            this.extraLifeNoneToolStripMenuItem.Click += new System.EventHandler(this.extraLifeNoneToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(193, 6);
            // 
            // difficultyToolStripMenuItem
            // 
            this.difficultyToolStripMenuItem.CheckOnClick = true;
            this.difficultyToolStripMenuItem.Name = "difficultyToolStripMenuItem";
            this.difficultyToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.difficultyToolStripMenuItem.Text = "Difficulty";
            this.difficultyToolStripMenuItem.Click += new System.EventHandler(this.difficultyToolStripMenuItem_Click);
            // 
            // ghostNamesToolStripMenuItem
            // 
            this.ghostNamesToolStripMenuItem.CheckOnClick = true;
            this.ghostNamesToolStripMenuItem.Name = "ghostNamesToolStripMenuItem";
            this.ghostNamesToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.ghostNamesToolStripMenuItem.Text = "Ghost Names";
            this.ghostNamesToolStripMenuItem.Click += new System.EventHandler(this.ghostNamesToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 387);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Z80 Emulator";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debuggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emulatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dIPSwitchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem freeplayMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem coin1PerGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem coin1Per2GamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem coins2PerGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem live1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lifes2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lives3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lives5ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem extraLife10000ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extraLife15000PointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extraLife20000PointsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem extraLifeNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem difficultyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ghostNamesToolStripMenuItem;
    }
}

