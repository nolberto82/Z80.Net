using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80.Net.Core
{
    public class Breakpoint
    {
        public ushort addr;
        public int type;
        public bool enabled;

        public Breakpoint(ushort addr, int type, bool enabled)
        {
            this.addr = addr;
            this.type = type;
            this.enabled = enabled;
        }
    }

    internal static class BPType
    {
        public const int bp_read = 1;
        public const int bp_write = 2;
        public const int bp_exec = 4;
    };
}
