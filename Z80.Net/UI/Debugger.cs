using Be.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z80.Net.Core;
using reg = Z80.Net.Core.Registers;
using static Z80.Net.Core.BPType;

namespace Z80.Net
{
    public partial class Debugger : Form
    {
        private PacMachine z80;

        private Form gotodlg;
        private TextBox txtjump;

        public delegate void update_debug();
        public delegate void update_hex(ushort addr, byte v);
        public delegate void update_test_info();

        public static update_debug upd_dbg;
        public static update_hex upd_hex;
        public static update_test_info upd_test_info;

        List<disasmentry> disasm;

        private bool show_tests = false;
        private string edit_value;

        public Debugger()
        {
            InitializeComponent();

            AutoScroll = true;

            upd_dbg = new update_debug(update_form);
            upd_hex = new update_hex(update_hexbox);
            upd_test_info = new update_test_info(get_test_messages);

            lvDisassembly.HeaderStyle = ColumnHeaderStyle.None;
            lvDisassembly.Columns[0].Width = 15;
            lvDisassembly.Columns[1].Width = 60;
            lvDisassembly.Columns[2].Width = 150;
            lvDisassembly.Columns[3].Width = 220;

            lvRegisters.HeaderStyle = ColumnHeaderStyle.None;
            lvBreakpoints.HeaderStyle = ColumnHeaderStyle.None;

            set_double_buffered(lvDisassembly);
            set_double_buffered(lvRegisters);
            set_double_buffered(lvBreakpoints);

            hexMemory.VScrollBarVisible = true;
            hexMemory.BytesPerLine = 16;
            hexMemory.LineInfoVisible = true;
            hexMemory.ColumnInfoVisible = true;
            hexMemory.UseFixedBytesPerLine = true;
            hexMemory.Font = new Font("Consolas", 9);

            hexMemory.ContextMenu = new ContextMenu(
                new MenuItem[] { new MenuItem("Go to"), new MenuItem("Set Write BP"), new MenuItem("Set Read BP") });

            hexMemory.ContextMenu.MenuItems[1].Click += mBPwrite_Clicked;
            hexMemory.ContextMenu.MenuItems[2].Click += mBPread_Click;

            add_tests_names();

            disasm = new List<disasmentry>();

            if (!show_tests)
            {
                lvTests.Hide();
                lblTestStatus.Hide();
                lblCycles.Width = 139;
                Size = new Size(630, Height);
            }
        }

        private void mBPread_Click(object sender, EventArgs e)
        {
            long line = hexMemory.CurrentLine > 0 ? hexMemory.CurrentLine - 1 : hexMemory.CurrentLine;
            ushort addr = (ushort)((line * hexMemory.BytesPerLine) + hexMemory.CurrentPositionInLine - 1);

            bool v = Cpu.breakpoints.Any(bp => bp.addr == addr);

            if (!v)
            {
                Cpu.breakpoints.Add(new Breakpoint(addr, bp_read, true));
                upd_dbg();

                lvBreakpoints.Items.Clear();
                foreach (Breakpoint bp in Cpu.breakpoints)
                    lvBreakpoints.Items.Add(new ListViewItem(new string[] { bp.addr.ToString("X4"), ".R." }));
            }
        }

        private void mBPwrite_Clicked(object sender, EventArgs e)
        {
            long line = hexMemory.CurrentLine > 0 ? hexMemory.CurrentLine - 1 : hexMemory.CurrentLine;
            ushort addr = (ushort)((line * hexMemory.BytesPerLine) + hexMemory.CurrentPositionInLine - 1);

            bool v = Cpu.breakpoints.Any(bp => bp.addr == addr);

            if (!v)
            {
                Cpu.breakpoints.Add(new Breakpoint(addr, bp_write, true));
                upd_dbg();

                lvBreakpoints.Items.Clear();
                foreach (Breakpoint bp in Cpu.breakpoints)
                    lvBreakpoints.Items.Add(new ListViewItem(new string[] { bp.addr.ToString("X4"), ".W." }));
            }
        }

