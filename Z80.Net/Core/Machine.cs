using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80.Net.Core
{
    public class Machine
    {
        public Cpu cpu;
        public Gpu gpu;
        public Memory mem;
        public Wsg wsg;
        public Tracer tracer;
        public RamAddress raddr;

        public static bool test_running;
        public static string res_str;

        public bool[] p1_keys;
        public bool[] p2_keys;

        public Machine()
        {
            mem = new Memory(this);
            cpu = new Cpu(this);
            gpu = new Gpu(this);
            wsg = new Wsg(this);
            tracer = new Tracer(this);
            raddr = new RamAddress();

            Opcodes.create_opcodes();

            mem.load_roms("", 0);

            p1_keys = new bool[8];
            p2_keys = new bool[8];

            cpu.state = Cpu.cstate.running;
            cpu.reset();
        }

        public Machine(bool test)
        {
            mem = new Memory(this);
            cpu = new Cpu(this);
            tracer = new Tracer(this);
            raddr = new RamAddress();

            Opcodes.create_opcodes();
        }
    }
}
