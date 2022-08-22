using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Z80.Net.Core.Opcodes;
using reg = Z80.Net.Core.Registers;

namespace Z80.Net.Core
{
    public class disasmentry
    {
        public int offset;
        public string name;
        public string oper;
        public string pctext;
        public string regtext;
        public string dtext;
        public string bytetext;
        public int size;
    }

    public class Tracer
    {
        private Memory mem;
        private StreamWriter outfile;
        public bool logging;

        public Tracer(Machine z80)
        {
            mem = z80.mem;
        }

        public void log_to_file()
        {
            if (outfile != null && outfile.BaseStream.CanWrite)
            {
                disasmentry e = disasm(reg.pc, true)[0];
                outfile.WriteLine($"{e.regtext} {e.offset:X4}: {e.dtext}");
            }
        }

        public void open_close_log(bool status)
        {
            logging = status;

            if (logging)
            {
                outfile = new StreamWriter(Environment.CurrentDirectory + "\\trace.log");
            }
            else
            {
                if (outfile != null)
                    outfile.Close();
            }
        }

        public List<disasmentry> disasm(ushort pc, bool get_registers)
        {
            int op = mem.rb(pc);

            int size = 0;
            string name = "";
            string oper = "";
            string data = "";
            string bytes = "";

            List<disasmentry> entry = new List<disasmentry>();

            Opcode dops = get_disasm_entry(op, pc);

            name = dops.name;
            size = dops.size;
            oper = dops.operand;

            disasmentry e = new disasmentry();

            if (size == 1)
            {
                if (oper.Contains("N/A"))
                    data = $"{name}";
                else
                    data = $"{name,-4} {oper}";

                bytes = $"{op:X2}";
            }
            else if (size == 2)
            {
                int b1 = mem.rb((ushort)(pc + 1));

                if (oper.Contains("X4"))
                {
                    int offset = pc + (sbyte)b1 + 2;
                    oper = oper.Replace("X4", $"${offset:X4}");
                    data = $"{name,-4} {oper}";
                }
                else if (oper.Contains("X2"))
                {
                    oper = oper.Replace("X2", $"${b1:X2}");
                    data = $"{name,-4} {oper}";
                }
                else if (oper.Contains("N/A"))
                    data = name;
                else
                    data = $"{name,-4} {oper}";

                bytes = $"{op:X2} {b1:X2}";
            }

            else if (size == 3)
            {
                int b1 = mem.rb((ushort)(pc + 1));
                int b2 = mem.rb((ushort)(pc + 2));

                if (oper.Contains("X4"))
                {
                    oper = oper.Replace("X4", $"${b2:X2}{b1:X2}");
                    data = $"{name,-4} {oper}";
                }
                else if (oper.Contains("X2"))
                {
                    oper = oper.Replace("X2", $"${b2:X2}");
                    data = $"{name,-4} {oper}";
                }
                else
                    data = $"{name} {oper} {b2:X2} {b1:X2}";

                bytes = $"{op:X2} {b1:X2} {b2:X2}";
            }
            else if (size == 4)
            {
                int b1 = mem.rb((ushort)(pc + 1));
                int b2 = mem.rb((ushort)(pc + 2));
                int b3 = mem.rb((ushort)(pc + 3));

                if (oper.Contains("X4"))
                {
                    oper = oper.Replace("X4", $"${b3:X2}{b2:X2}");
                    data = $"{name,-4} {oper}";
                }
                else if (oper.Contains(",X2"))
                {
                    oper = oper.Replace(",X2", $",${b3:X2}");
                    oper = oper.Replace("X2", $"${b2:X2}");
                    data = $"{name,-4} {oper}";
                }
                //else
                //    snprintf(data, TEXTSIZE, "%-4s %s$%02X%02X", name, oper, b2, b1);

                bytes = $"{op:X2} {b1:X2} {b2:X2} {b3:X2}";
            }

            if (name.Contains("pre"))
            {
                size = 1;
                data = $"pre{"",-5}";
                bytes = $"{op:X2}";
            }

            if (get_registers)
            {
                e.regtext = $"SP={reg.sp:X4} AF={reg.af:X4} BC={reg.bc:X4} " +
                    $"DE={reg.de:X4} HL={reg.hl:X4}";// IX={reg.ix:X4} IY={reg.iy:X4}";

            }

            e.name = name;
            e.offset = pc;
            e.size = size;
            e.dtext = data;
            e.bytetext = bytes;
            entry.Add(e);

            return entry;
        }

        public Opcode get_disasm_entry(int op, int pc)
        {
            Opcode dops;

            if (op == 0xcb || op == 0xdd || op == 0xed || op == 0xfd)
            {
                int dop = mem.rw((ushort)pc);

                if (dop == 0xcbdd)
                {
                    byte b3 = mem.rb((ushort)(pc + 3));
                    dops = disasm_ddcb[b3];
                }
                else if (dop == 0xcbfd)
                {
                    int b3 = mem.rw((ushort)(pc + 3));
                    dops = disasm_fdcb[b3];
                }
                else
                {
                    byte b1 = (byte)mem.rw((ushort)(pc + 1));
                    dops = mdisasm[op][b1];
                }
            }
            else
            {
                dops = mdisasm[0x00][op];
            }

            return dops;
        }

    }

}