        internal void set_obj(PacMachine m)
        {
            z80 = m;
        }

        private void set_double_buffered(Control c)
        {
            c
            .GetType()
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(c, true, null);
        }

        private void update_form()
        {
            update_disassembly();
            update_registers();
        }

        private void update_hexbox(ushort addr, byte v)
        {
            hexMemory.ByteProvider.WriteByte(addr, v);
            hexMemory.Invalidate();
        }

        private void init_disassembly(int addr = -1)
        {
            lvDisassembly.Items.Clear();
            disasm.Clear();

            //ushort pc = addr > -1 ? (ushort)addr : reg.pc;
            ushort pc = 0;
            if (lvDisassembly.TopItem != null)
                pc = Convert.ToUInt16(lvDisassembly.TopItem.SubItems[1].Text, 16);

            int itemcnt = 0x4000;
            int left = itemcnt;

            for (int i = 0; i < itemcnt; i++)
            {
                disasm.Add(z80.tracer.disasm(pc, false)[0]);
                pc += (ushort)disasm[i].size;
                left -= disasm[i].size;

                disasmentry e = disasm[i];
                var row = new string[] { "", e.offset.ToString("X4"), e.dtext, e.bytetext };
                var lvi = new ListViewItem(row);
                lvDisassembly.Items.Add(lvi);

                if (pc >= 0x4000)
                    break;
            }

            z80.cpu.jump_addr = -1;
        }

        private void update_disassembly(int addr = -1)
        {
            int cleared = 0;
            int pc = Convert.ToUInt16(lvDisassembly.TopItem.SubItems[1].Text, 16);
            int start = lvDisassembly.TopItem.Index;

            for (int i = start; i < start + 19; i++)
            {
                disasm[i] = z80.tracer.disasm((ushort)pc, false)[0];
                pc += (ushort)disasm[i].size;

                disasmentry e = disasm[i];
                var row = new string[] { "", e.offset.ToString("X4"), e.dtext, e.bytetext };
                lvDisassembly.Items[i].Text = row[0];
                lvDisassembly.Items[i].SubItems[1].Text = row[1];
                lvDisassembly.Items[i].SubItems[2].Text = row[2];
                lvDisassembly.Items[i].SubItems[3].Text = row[3];
            }

            pc = addr > -1 ? addr : reg.pc;
            for (int i = 0; i < disasm.Count; i++)
            {
                var lvi = lvDisassembly.Items[i];

                if (lvi.BackColor != Color.White)
                {
                    lvi.BackColor = Color.White;
                }

                if (lvi.Text == ">")
                {
                    lvi.Text = "";
                    lvi.Selected = false;
                    cleared++;
                }

                if (pc == Convert.ToInt32(lvi.SubItems[1].Text, 16))
                {
                    lvi.BackColor = Color.LightBlue;

                    if (z80.cpu.state == Cpu.cstate.debugging)
                        lvi.BackColor = Color.LightCoral;

                    lvi.Text = ">";

                    if (i < 19)
                    {
                        lvDisassembly.EnsureVisible(0);
                    }
                    if (i >= 8)
                    {
                        lvDisassembly.EnsureVisible(i + 9);
                        lvDisassembly.TopItem = lvDisassembly.Items[i - 9];
                    }

                    cleared++;
                }

                if (cleared == 2)
                    break;
            }

            lvDisassembly.HideSelection = false;
        }

