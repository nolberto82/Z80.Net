using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Z80.Net.Core;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Z80.Net
{
    public partial class MainWindow : Form
    {
        Machine z80;
        Display d;

        public MainWindow()
        {
            InitializeComponent();

            z80 = new Machine();
            Display.set_obj(z80);
            //Tiles.set_obj(z80);


            Size = new System.Drawing.Size(232 * 2, 320 * 2);

            d = new Display();
            d.Location = new Point(0, 24);
            d.Size = Size;
            //d.Name = "display1";

            coin1PerGameToolStripMenuItem.Checked = true;
            lives3ToolStripMenuItem.Checked = true;
            extraLife10000ToolStripMenuItem.Checked = true;

            Controls.Add(d);
        }

        private void openRomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd = new OpenFileDialog();
            ofd.Filter = "Nes Roms (*.)|*";
            ofd.InitialDirectory = Environment.CurrentDirectory;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                z80.mem.load_roms(ofd.FileName, 0);
            }
        }

        private void debuggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debugger dbg = new Debugger();
            dbg.set_obj(z80);
            dbg.Show();
        }

        private void tilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TileWindow tiles = new TileWindow();
            //tiles.set_obj(z80);
            //tiles.Show();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.cpu.reset();
            z80.wsg.reset();
        }

        private void reside_window(int size)
        {
            //Size = new Size(Width * size, Height * size);
            Size = new System.Drawing.Size(232 * size, 320 * size);
            d.Size = Size;

        }

        private void uncheck_menu_items(int id)
        {
            switch (id)
            {
                case 0:
                    for (int i = 0; i < 4; i++)
                    {
                        ToolStripMenuItem item = dIPSwitchesToolStripMenuItem.DropDownItems[i] as ToolStripMenuItem;
                        item.Checked = false;
                    }
                    break;
                case 1:
                    for (int i = 5; i < 8; i++)
                    {
                        ToolStripMenuItem item = dIPSwitchesToolStripMenuItem.DropDownItems[i] as ToolStripMenuItem;
                        item.Checked = false;
                    }
                    break;
                case 2:
                    for (int i = 10; i < 14; i++)
                    {
                        ToolStripMenuItem item = dIPSwitchesToolStripMenuItem.DropDownItems[i] as ToolStripMenuItem;
                        item.Checked = false;
                    }
                    break;
            }
        }

        private void freeplayMenuItem1_Click(object sender, EventArgs e)
        {
            z80.mem.dip.coins = 0;
            uncheck_menu_items(0);
            freeplayMenuItem1.Checked = true;
        }

        private void coin1PerGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.coins = 1;
            uncheck_menu_items(0);
            coin1PerGameToolStripMenuItem.Checked = true;
        }

        private void coin1Per2GamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.coins = 2;
            uncheck_menu_items(0);
            coin1Per2GamesToolStripMenuItem.Checked = true;
        }

        private void coins2PerGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.coins = 3;
            uncheck_menu_items(0);
            coins2PerGameToolStripMenuItem.Checked = true;
        }

        private void live1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.lives = 0;
            uncheck_menu_items(1);
            live1ToolStripMenuItem.Checked = true;
        }

        private void lifes2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.lives = 1;
            uncheck_menu_items(1);
            lifes2ToolStripMenuItem.Checked = true;
        }

        private void lives3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.lives = 2;
            uncheck_menu_items(1);
            lives3ToolStripMenuItem.Checked = true;
        }

        private void lives5ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            z80.mem.dip.lives = 3;
            uncheck_menu_items(1);
            lives5ToolStripMenuItem1.Checked = true;
        }

        private void extraLife10000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.score = 0;
            uncheck_menu_items(2);
            extraLife10000ToolStripMenuItem.Checked = true;
        }

        private void extraLife15000PointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.score = 1;
            uncheck_menu_items(2);
            extraLife15000PointsToolStripMenuItem.Checked = true;
        }

        private void extraLife20000PointsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            z80.mem.dip.score = 2;
            uncheck_menu_items(2);
            extraLife20000PointsToolStripMenuItem1.Checked = true;
        }

        private void extraLifeNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.score = 3;
            uncheck_menu_items(2);
            extraLifeNoneToolStripMenuItem.Checked = true;
        }

        private void difficultyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.difficulty ^= 1;
        }

        private void ghostNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            z80.mem.dip.ghosts_names ^= 1;
        }
    }
}
