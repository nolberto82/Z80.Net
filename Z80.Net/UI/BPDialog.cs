using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z80.Net.Core;

namespace Z80.Net.UI
{
    public partial class BPDialog : Form
    {
        public BPDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ushort addr = Convert.ToUInt16(txtBP.Text, 16);

            bool v = Cpu.breakpoints.Any(bp => bp.addr == addr);

            if (!v)
            {
                Cpu.breakpoints.Add(new Breakpoint(addr, 0, true));
                Debugger.upd_dbg();
            }

            Close();
        }
    }
}