        private void update_registers()
        {
            lvRegisters.Items.Clear();
            lvRegisters.Items.Add(new ListViewItem(new string[] { "PC", reg.pc.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "SP", reg.sp.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "AF", reg.af.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "BC", reg.bc.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "DE", reg.de.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "HL", reg.hl.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "IX", reg.ix.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "IY", reg.iy.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "AFs", reg.saf.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "BCs", reg.sbc.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "DEs", reg.sde.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "HLs", reg.shl.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "WZ", reg.wz.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "R", reg.r.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "I", reg.i.ToString("X4") }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "IM", Convert.ToInt32(z80.cpu.im).ToString() }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "IFF1", Convert.ToInt32(z80.cpu.iff1).ToString() }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "IFF2", Convert.ToInt32(z80.cpu.iff2).ToString() }));
            lvRegisters.Items.Add(new ListViewItem(new string[] { "HALT", Convert.ToInt32(z80.cpu.halt).ToString() }));

            lblCycles.Text = "Cycles: " + z80.cpu.cycles.ToString();
        }

        private void add_tests_names()
        {
            lvTests.Columns[0].Width = 240;
            lvTests.Items.Clear();
            byte[] data = File.ReadAllBytes("Content/testfiles/zexall.bin");

            for (int i = 0; i < 67; i++)
            {
                lvTests.Items.Add(new ListViewItem(new string[] { Encoding.ASCII.GetString(data, 0x0103 + (i * 0x60), 0x1e) }));
            }
        }

        public void get_test_messages()
        {
            if (reg.pc == 0x0000)
            {
                z80.cpu.state = Cpu.cstate.debugging;
            }
            else if (reg.pc == 0x002b)
            {
                byte[] c = new byte[1];
                c[0] = z80.mem.Rb(0xffff);
                PacMachine.res_str += Encoding.ASCII.GetString(c);

                switch (reg.c)
                {
                    case 2:
                        //crcstr += (char)cpu.e;
                        break;
                    case 9:
                        ushort addr = reg.de;

                        //if (addr == 0x1dda)
                        //	return;

                        if (addr == 0x1df6)
                            z80.cpu.state = Cpu.cstate.debugging;
                        break;
                }
            }

            lblTestStatus.Text = PacMachine.res_str;
            //lblCycles.Text = "Cycles: " + z80.cpu.cycles.ToString();
        }

        private void btnStepInto_Click(object sender, EventArgs e)
        {
            z80.cpu.step();
            z80.cpu.state = Cpu.cstate.debugging;
            get_test_messages();
            //init_disassembly();
            update_disassembly();
            update_registers();
            update_hexbox((ushort)z80.raddr.addr, z80.raddr.v);
            //z80.cpu.cycles -= (3072000 / 60);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            z80.cpu.state = Cpu.cstate.debugging;
            z80.cpu.reset();
            z80.wsg.reset();
            //init_disassembly();
            update_disassembly();
            update_registers();

            if (PacMachine.test_running && lvTests.SelectedIndices[0] > -1)
            {
                z80.mem.load_test();
                z80.mem.set_test_number(lvTests.SelectedIndices[0]);
            }

            for (int i = 0; i < z80.mem.ram.Length; i++)
                hexMemory.ByteProvider.WriteByte(i, z80.mem.ram[i]);

            hexMemory.Invalidate();

            PacMachine.res_str = "";
            get_test_messages();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            z80.cpu.state = Cpu.cstate.running;
            update_disassembly();
            update_registers();
            z80.cpu.step();
        }

        private void lvDisassembly_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var focus = lvDisassembly.FocusedItem;
                if (focus != null && focus.Bounds.Contains(e.Location))
                {
                    conLvDasmMenu.Show(Cursor.Position);
                }
            }
        }

        private const string hexchars = "0123456789abcdefABCDEF\b";

        private void menuGoto_Click(object sender, EventArgs e)
        {
            gotodlg = new Form();
            gotodlg.MaximizeBox = false;
            gotodlg.FormBorderStyle = FormBorderStyle.FixedSingle;
            gotodlg.Text = "Go to";
            gotodlg.Size = new Size(250, 100);

            txtjump = new TextBox();
            txtjump.Location = new Point(5, 5);
            txtjump.CharacterCasing = CharacterCasing.Upper;
            txtjump.MaxLength = 4;
            txtjump.KeyPress += Txtjump_KeyPress;

            Button btnok = new Button();
            btnok.Text = "Ok";
            btnok.Location = new Point(5, 35);
            btnok.Click += Btnok_Click;

            Button btncancel = new Button();
            btncancel.Text = "Cancel";
            btncancel.Location = new Point(btnok.Size.Width + 5, 35);
            btncancel.Click += Btncancel_Click;

            gotodlg.Controls.AddRange(new Control[] { txtjump, btnok, btncancel });

            gotodlg.ShowDialog();
        }

        private void Btncancel_Click(object sender, EventArgs e)
        {
            gotodlg.Close();
        }

        private void Btnok_Click(object sender, EventArgs e)
        {
            if (txtjump.Text != "")
            {
                update_disassembly(Convert.ToInt32(txtjump.Text, 16));
                gotodlg.Close();
            }
        }

        private void Txtjump_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!hexchars.Contains(e.KeyChar))
                e.Handled = true;
        }

        private void lvDisassembly_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvDisassembly.SelectedItems.Count > 0)
            {
                ushort addr = Convert.ToUInt16(lvDisassembly.SelectedItems[0].SubItems[1].Text, 16);

                bool v = Cpu.breakpoints.Any(n => n.addr == addr);

                if (v)
                    return;

                Cpu.breakpoints.Add(new Breakpoint(addr, 0, true));

                lvBreakpoints.Items.Clear();
                foreach (Breakpoint bp in Cpu.breakpoints)
                    lvBreakpoints.Items.Add(new ListViewItem(new string[] { bp.addr.ToString("X4"), "E.." }));
            }
        }

        private void lvBreakpoints_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvBreakpoints.SelectedItems.Count > 0)
            {
                Cpu.breakpoints.RemoveAt(lvBreakpoints.SelectedIndices[0]);

                lvBreakpoints.Items.Clear();
                foreach (Breakpoint bp in Cpu.breakpoints)
                    lvBreakpoints.Items.Add(new ListViewItem(new string[] { bp.addr.ToString("X4"), bp.type.ToString() }));
            }
        }

        private void lvTests_DoubleClick(object sender, EventArgs e)
        {
            PacMachine.res_str = "";
            get_test_messages();

            if (lvTests.SelectedIndices[0] > -1)
            {
                z80.mem.load_test();
                z80.mem.set_test_number(lvTests.SelectedIndices[0]);

                init_disassembly();
                update_disassembly();
                update_registers();

                for (int i = 0; i < z80.mem.ram.Length; i++)
                    hexMemory.ByteProvider.WriteByte(i, z80.mem.ram[i]);

                hexMemory.Invalidate();
            }
        }

        private void Debugger_Load(object sender, EventArgs e)
        {
            init_disassembly();
            update_disassembly();
            update_registers();

            hexMemory.ByteProvider = new DynamicByteProvider(z80.mem.ram);
        }

        private void btnTrace_Click(object sender, EventArgs e)
        {
            z80.tracer.logging = !z80.tracer.logging;
            z80.tracer.open_close_log(z80.tracer.logging);
            btnTrace.BackColor = z80.tracer.logging ? Color.Green : Color.Red;
        }

        private void hexMemory_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (hexchars.Contains(e.KeyChar))
            {
                edit_value += e.KeyChar;

                if (edit_value.Length >= 2)
                {
                    int ba = (int)(hexMemory.CurrentLine - 1) * hexMemory.BytesPerLine;
                    ba += (int)hexMemory.CurrentPositionInLine - 1;
                    byte v = hexMemory.ByteProvider.ReadByte(ba);
                    z80.mem.ram[ba] = Convert.ToByte(edit_value, 16);
                    edit_value = "";
                    hexMemory.Invalidate();
                }
            }
        }
    }
}
