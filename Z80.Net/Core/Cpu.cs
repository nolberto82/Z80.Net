using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reg = Z80.Net.Core.Registers;
using static Z80.Net.Core.Opcodes;

namespace Z80.Net.Core
{
    public class Cpu
    {
        #region constants
        const int FC = 0x01;
        const int FN = 0x02;
        const int FP = 0x04;
        const int FX = 0x08;
        const int FH = 0x10;
        const int FY = 0x20;
        const int FZ = 0x40;
        const int FS = 0x80;

        const int CYCLES_PER_FRAME = 3072000 / 60;
        #endregion

        #region PUBLIC
        public int cycles, im;
        public cstate state;
        public bool iff1, iff2, inte, halt, crashed;
        public int waddr, raddr, jump_addr;

        public static List<Breakpoint> breakpoints;

        public byte[] ports;

        public enum cstate
        {
            running,
            debugging
        }
        #endregion

        #region PRIVATE

        private Memory mem;
        private PacMachine z80;

        #endregion

        public Cpu(PacMachine z80)
        {
            mem = z80.mem;
            this.z80 = z80;
            breakpoints = new List<Breakpoint>();
        }

        public void step()
        {
            int op = mem.Rbd(reg.pc);

            exec_00(op);

            if (halt)
                op_halt();
            else if (cycles > CYCLES_PER_FRAME)
                handle_interrupts();
        }

        void set_port(int v)
        {
            int id = mem.Rbd((ushort)(reg.pc + 1));
            mem.ports[mem.Rbd((ushort)(reg.pc + 1))] = (byte)v;
            reg.wz = (ushort)((id + 1) | reg.a << 8);
        }

        void exec_00(int op)
        {
            switch (op)
            {
                case 0x00:
                    {
                        advance(op);
                        break;
                    }
                case 0x01:
                    {
                        reg.bc = mem.Rw((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x02:
                    {
                        mem.Wb(reg.bc, reg.a);
                        reg.w = reg.a;
                        reg.z = (byte)(reg.c + 1);
                        advance(op);
                        break;
                    }
                case 0x03:
                    {
                        reg.bc++;
                        advance(op);
                        break;
                    }
                case 0x04:
                    {
                        reg.b = op_inc8(reg.b);
                        advance(op);
                        break;
                    }
                case 0x05:
                    {
                        reg.b = op_dec8(reg.b);
                        advance(op);
                        break;
                    }
                case 0x06:
                    {
                        reg.b = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x07:
                    {
                        op_rlca();
                        advance(op);
                        break;
                    }

                case 0x09:
                    {
                        reg.hl = op_add(reg.hl, reg.bc);
                        advance(op);
                        break;
                    }
                case 0x0a:
                    {
                        reg.a = mem.Rb(reg.bc);
                        reg.wz = (ushort)(reg.bc + 1);
                        advance(op);
                        break;
                    }
                case 0x0b:
                    {
                        reg.bc--;
                        advance(op);
                        break;
                    }
                case 0x0c:
                    {
                        reg.c = op_inc8(reg.c);
                        advance(op);
                        break;
                    }
                case 0x0d:
                    {
                        reg.c = op_dec8(reg.c);
                        advance(op);
                        break;
                    }
                case 0x0e:
                    {
                        reg.c = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x0f:
                    {
                        op_rrca();
                        advance(op);
                        break;
                    }
                case 0x10:
                    {
                        op_djnz(op);
                        break;
                    }
                case 0x11:
                    {
                        reg.de = mem.Rw((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x12:
                    {
                        mem.Wb(reg.de, reg.a);
                        reg.w = reg.a;
                        reg.z = (byte)(reg.e + 1);
                        advance(op);
                        break;
                    }
                case 0x13:
                    {
                        reg.de++;
                        advance(op);
                        break;
                    }
                case 0x14:
                    {
                        reg.d = op_inc8(reg.d);
                        advance(op);
                        break;
                    }
                case 0x15:
                    {
                        reg.d = op_dec8(reg.d);
                        advance(op);
                        break;
                    }
                case 0x16:
                    {
                        reg.d = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x17:
                    {
                        op_rla8();
                        advance(op);
                        break;
                    }
                case 0x18:
                    {
                        op_jr(op, true);
                        reg.wz = reg.pc;
                        break;
                    }
                case 0x19:
                    {
                        reg.hl = op_add(reg.hl, reg.de);
                        advance(op);
                        break;
                    }
                case 0x1a:
                    {
                        reg.a = mem.Rb(reg.de);
                        reg.wz = (ushort)(reg.de + 1);
                        advance(op);
                        break;
                    }
                case 0x1b:
                    {
                        reg.de--;
                        advance(op);
                        break;
                    }
                case 0x1c:
                    {
                        reg.e = op_inc8(reg.e);
                        advance(op);
                        break;
                    }
                case 0x1d:
                    {
                        reg.e = op_dec8(reg.e);
                        advance(op);
                        break;
                    }
                case 0x1e:
                    {
                        reg.e = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x1f:
                    {
                        op_rra();
                        advance(op);
                        break;
                    }
                case 0x20:
                    {
                        if (op_jr(op, (reg.f & FZ) == 0)) reg.wz = reg.pc;
                        break;
                    }

                case 0x21:
                    {
                        reg.hl = mem.Rw((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x22:
                    {
                        mem.ww(reg.wz = mem.Rw((ushort)(reg.pc + 1)), reg.hl);
                        reg.wz++;
                        advance(op);
                        break;
                    }
                case 0x23:
                    {
                        reg.hl++;
                        advance(op);
                        break;
                    }
                case 0x24:
                    {
                        reg.h = op_inc8(reg.h);
                        advance(op);
                        break;
                    }
                case 0x25:
                    {
                        reg.h = op_dec8(reg.h);
                        advance(op);
                        break;
                    }
                case 0x26:
                    {
                        reg.h = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x27:
                    {
                        op_daa();
                        advance(op);
                        break;
                    }
                case 0x28:
                    {
                        if (op_jr(op, (reg.f & FZ) > 0)) reg.wz = reg.pc;
                        break;
                    }
                case 0x29:
                    {
                        reg.hl = op_add(reg.hl, reg.hl);
                        advance(op);
                        break;
                    }
                case 0x2a:
                    {
                        reg.hl = mem.Rw(reg.wz = mem.Rw((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0x2b:
                    {
                        reg.hl--;
                        advance(op);
                        break;
                    }
                case 0x2c:
                    {
                        reg.l = op_inc8(reg.l);
                        advance(op);
                        break;
                    }
                case 0x2d:
                    {
                        reg.l = op_dec8(reg.l);
                        advance(op);
                        break;
                    }
                case 0x2e:
                    {
                        reg.l = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x2f:
                    {
                        op_cpl();
                        advance(op);
                        break;
                    }
                case 0x30:
                    {
                        if (op_jr(op, (reg.f & FC) == 0)) reg.wz = reg.pc;
                        break;
                    }
                case 0x31:
                    {
                        reg.sp = mem.Rw((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x32:
                    {
                        mem.Wb(reg.wz = mem.Rw((ushort)(reg.pc + 1)), reg.a);
                        reg.wz++;
                        reg.w = reg.a;
                        advance(op);
                        break;
                    }
                case 0x33:
                    {
                        reg.sp++;
                        advance(op);
                        break;
                    }
                case 0x34:
                    {
                        mem.Wb(reg.hl, op_inc8(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x35:
                    {
                        mem.Wb(reg.hl, op_dec8(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x36:
                    {
                        mem.Wb(reg.hl, mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0x37:
                    {
                        op_scf();
                        advance(op);
                        break;
                    }
                case 0x38:
                    {
                        if (op_jr(op, (reg.f & FC) > 0)) reg.wz = reg.pc;
                        break;
                    }
                case 0x39:
                    {
                        reg.hl = op_add(reg.hl, reg.sp);
                        advance(op);
                        break;
                    }
                case 0x3a:
                    {
                        reg.a = mem.Rb(reg.wz = mem.Rw((ushort)(reg.pc + 1)));
                        reg.wz++;
                        advance(op);
                        break;
                    }
                case 0x3b:
                    {
                        reg.sp--;
                        advance(op);
                        break;
                    }
                case 0x3c:
                    {
                        reg.a = op_inc8(reg.a);
                        advance(op);
                        break;
                    }
                case 0x3d:
                    {
                        reg.a = op_dec8(reg.a);
                        advance(op);
                        break;
                    }
                case 0x3e:
                    {
                        reg.a = mem.Rb((ushort)(reg.pc + 1));
                        advance(op);
                        break;
                    }
                case 0x3f:
                    {
                        op_ccf();
                        advance(op);
                        break;
                    }
                case 0x40:
                    {
                        advance(op);
                        break;
                    }

                case 0x41:
                    {
                        reg.b = reg.c;
                        advance(op);
                        break;
                    }
                case 0x42:
                    {
                        reg.b = reg.d;
                        advance(op);
                        break;
                    }
                case 0x43:
                    {
                        reg.b = reg.e;
                        advance(op);
                        break;
                    }
                case 0x44:
                    {
                        reg.b = reg.h;
                        advance(op);
                        break;
                    }
                case 0x45:
                    {
                        reg.b = reg.l;
                        advance(op);
                        break;
                    }
                case 0x46:
                    {
                        reg.b = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x47:
                    {
                        reg.b = reg.a;
                        advance(op);
                        break;
                    }
                case 0x48:
                    {
                        reg.c = reg.b;
                        advance(op);
                        break;
                    }
                case 0x49:
                    {
                        advance(op);
                        break;
                    }
                case 0x4a:
                    {
                        reg.c = reg.d;
                        advance(op);
                        break;
                    }
                case 0x4b:
                    {
                        reg.c = reg.e;
                        advance(op);
                        break;
                    }

                case 0x4c:
                    {
                        reg.c = reg.h;
                        advance(op);
                        break;
                    }
                case 0x4d:
                    {
                        reg.c = reg.l;
                        advance(op);
                        break;
                    }
                case 0x4e:
                    {
                        reg.c = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x4f:
                    {
                        reg.c = reg.a;
                        advance(op);
                        break;
                    }
                case 0x50:
                    {
                        reg.d = reg.b;
                        advance(op);
                        break;
                    }
                case 0x51:
                    {
                        reg.d = reg.c;
                        advance(op);
                        break;
                    }
                case 0x52:
                    {
                        advance(op);
                        break;
                    }
                case 0x53:
                    {
                        reg.d = reg.e;
                        advance(op);
                        break;
                    }
                case 0x54:
                    {
                        reg.d = reg.h;
                        advance(op);
                        break;
                    }
                case 0x55:
                    {
                        reg.d = reg.l;
                        advance(op);
                        break;
                    }
                case 0x56:
                    {
                        reg.d = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x57:
                    {
                        reg.d = reg.a;
                        advance(op);
                        break;
                    }
                case 0x58:
                    {
                        reg.e = reg.b;
                        advance(op);
                        break;
                    }
                case 0x59:
                    {
                        reg.e = reg.c;
                        advance(op);
                        break;
                    }
                case 0x5a:
                    {
                        reg.e = reg.d;
                        advance(op);
                        break;
                    }
                case 0x5b:
                    {
                        add_pc(op);
                        break;
                    }
                case 0x5c:
                    {
                        reg.e = reg.h;
                        advance(op);
                        break;
                    }
                case 0x5d:
                    {
                        reg.e = reg.l;
                        advance(op);
                        break;
                    }
                case 0x5e:
                    {
                        reg.e = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x5f:
                    {
                        reg.e = reg.a;
                        advance(op);
                        break;
                    }
                case 0x60:
                    {
                        reg.h = reg.b;
                        advance(op);
                        break;
                    }
                case 0x61:
                    {
                        reg.h = reg.c;
                        advance(op);
                        break;
                    }
                case 0x62:
                    {
                        reg.h = reg.d;
                        advance(op);
                        break;
                    }
                case 0x63:
                    {
                        reg.h = reg.e;
                        advance(op);
                        break;
                    }
                case 0x64:
                    {
                        advance(op);
                        break;
                    }
                case 0x65:
                    {
                        reg.h = reg.l;
                        advance(op);
                        break;
                    }
                case 0x66:
                    {
                        reg.h = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x67:
                    {
                        reg.h = reg.a;
                        advance(op);
                        break;
                    }
                case 0x68:
                    {
                        reg.l = reg.b;
                        advance(op);
                        break;
                    }
                case 0x69:
                    {
                        reg.l = reg.c;
                        advance(op);
                        break;
                    }
                case 0x6a:
                    {
                        reg.l = reg.d;
                        advance(op);
                        break;
                    }
                case 0x6b:
                    {
                        reg.l = reg.e;
                        advance(op);
                        break;
                    }
                case 0x6c:
                    {
                        reg.l = reg.h;
                        advance(op);
                        break;
                    }
                case 0x6d:
                    {
                        advance(op);
                        break;
                    }
                case 0x6e:
                    {
                        reg.l = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x6f:
                    {
                        reg.l = reg.a;
                        advance(op);
                        break;
                    }

                case 0x70:
                    {
                        mem.Wb(reg.hl, reg.b);
                        advance(op);
                        break;
                    }
                case 0x71:
                    {
                        mem.Wb(reg.hl, reg.c);
                        advance(op);
                        break;
                    }
                case 0x72:
                    {
                        mem.Wb(reg.hl, reg.d);
                        advance(op);
                        break;
                    }
                case 0x73:
                    {
                        mem.Wb(reg.hl, reg.e);
                        advance(op);
                        break;
                    }
                case 0x74:
                    {
                        mem.Wb(reg.hl, reg.h);
                        advance(op);
                        break;
                    }
                case 0x75:
                    {
                        mem.Wb(reg.hl, reg.l);
                        advance(op);
                        break;
                    }
                case 0x76:
                    {
                        halt = true;
                        break;
                    }
                case 0x77:
                    {
                        mem.Wb(reg.hl, reg.a);
                        advance(op);
                        break;
                    }
                case 0x78:
                    {
                        reg.a = reg.b;
                        advance(op);
                        break;
                    }
                case 0x79:
                    {
                        reg.a = reg.c;
                        advance(op);
                        break;
                    }
                case 0x7a:
                    {
                        reg.a = reg.d;
                        advance(op);
                        break;
                    }
                case 0x7b:
                    {
                        reg.a = reg.e;
                        advance(op);
                        break;
                    }
                case 0x7c:
                    {
                        reg.a = reg.h;
                        advance(op);
                        break;
                    }
                case 0x7d:
                    {
                        reg.a = reg.l;
                        advance(op);
                        break;
                    }
                case 0x7e:
                    {
                        reg.a = mem.Rb(reg.hl);
                        advance(op);
                        break;
                    }
                case 0x7f:
                    {
                        advance(op);
                        break;
                    }
                case 0x80:
                    {
                        op_add8(reg.b);
                        advance(op);
                        break;
                    }
                case 0x81:
                    {
                        op_add8(reg.c);
                        advance(op);
                        break;
                    }
                case 0x82:
                    {
                        op_add8(reg.d);
                        advance(op);
                        break;
                    }
                case 0x83:
                    {
                        op_add8(reg.e);
                        advance(op);
                        break;
                    }
                case 0x84:
                    {
                        op_add8(reg.h);
                        advance(op);
                        break;
                    }
                case 0x85:
                    {
                        op_add8(reg.l);
                        advance(op);
                        break;
                    }
                case 0x86:
                    {
                        op_add8(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0x87:
                    {
                        op_add8(reg.a);
                        advance(op);
                        break;
                    }
                case 0x88:
                    {
                        op_adc8(reg.b);
                        advance(op);
                        break;
                    }
                case 0x89:
                    {
                        op_adc8(reg.c);
                        advance(op);
                        break;
                    }
                case 0x8a:
                    {
                        op_adc8(reg.d);
                        advance(op);
                        break;
                    }
                case 0x8b:
                    {
                        op_adc8(reg.e);
                        advance(op);
                        break;
                    }
                case 0x8c:
                    {
                        op_adc8(reg.h);
                        advance(op);
                        break;
                    }
                case 0x8d:
                    {
                        op_adc8(reg.l);
                        advance(op);
                        break;
                    }
                case 0x8e:
                    {
                        op_adc8(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0x8f:
                    {
                        op_adc8(reg.a);
                        advance(op);
                        break;
                    }
                case 0x90:
                    {
                        op_sub8(reg.b);
                        advance(op);
                        break;
                    }
                case 0x91:
                    {
                        op_sub8(reg.c);
                        advance(op);
                        break;
                    }
                case 0x92:
                    {
                        op_sub8(reg.d);
                        advance(op);
                        break;
                    }
                case 0x93:
                    {
                        op_sub8(reg.e);
                        advance(op);
                        break;
                    }
                case 0x94:
                    {
                        op_sub8(reg.h);
                        advance(op);
                        break;
                    }
                case 0x95:
                    {
                        op_sub8(reg.l);
                        advance(op);
                        break;
                    }
                case 0x96:
                    {
                        op_sub8(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0x97:
                    {
                        op_sub8(reg.a);
                        advance(op);
                        break;
                    }
                case 0x98:
                    {
                        op_sbc8(reg.b);
                        advance(op);
                        break;
                    }
                case 0x99:
                    {
                        op_sbc8(reg.c);
                        advance(op);
                        break;
                    }
                case 0x9a:
                    {
                        op_sbc8(reg.d);
                        advance(op);
                        break;
                    }
                case 0x9b:
                    {
                        op_sbc8(reg.e);
                        advance(op);
                        break;
                    }
                case 0x9c:
                    {
                        op_sbc8(reg.h);
                        advance(op);
                        break;
                    }
                case 0x9d:
                    {
                        op_sbc8(reg.l);
                        advance(op);
                        break;
                    }
                case 0x9e:
                    {
                        op_sbc8(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0x9f:
                    {
                        op_sbc8(reg.a);
                        advance(op);
                        break;
                    }
                case 0xa0:
                    {
                        op_and(reg.b);
                        advance(op);
                        break;
                    }
                case 0xa1:
                    {
                        op_and(reg.c);
                        advance(op);
                        break;
                    }
                case 0xa2:
                    {
                        op_and(reg.d);
                        advance(op);
                        break;
                    }
                case 0xa3:
                    {
                        op_and(reg.e);
                        advance(op);
                        break;
                    }
                case 0xa4:
                    {
                        op_and(reg.h);
                        advance(op);
                        break;
                    }
                case 0xa5:
                    {
                        op_and(reg.l);
                        advance(op);
                        break;
                    }
                case 0xa6:
                    {
                        op_and(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0xa7:
                    {
                        op_and(reg.a);
                        advance(op);
                        break;
                    }
                case 0xa8:
                    {
                        op_xor(reg.b);
                        advance(op);
                        break;
                    }
                case 0xa9:
                    {
                        op_xor(reg.c);
                        advance(op);
                        break;
                    }
                case 0xaa:
                    {
                        op_xor(reg.d);
                        advance(op);
                        break;
                    }
                case 0xab:
                    {
                        op_xor(reg.e);
                        advance(op);
                        break;
                    }
                case 0xac:
                    {
                        op_xor(reg.h);
                        advance(op);
                        break;
                    }
                case 0xad:
                    {
                        op_xor(reg.l);
                        advance(op);
                        break;
                    }
                case 0xae:
                    {
                        op_xor(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0xaf:
                    {
                        op_xor(reg.a);
                        advance(op);
                        break;
                    }
                case 0xb0:
                    {
                        op_or(reg.b);
                        advance(op);
                        break;
                    }
                case 0xb1:
                    {
                        op_or(reg.c);
                        advance(op);
                        break;
                    }
                case 0xb2:
                    {
                        op_or(reg.d);
                        advance(op);
                        break;
                    }
                case 0xb3:
                    {
                        op_or(reg.e);
                        advance(op);
                        break;
                    }
                case 0xb4:
                    {
                        op_or(reg.h);
                        advance(op);
                        break;
                    }
                case 0xb5:
                    {
                        op_or(reg.l);
                        advance(op);
                        break;
                    }
                case 0xb6:
                    {
                        op_or(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0xb7:
                    {
                        op_or(reg.a);
                        advance(op);
                        break;
                    }
                case 0xb8:
                    {
                        op_cp(reg.b);
                        advance(op);
                        break;
                    }
                case 0xb9:
                    {
                        op_cp(reg.c);
                        advance(op);
                        break;
                    }
                case 0xba:
                    {
                        op_cp(reg.d);
                        advance(op);
                        break;
                    }
                case 0xbb:
                    {
                        op_cp(reg.e);
                        advance(op);
                        break;
                    }
                case 0xbc:
                    {
                        op_cp(reg.h);
                        advance(op);
                        break;
                    }
                case 0xbd:
                    {
                        op_cp(reg.l);
                        advance(op);
                        break;
                    }
                case 0xbe:
                    {
                        op_cp(mem.Rb(reg.hl));
                        advance(op);
                        break;
                    }
                case 0xbf:
                    {
                        op_cp(reg.a);
                        advance(op);
                        break;
                    }
                case 0xc0:
                    {
                        op_ret(op, (reg.f & FZ) == 0);
                        break;
                    }
                case 0xc1:
                    {
                        reg.bc = op_pop();
                        advance(op);
                        break;
                    }
                case 0xc2:
                    {
                        op_jp(op, (reg.f & FZ) == 0);
                        break;
                    }
                case 0xc3:
                    {
                        op_jp(op, true);
                        break;
                    }
                case 0xc4:
                    {
                        op_call(op, (reg.f & FZ) == 0);
                        add_cyc(op);
                        break;
                    }
                case 0xc5:
                    {
                        op_push(reg.bc);
                        advance(op);
                        break;
                    }
                case 0xc6:
                    {
                        op_add8(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }

                case 0xc8:
                    {
                        op_ret(op, (reg.f & FZ) > 0);
                        break;
                    }
                case 0xc9:
                    {
                        op_ret(op, true);
                        break;
                    }

                case 0xca:
                    {
                        op_jp(op, (reg.f & FZ) > 0);
                        break;
                    }
                case 0xcb:
                    {
                        exec_cb(op);
                        break;
                    }
                case 0xcc:
                    {
                        op_call(op, (reg.f & FZ) > 0);
                        add_cyc(op);
                        break;
                    }
                case 0xcd:
                    {
                        op_call(op, true);
                        add_cyc(op);
                        break;
                    }
                case 0xce:
                    {
                        op_adc8(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xcf:
                    {
                        op_rst(0x08);
                        add_cyc(op);
                        break;
                    }
                case 0xd0:
                    {
                        op_ret(op, (reg.f & FC) == 0);
                        break;
                    }
                case 0xd1:
                    {
                        reg.de = op_pop();
                        advance(op);
                        break;
                    }
                case 0xd2:
                    {
                        op_jp(op, (reg.f & FC) == 0);
                        break;
                    }
                case 0xd3:
                    {
                        set_port(reg.a);
                        advance(op);
                        break;
                    }
                case 0xd4:
                    {
                        op_call(op, (reg.f & FC) == 0);
                        add_cyc(op);
                        break;
                    }
                case 0xd5:
                    {
                        op_push(reg.de);
                        advance(op);
                        break;
                    }
                case 0xd6:
                    {
                        op_sub8(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xd7:
                    {
                        op_rst(0x10);
                        add_cyc(op);
                        break;
                    }
                case 0xd8:
                    {
                        op_ret(op, (reg.f & FC) > 0);
                        break;
                    }
                case 0xd9:
                    {
                        op_exx();
                        advance(op);
                        break;
                    }
                case 0xda:
                    {
                        op_jp(op, (reg.f & FC) > 0);
                        break;
                    }
                case 0xdb:
                    {
                        reg.a = mem.ports[0];
                        advance(op);
                        break;
                    }
                case 0xdc:
                    {
                        op_call(op, (reg.f & FC) > 0);
                        add_cyc(op);
                        break;
                    }
                case 0xdd:
                    {
                        exec_dd(op);
                        break;
                    }
                case 0xde:
                    {
                        op_sbc8(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xdf:
                    {
                        op_rst(0x18);
                        add_cyc(op);
                        break;
                    }
                case 0xe1:
                    {
                        reg.hl = op_pop();
                        advance(op);
                        break;
                    }
                case 0xe5:
                    {
                        op_push(reg.hl);
                        advance(op);
                        break;
                    }
                case 0xe6:
                    {
                        op_and(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xe7:
                    {
                        op_rst(0x20);
                        add_cyc(op);
                        break;
                    }
                case 0xe9:
                    {
                        reg.pc = reg.hl;
                        add_cyc(op);
                        break;
                    }
                case 0xea:
                    {
                        op_jp(op, (reg.f & FP) == 0);
                        break;
                    }
                case 0xeb:
                    {
                        (reg.de, reg.hl) = (reg.hl, reg.de);
                        advance(op);
                        break;
                    }
                case 0xed:
                    {
                        exec_ed(op);
                        break;
                    }
                case 0xee:
                    {
                        op_xor(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xef:
                    {
                        op_rst(0x28);
                        add_cyc(op);
                        break;
                    }
                case 0xf1:
                    {
                        reg.af = op_pop();
                        advance(op);
                        break;
                    }
                case 0xf3:
                    {
                        iff1 = false;
                        advance(op);
                        break;
                    }
                case 0xf5:
                    {
                        op_push(reg.af);
                        advance(op);
                        break;
                    }
                case 0xf6:
                    {
                        op_or(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xf7:
                    {
                        op_rst(0x30);
                        add_cyc(op);
                        break;
                    }
                case 0xf8:
                    {
                        op_ret(op, (reg.f & FS) > 0);
                        add_cyc(op);
                        break;
                    }
                case 0xf9:
                    {
                        reg.sp = reg.hl;
                        advance(op);
                        break;
                    }
                case 0xfa:
                    {
                        op_jp(op, (reg.f & FS) > 0);
                        break;
                    }
                case 0xfb:
                    {
                        iff1 = true;
                        advance(op);
                        break;
                    }
                case 0xfd:
                    {
                        exec_fd(op);
                        break;
                    }
                case 0xfe:
                    {
                        op_cp(mem.Rb((ushort)(reg.pc + 1)));
                        advance(op);
                        break;
                    }
                case 0xff:
                    {
                        op_rst(0x38);
                        add_cyc(op);
                        break;
                    }
                default:
                    {
                        //printf("%04X %02X\n", reg.pc, op);
                        state = cstate.debugging;
                        break;
                    }


            }

            if (PacMachine.test_running)
            {
                if (op == 0x76)
                {
                    //printf("%04X %02X\n", reg.pc, op);
                    state = cstate.debugging;
                }
            }
        }

        void exec_cb(int op)
        {
            byte b1 = mem.Rbd((ushort)(reg.pc + 1));

            switch (b1)
            {
                case 0x00:
                    {
                        reg.b = op_rlc(reg.b);
                        advance(op);
                        break;
                    }
                case 0x01:
                    {
                        reg.c = op_rlc(reg.c);
                        advance(op);
                        break;
                    }
                case 0x02:
                    {
                        reg.d = op_rlc(reg.d);
                        advance(op);
                        break;
                    }
                case 0x03:
                    {
                        reg.e = op_rlc(reg.e);
                        advance(op);
                        break;
                    }
                case 0x04:
                    {
                        reg.h = op_rlc(reg.h);
                        advance(op);
                        break;
                    }
                case 0x05:
                    {
                        reg.l = op_rlc(reg.l);
                        advance(op);
                        break;
                    }
                case 0x06:
                    {
                        mem.Wb(reg.hl, op_rlc(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x07:
                    {
                        reg.a = op_rlc(reg.a);
                        advance(op);
                        break;
                    }
                case 0x08:
                    {
                        reg.b = op_rrc(reg.b);
                        advance(op);
                        break;
                    }
                case 0x09:
                    {
                        reg.c = op_rrc(reg.c);
                        advance(op);
                        break;
                    }
                case 0x0a:
                    {
                        reg.d = op_rrc(reg.d);
                        advance(op);
                        break;
                    }
                case 0x0b:
                    {
                        reg.e = op_rrc(reg.e);
                        advance(op);
                        break;
                    }
                case 0x0c:
                    {
                        reg.h = op_rrc(reg.h);
                        advance(op);
                        break;
                    }
                case 0x0d:
                    {
                        reg.l = op_rrc(reg.l);
                        advance(op);
                        break;
                    }
                case 0x0e:
                    {
                        mem.Wb(reg.hl, op_rrc(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x0f:
                    {
                        reg.a = op_rrc(reg.a);
                        advance(op);
                        break;
                    }
                case 0x10:
                    {
                        reg.b = op_rl(reg.b);
                        advance(op);
                        break;
                    }
                case 0x11:
                    {
                        reg.c = op_rl(reg.c);
                        advance(op);
                        break;
                    }
                case 0x12:
                    {
                        reg.d = op_rl(reg.d);
                        advance(op);
                        break;
                    }
                case 0x13:
                    {
                        reg.e = op_rl(reg.e);
                        advance(op);
                        break;
                    }
                case 0x14:
                    {
                        reg.h = op_rl(reg.h);
                        advance(op);
                        break;
                    }
                case 0x15:
                    {
                        reg.l = op_rl(reg.l);
                        advance(op);
                        break;
                    }
                case 0x16:
                    {
                        mem.Wb(reg.hl, op_rl(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x17:
                    {
                        reg.a = op_rl(reg.a);
                        advance(op);
                        break;
                    }
                case 0x18:
                    {
                        reg.b = op_rr(reg.b);
                        advance(op);
                        break;
                    }
                case 0x19:
                    {
                        reg.c = op_rr(reg.c);
                        advance(op);
                        break;
                    }
                case 0x1a:
                    {
                        reg.d = op_rr(reg.d);
                        advance(op);
                        break;
                    }
                case 0x1b:
                    {
                        reg.e = op_rr(reg.e);
                        advance(op);
                        break;
                    }
                case 0x1c:
                    {
                        reg.h = op_rr(reg.h);
                        advance(op);
                        break;
                    }
                case 0x1d:
                    {
                        reg.l = op_rr(reg.l);
                        advance(op);
                        break;
                    }
                case 0x1e:
                    {
                        mem.Wb(reg.hl, op_rr(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x1f:
                    {
                        reg.a = op_rr(reg.a);
                        advance(op);
                        break;
                    }
                case 0x20:
                    {
                        reg.b = op_sla(reg.b);
                        advance(op);
                        break;
                    }
                case 0x21:
                    {
                        reg.c = op_sla(reg.c);
                        advance(op);
                        break;
                    }
                case 0x22:
                    {
                        reg.d = op_sla(reg.d);
                        advance(op);
                        break;
                    }
                case 0x23:
                    {
                        reg.e = op_sla(reg.e);
                        advance(op);
                        break;
                    }
                case 0x24:
                    {
                        reg.h = op_sla(reg.h);
                        advance(op);
                        break;
                    }
                case 0x25:
                    {
                        reg.l = op_sla(reg.l);
                        advance(op);
                        break;
                    }
                case 0x26:
                    {
                        mem.Wb(reg.hl, op_sla(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x27:
                    {
                        reg.a = op_sla(reg.a);
                        advance(op);
                        break;
                    }
                case 0x28:
                    {
                        reg.b = op_sra(reg.b);
                        advance(op);
                        break;
                    }
                case 0x29:
                    {
                        reg.c = op_sra(reg.c);
                        advance(op);
                        break;
                    }
                case 0x2a:
                    {
                        reg.d = op_sra(reg.d);
                        advance(op);
                        break;
                    }
                case 0x2b:
                    {
                        reg.e = op_sra(reg.e);
                        advance(op);
                        break;
                    }
                case 0x2c:
                    {
                        reg.h = op_sra(reg.h);
                        advance(op);
                        break;
                    }
                case 0x2d:
                    {
                        reg.l = op_sra(reg.l);
                        advance(op);
                        break;
                    }
                case 0x2e:
                    {
                        mem.Wb(reg.hl, op_sra(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x2f:
                    {
                        reg.a = op_sra(reg.a);
                        advance(op);
                        break;
                    }
                case 0x30:
                    {
                        reg.b = op_sll(reg.b);
                        advance(op);
                        break;
                    }
                case 0x31:
                    {
                        reg.c = op_sll(reg.c);
                        advance(op);
                        break;
                    }
                case 0x32:
                    {
                        reg.d = op_sll(reg.d);
                        advance(op);
                        break;
                    }
                case 0x33:
                    {
                        reg.e = op_sll(reg.e);
                        advance(op);
                        break;
                    }
                case 0x34:
                    {
                        reg.h = op_sll(reg.h);
                        advance(op);
                        break;
                    }
                case 0x35:
                    {
                        reg.l = op_sll(reg.l);
                        advance(op);
                        break;
                    }
                case 0x36:
                    {
                        mem.Wb(reg.hl, op_sll(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x37:
                    {
                        reg.a = op_sll(reg.a);
                        advance(op);
                        break;
                    }
                case 0x38:
                    {
                        reg.b = op_srl(reg.b);
                        advance(op);
                        break;
                    }
                case 0x39:
                    {
                        reg.c = op_srl(reg.c);
                        advance(op);
                        break;
                    }
                case 0x3a:
                    {
                        reg.d = op_srl(reg.d);
                        advance(op);
                        break;
                    }
                case 0x3b:
                    {
                        reg.e = op_srl(reg.e);
                        advance(op);
                        break;
                    }
                case 0x3c:
                    {
                        reg.h = op_srl(reg.h);
                        advance(op);
                        break;
                    }
                case 0x3d:
                    {
                        reg.l = op_srl(reg.l);
                        advance(op);
                        break;
                    }
                case 0x3e:
                    {
                        mem.Wb(reg.hl, op_srl(mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x3f:
                    {
                        reg.a = op_srl(reg.a);
                        advance(op);
                        break;
                    }
                case 0x40:
                    {
                        op_bit(0, reg.b);
                        advance(op);
                        break;
                    }
                case 0x41:
                    {
                        op_bit(0, reg.c);
                        advance(op);
                        break;
                    }
                case 0x42:
                    {
                        op_bit(0, reg.d);
                        advance(op);
                        break;
                    }
                case 0x43:
                    {
                        op_bit(0, reg.e);
                        advance(op);
                        break;
                    }
                case 0x44:
                    {
                        op_bit(0, reg.h);
                        advance(op);
                        break;
                    }
                case 0x45:
                    {
                        op_bit(0, reg.l);
                        advance(op);
                        break;
                    }
                case 0x46:
                    {
                        op_bit(0, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x47:
                    {
                        op_bit(0, reg.a);
                        advance(op);
                        break;
                    }
                case 0x48:
                    {
                        op_bit(1, reg.b);
                        advance(op);
                        break;
                    }
                case 0x49:
                    {
                        op_bit(1, reg.c);
                        advance(op);
                        break;
                    }
                case 0x4a:
                    {
                        op_bit(1, reg.d);
                        advance(op);
                        break;
                    }
                case 0x4b:
                    {
                        op_bit(1, reg.e);
                        advance(op);
                        break;
                    }
                case 0x4c:
                    {
                        op_bit(1, reg.h);
                        advance(op);
                        break;
                    }
                case 0x4d:
                    {
                        op_bit(1, reg.l);
                        advance(op);
                        break;
                    }
                case 0x4e:
                    {
                        op_bit(1, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x4f:
                    {
                        op_bit(1, reg.a);
                        advance(op);
                        break;
                    }
                case 0x50:
                    {
                        op_bit(2, reg.b);
                        advance(op);
                        break;
                    }
                case 0x51:
                    {
                        op_bit(2, reg.c);
                        advance(op);
                        break;
                    }
                case 0x52:
                    {
                        op_bit(2, reg.d);
                        advance(op);
                        break;
                    }
                case 0x53:
                    {
                        op_bit(2, reg.e);
                        advance(op);
                        break;
                    }
                case 0x54:
                    {
                        op_bit(2, reg.h);
                        advance(op);
                        break;
                    }
                case 0x55:
                    {
                        op_bit(2, reg.l);
                        advance(op);
                        break;
                    }
                case 0x56:
                    {
                        op_bit(2, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x57:
                    {
                        op_bit(2, reg.a);
                        advance(op);
                        break;
                    }
                case 0x58:
                    {
                        op_bit(3, reg.b);
                        advance(op);
                        break;
                    }
                case 0x59:
                    {
                        op_bit(3, reg.c);
                        advance(op);
                        break;
                    }
                case 0x5a:
                    {
                        op_bit(3, reg.d);
                        advance(op);
                        break;
                    }
                case 0x5b:
                    {
                        op_bit(3, reg.e);
                        advance(op);
                        break;
                    }
                case 0x5c:
                    {
                        op_bit(3, reg.h);
                        advance(op);
                        break;
                    }
                case 0x5d:
                    {
                        op_bit(3, reg.l);
                        advance(op);
                        break;
                    }
                case 0x5e:
                    {
                        op_bit(3, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x5f:
                    {
                        op_bit(3, reg.a);
                        advance(op);
                        break;
                    }
                case 0x60:
                    {
                        op_bit(4, reg.b);
                        advance(op);
                        break;
                    }
                case 0x61:
                    {
                        op_bit(4, reg.c);
                        advance(op);
                        break;
                    }
                case 0x62:
                    {
                        op_bit(4, reg.d);
                        advance(op);
                        break;
                    }
                case 0x63:
                    {
                        op_bit(4, reg.e);
                        advance(op);
                        break;
                    }
                case 0x64:
                    {
                        op_bit(4, reg.h);
                        advance(op);
                        break;
                    }
                case 0x65:
                    {
                        op_bit(4, reg.l);
                        advance(op);
                        break;
                    }
                case 0x66:
                    {
                        op_bit(4, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x67:
                    {
                        op_bit(4, reg.a);
                        advance(op);
                        break;
                    }
                case 0x68:
                    {
                        op_bit(5, reg.b);
                        advance(op);
                        break;
                    }
                case 0x69:
                    {
                        op_bit(5, reg.c);
                        advance(op);
                        break;
                    }
                case 0x6a:
                    {
                        op_bit(5, reg.d);
                        advance(op);
                        break;
                    }
                case 0x6b:
                    {
                        op_bit(5, reg.e);
                        advance(op);
                        break;
                    }
                case 0x6c:
                    {
                        op_bit(5, reg.h);
                        advance(op);
                        break;
                    }
                case 0x6d:
                    {
                        op_bit(5, reg.l);
                        advance(op);
                        break;
                    }
                case 0x6e:
                    {
                        op_bit(5, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x6f:
                    {
                        op_bit(5, reg.a);
                        advance(op);
                        break;
                    }
                case 0x70:
                    {
                        op_bit(6, reg.b);
                        advance(op);
                        break;
                    }
                case 0x71:
                    {
                        op_bit(6, reg.c);
                        advance(op);
                        break;
                    }
                case 0x72:
                    {
                        op_bit(6, reg.d);
                        advance(op);
                        break;
                    }
                case 0x73:
                    {
                        op_bit(6, reg.e);
                        advance(op);
                        break;
                    }
                case 0x74:
                    {
                        op_bit(6, reg.h);
                        advance(op);
                        break;
                    }
                case 0x75:
                    {
                        op_bit(6, reg.l);
                        advance(op);
                        break;
                    }
                case 0x76:
                    {
                        op_bit(6, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x77:
                    {
                        op_bit(6, reg.a);
                        advance(op);
                        break;
                    }
                case 0x78:
                    {
                        op_bit(7, reg.b);
                        advance(op);
                        break;
                    }
                case 0x79:
                    {
                        op_bit(7, reg.c);
                        advance(op);
                        break;
                    }
                case 0x7a:
                    {
                        op_bit(7, reg.d);
                        advance(op);
                        break;
                    }
                case 0x7b:
                    {
                        op_bit(7, reg.e);
                        advance(op);
                        break;
                    }
                case 0x7c:
                    {
                        op_bit(7, reg.h);
                        advance(op);
                        break;
                    }
                case 0x7d:
                    {
                        op_bit(7, reg.l);
                        advance(op);
                        break;
                    }
                case 0x7e:
                    {
                        op_bit(7, mem.Rb(reg.hl), reg.hl);
                        advance(op);
                        break;
                    }
                case 0x7f:
                    {
                        op_bit(7, reg.a);
                        advance(op);
                        break;
                    }
                case 0x80:
                    {
                        reg.b = op_res(0, reg.b);
                        advance(op);
                        break;
                    }
                case 0x81:
                    {
                        reg.c = op_res(0, reg.c);
                        advance(op);
                        break;
                    }
                case 0x82:
                    {
                        reg.d = op_res(0, reg.d);
                        advance(op);
                        break;
                    }
                case 0x83:
                    {
                        reg.e = op_res(0, reg.e);
                        advance(op);
                        break;
                    }
                case 0x84:
                    {
                        reg.h = op_res(0, reg.h);
                        advance(op);
                        break;
                    }
                case 0x85:
                    {
                        reg.l = op_res(0, reg.l);
                        advance(op);
                        break;
                    }
                case 0x86:
                    {
                        mem.Wb(reg.hl, op_res(0, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x87:
                    {
                        reg.a = op_res(0, reg.a);
                        advance(op);
                        break;
                    }
                case 0x88:
                    {
                        reg.b = op_res(1, reg.b);
                        advance(op);
                        break;
                    }
                case 0x89:
                    {
                        reg.c = op_res(1, reg.c);
                        advance(op);
                        break;
                    }
                case 0x8a:
                    {
                        reg.d = op_res(1, reg.d);
                        advance(op);
                        break;
                    }
                case 0x8b:
                    {
                        reg.e = op_res(1, reg.e);
                        advance(op);
                        break;
                    }
                case 0x8c:
                    {
                        reg.h = op_res(1, reg.h);
                        advance(op);
                        break;
                    }
                case 0x8d:
                    {
                        reg.l = op_res(1, reg.l);
                        advance(op);
                        break;
                    }
                case 0x8e:
                    {
                        mem.Wb(reg.hl, op_res(1, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x8f:
                    {
                        reg.a = op_res(1, reg.a);
                        advance(op);
                        break;
                    }
                case 0x90:
                    {
                        reg.b = op_res(2, reg.b);
                        advance(op);
                        break;
                    }
                case 0x91:
                    {
                        reg.c = op_res(2, reg.c);
                        advance(op);
                        break;
                    }
                case 0x92:
                    {
                        reg.d = op_res(2, reg.d);
                        advance(op);
                        break;
                    }
                case 0x93:
                    {
                        reg.e = op_res(2, reg.e);
                        advance(op);
                        break;
                    }
                case 0x94:
                    {
                        reg.h = op_res(2, reg.h);
                        advance(op);
                        break;
                    }
                case 0x95:
                    {
                        reg.l = op_res(2, reg.l);
                        advance(op);
                        break;
                    }
                case 0x96:
                    {
                        mem.Wb(reg.hl, op_res(2, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x97:
                    {
                        reg.a = op_res(2, reg.a);
                        advance(op);
                        break;
                    }
                case 0x98:
                    {
                        reg.b = op_res(3, reg.b);
                        advance(op);
                        break;
                    }
                case 0x99:
                    {
                        reg.c = op_res(3, reg.c);
                        advance(op);
                        break;
                    }
                case 0x9a:
                    {
                        reg.d = op_res(3, reg.d);
                        advance(op);
                        break;
                    }
                case 0x9b:
                    {
                        reg.e = op_res(3, reg.e);
                        advance(op);
                        break;
                    }
                case 0x9c:
                    {
                        reg.h = op_res(3, reg.h);
                        advance(op);
                        break;
                    }
                case 0x9d:
                    {
                        reg.l = op_res(3, reg.l);
                        advance(op);
                        break;
                    }
                case 0x9e:
                    {
                        mem.Wb(reg.hl, op_res(3, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0x9f:
                    {
                        reg.a = op_res(3, reg.a);
                        advance(op);
                        break;
                    }
                case 0xa0:
                    {
                        reg.b = op_res(4, reg.b);
                        advance(op);
                        break;
                    }
                case 0xa1:
                    {
                        reg.c = op_res(4, reg.c);
                        advance(op);
                        break;
                    }
                case 0xa2:
                    {
                        reg.d = op_res(4, reg.d);
                        advance(op);
                        break;
                    }
                case 0xa3:
                    {
                        reg.e = op_res(4, reg.e);
                        advance(op);
                        break;
                    }
                case 0xa4:
                    {
                        reg.h = op_res(4, reg.h);
                        advance(op);
                        break;
                    }
                case 0xa5:
                    {
                        reg.l = op_res(4, reg.l);
                        advance(op);
                        break;
                    }
                case 0xa6:
                    {
                        mem.Wb(reg.hl, op_res(4, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xa7:
                    {
                        reg.a = op_res(4, reg.a);
                        advance(op);
                        break;
                    }
                case 0xa8:
                    {
                        reg.b = op_res(5, reg.b);
                        advance(op);
                        break;
                    }
                case 0xa9:
                    {
                        reg.c = op_res(5, reg.c);
                        advance(op);
                        break;
                    }
                case 0xaa:
                    {
                        reg.d = op_res(5, reg.d);
                        advance(op);
                        break;
                    }
                case 0xab:
                    {
                        reg.e = op_res(5, reg.e);
                        advance(op);
                        break;
                    }
                case 0xac:
                    {
                        reg.h = op_res(5, reg.h);
                        advance(op);
                        break;
                    }
                case 0xad:
                    {
                        reg.l = op_res(5, reg.l);
                        advance(op);
                        break;
                    }
                case 0xae:
                    {
                        mem.Wb(reg.hl, op_res(5, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xaf:
                    {
                        reg.a = op_res(5, reg.a);
                        advance(op);
                        break;
                    }
                case 0xb0:
                    {
                        reg.b = op_res(6, reg.b);
                        advance(op);
                        break;
                    }
                case 0xb1:
                    {
                        reg.c = op_res(6, reg.c);
                        advance(op);
                        break;
                    }
                case 0xb2:
                    {
                        reg.d = op_res(6, reg.d);
                        advance(op);
                        break;
                    }
                case 0xb3:
                    {
                        reg.e = op_res(6, reg.e);
                        advance(op);
                        break;
                    }
                case 0xb4:
                    {
                        reg.h = op_res(6, reg.h);
                        advance(op);
                        break;
                    }
                case 0xb5:
                    {
                        reg.l = op_res(6, reg.l);
                        advance(op);
                        break;
                    }
                case 0xb6:
                    {
                        mem.Wb(reg.hl, op_res(6, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xb7:
                    {
                        reg.a = op_res(6, reg.a);
                        advance(op);
                        break;
                    }
                case 0xb8:
                    {
                        reg.b = op_res(7, reg.b);
                        advance(op);
                        break;
                    }
                case 0xb9:
                    {
                        reg.c = op_res(7, reg.c);
                        advance(op);
                        break;
                    }
                case 0xba:
                    {
                        reg.d = op_res(7, reg.d);
                        advance(op);
                        break;
                    }
                case 0xbb:
                    {
                        reg.e = op_res(7, reg.e);
                        advance(op);
                        break;
                    }
                case 0xbc:
                    {
                        reg.h = op_res(7, reg.h);
                        advance(op);
                        break;
                    }
                case 0xbd:
                    {
                        reg.l = op_res(7, reg.l);
                        advance(op);
                        break;
                    }
                case 0xbe:
                    {
                        mem.Wb(reg.hl, op_res(7, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xbf:
                    {
                        reg.a = op_res(7, reg.a);
                        advance(op);
                        break;
                    }
                case 0xc0:
                    {
                        reg.b = op_set(0, reg.b);
                        advance(op);
                        break;
                    }
                case 0xc1:
                    {
                        reg.c = op_set(0, reg.c);
                        advance(op);
                        break;
                    }
                case 0xc2:
                    {
                        reg.d = op_set(0, reg.d);
                        advance(op);
                        break;
                    }
                case 0xc3:
                    {
                        reg.e = op_set(0, reg.e);
                        advance(op);
                        break;
                    }
                case 0xc4:
                    {
                        reg.h = op_set(0, reg.h);
                        advance(op);
                        break;
                    }
                case 0xc5:
                    {
                        reg.l = op_set(0, reg.l);
                        advance(op);
                        break;
                    }
                case 0xc6:
                    {
                        mem.Wb(reg.hl, op_set(0, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xc7:
                    {
                        reg.a = op_set(0, reg.a);
                        advance(op);
                        break;
                    }
                case 0xc8:
                    {
                        reg.b = op_set(1, reg.b);
                        advance(op);
                        break;
                    }
                case 0xc9:
                    {
                        reg.c = op_set(1, reg.c);
                        advance(op);
                        break;
                    }
                case 0xca:
                    {
                        reg.d = op_set(1, reg.d);
                        advance(op);
                        break;
                    }
                case 0xcb:
                    {
                        reg.e = op_set(1, reg.e);
                        advance(op);
                        break;
                    }
                case 0xcc:
                    {
                        reg.h = op_set(1, reg.h);
                        advance(op);
                        break;
                    }
                case 0xcd:
                    {
                        reg.l = op_set(1, reg.l);
                        advance(op);
                        break;
                    }
                case 0xce:
                    {
                        mem.Wb(reg.hl, op_set(1, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xcf:
                    {
                        reg.a = op_set(1, reg.a);
                        advance(op);
                        break;
                    }
                case 0xd0:
                    {
                        reg.b = op_set(2, reg.b);
                        advance(op);
                        break;
                    }
                case 0xd1:
                    {
                        reg.c = op_set(2, reg.c);
                        advance(op);
                        break;
                    }
                case 0xd2:
                    {
                        reg.d = op_set(2, reg.d);
                        advance(op);
                        break;
                    }
                case 0xd3:
                    {
                        reg.e = op_set(2, reg.e);
                        advance(op);
                        break;
                    }
                case 0xd4:
                    {
                        reg.h = op_set(2, reg.h);
                        advance(op);
                        break;
                    }
                case 0xd5:
                    {
                        reg.l = op_set(2, reg.l);
                        advance(op);
                        break;
                    }
                case 0xd6:
                    {
                        mem.Wb(reg.hl, op_set(2, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xd7:
                    {
                        reg.a = op_set(2, reg.a);
                        advance(op);
                        break;
                    }
                case 0xd8:
                    {
                        reg.b = op_set(3, reg.b);
                        advance(op);
                        break;
                    }
                case 0xd9:
                    {
                        reg.c = op_set(3, reg.c);
                        advance(op);
                        break;
                    }
                case 0xda:
                    {
                        reg.d = op_set(3, reg.d);
                        advance(op);
                        break;
                    }
                case 0xdb:
                    {
                        reg.e = op_set(3, reg.e);
                        advance(op);
                        break;
                    }
                case 0xdc:
                    {
                        reg.h = op_set(3, reg.h);
                        advance(op);
                        break;
                    }
                case 0xdd:
                    {
                        reg.l = op_set(3, reg.l);
                        advance(op);
                        break;
                    }
                case 0xde:
                    {
                        mem.Wb(reg.hl, op_set(3, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xdf:
                    {
                        reg.a = op_set(3, reg.a);
                        advance(op);
                        break;
                    }
                case 0xe0:
                    {
                        reg.b = op_set(4, reg.b);
                        advance(op);
                        break;
                    }
                case 0xe1:
                    {
                        reg.c = op_set(4, reg.c);
                        advance(op);
                        break;
                    }
                case 0xe2:
                    {
                        reg.d = op_set(4, reg.d);
                        advance(op);
                        break;
                    }
                case 0xe3:
                    {
                        reg.e = op_set(4, reg.e);
                        advance(op);
                        break;
                    }
                case 0xe4:
                    {
                        reg.h = op_set(4, reg.h);
                        advance(op);
                        break;
                    }
                case 0xe5:
                    {
                        reg.l = op_set(4, reg.l);
                        advance(op);
                        break;
                    }
                case 0xe6:
                    {
                        mem.Wb(reg.hl, op_set(4, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xe7:
                    {
                        reg.a = op_set(4, reg.a);
                        advance(op);
                        break;
                    }
                case 0xe8:
                    {
                        reg.b = op_set(5, reg.b);
                        advance(op);
                        break;
                    }
                case 0xe9:
                    {
                        reg.c = op_set(5, reg.c);
                        advance(op);
                        break;
                    }
                case 0xea:
                    {
                        reg.d = op_set(5, reg.d);
                        advance(op);
                        break;
                    }
                case 0xeb:
                    {
                        reg.e = op_set(5, reg.e);
                        advance(op);
                        break;
                    }
                case 0xec:
                    {
                        reg.h = op_set(5, reg.h);
                        advance(op);
                        break;
                    }
                case 0xed:
                    {
                        reg.l = op_set(5, reg.l);
                        advance(op);
                        break;
                    }
                case 0xee:
                    {
                        mem.Wb(reg.hl, op_set(5, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xef:
                    {
                        reg.a = op_set(5, reg.a);
                        advance(op);
                        break;
                    }
                case 0xf0:
                    {
                        reg.b = op_set(6, reg.b);
                        advance(op);
                        break;
                    }
                case 0xf1:
                    {
                        reg.c = op_set(6, reg.c);
                        advance(op);
                        break;
                    }
                case 0xf2:
                    {
                        reg.d = op_set(6, reg.d);
                        advance(op);
                        break;
                    }
                case 0xf3:
                    {
                        reg.e = op_set(6, reg.e);
                        advance(op);
                        break;
                    }
                case 0xf4:
                    {
                        reg.h = op_set(6, reg.h);
                        advance(op);
                        break;
                    }
                case 0xf5:
                    {
                        reg.l = op_set(6, reg.l);
                        advance(op);
                        break;
                    }
                case 0xf6:
                    {
                        mem.Wb(reg.hl, op_set(6, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xf7:
                    {
                        reg.a = op_set(6, reg.a);
                        advance(op);
                        break;
                    }
                case 0xf8:
                    {
                        reg.b = op_set(7, reg.b);
                        advance(op);
                        break;
                    }
                case 0xf9:
                    {
                        reg.c = op_set(7, reg.c);
                        advance(op);
                        break;
                    }
                case 0xfa:
                    {
                        reg.d = op_set(7, reg.d);
                        advance(op);
                        break;
                    }
                case 0xfb:
                    {
                        reg.e = op_set(7, reg.e);
                        advance(op);
                        break;
                    }
                case 0xfc:
                    {
                        reg.h = op_set(7, reg.h);
                        advance(op);
                        break;
                    }
                case 0xfd:
                    {
                        reg.l = op_set(7, reg.l);
                        advance(op);
                        break;
                    }
                case 0xfe:
                    {
                        mem.Wb(reg.hl, op_set(7, mem.Rb(reg.hl)));
                        advance(op);
                        break;
                    }
                case 0xff:
                    {
                        reg.a = op_set(7, reg.a);
                        advance(op);
                        break;
                    }
                default:
                    {
                        //printf("%04X %02X %02X\n", reg.pc, op, b1);
                        state = cstate.debugging;
                        break;
                    }
            }
        }

        void exec_dd(int op)
        {
            byte b1 = mem.Rbd((ushort)(reg.pc + 1));

            switch (b1)
            {
                case 0x09:
                    {
                        reg.ix = op_add(reg.ix, reg.bc);
                        advance(op);
                        break;
                    }

                case 0x19:
                    {
                        reg.ix = op_add(reg.ix, reg.de);
                        advance(op);
                        break;
                    }

                case 0x21:
                    {
                        reg.ix = mem.Rw((ushort)(reg.pc + 2));
                        advance(op);
                        break;
                    }
                case 0x22:
                    {
                        mem.ww(mem.Rw((ushort)(reg.pc + 2)), reg.ix);
                        advance(op);
                        break;
                    }
                case 0x23:
                    {
                        reg.ix++;
                        advance(op);
                        break;
                    }
                case 0x24:
                    {
                        reg.ixh = op_inc8(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0x25:
                    {
                        reg.ixh = op_dec8(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0x26:
                    {
                        reg.ixh = mem.Rb((ushort)(reg.pc + 2));
                        advance(op);
                        break;
                    }

                case 0x29:
                    {
                        reg.ix = op_add(reg.ix, reg.ix);
                        advance(op);
                        break;
                    }
                case 0x2a:
                    {
                        reg.ix = mem.Rw(mem.Rw((ushort)(reg.pc + 2)));
                        advance(op);
                        break;
                    }
                case 0x2b:
                    {
                        reg.ix--;
                        advance(op);
                        break;
                    }
                case 0x2c:
                    {
                        reg.ixl = op_inc8(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0x2d:
                    {
                        reg.ixl = op_dec8(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0x2e:
                    {
                        reg.ixl = mem.Rb((ushort)(reg.pc + 2));
                        advance(op);
                        break;
                    }

                case 0x34:
                    {
                        op_inchl(reg.ix, reg.pc + 2);
                        advance(op);
                        break;
                    }
                case 0x35:
                    {
                        op_dechl(reg.ix, reg.pc + 2);
                        advance(op);
                        break;
                    }
                case 0x36:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, mem.Rb((ushort)(reg.pc + 3)));
                        advance(op);
                        break;
                    }

                case 0x39:
                    {
                        reg.ix = op_add(reg.ix, reg.sp);
                        advance(op);
                        break;
                    }

                case 0x40:
                case 0x41:
                case 0x42:
                    {
                        reg.pc += 2;
                        break;
                    }

                case 0x46:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rw((ushort)(reg.pc + 2)));
                        reg.b = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x4e:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        reg.c = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x56:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        reg.d = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x5e:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        reg.e = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x66:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        reg.h = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x6e:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        reg.l = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x70:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.b);
                        advance(op);
                        break;
                    }
                case 0x71:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.c);
                        advance(op);
                        break;
                    }
                case 0x72:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.d);
                        advance(op);
                        break;
                    }
                case 0x73:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.e);
                        advance(op);
                        break;
                    }
                case 0x74:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.h);
                        advance(op);
                        break;
                    }
                case 0x75:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.l);
                        advance(op);
                        break;
                    }

                case 0x77:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.a);
                        advance(op);
                        break;
                    }

                case 0x7e:
                    {
                        reg.wz = (ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)));
                        reg.a = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x84:
                    {
                        op_add8(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0x85:
                    {
                        op_add8(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0x86:
                    {
                        op_add8(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0x8c:
                    {
                        op_adc8(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0x8d:
                    {
                        op_adc8(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0x8e:
                    {
                        op_adc8(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0x94:
                    {
                        op_sub8(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0x95:
                    {
                        op_sub8(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0x96:
                    {
                        op_sub8(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0x9c:
                    {
                        op_sbc8(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0x9d:
                    {
                        op_sbc8(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0x9e:
                    {
                        op_sbc8(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xa4:
                    {
                        op_and(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0xa5:
                    {
                        op_and(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0xa6:
                    {
                        op_and(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xac:
                    {
                        op_xor(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0xad:
                    {
                        op_xor(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0xae:
                    {
                        op_xor(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xb4:
                    {
                        op_or(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0xb5:
                    {
                        op_or(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0xb6:
                    {
                        op_or(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xbc:
                    {
                        op_cp(reg.ixh);
                        advance(op);
                        break;
                    }
                case 0xbd:
                    {
                        op_cp(reg.ixl);
                        advance(op);
                        break;
                    }
                case 0xbe:
                    {
                        op_cp(mem.Rb((ushort)(reg.ix + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xcb:
                    {
                        exec_ddfd(op, b1, reg.ix);
                        break;
                    }



                case 0xe1:
                    {
                        reg.ix = op_pop();
                        advance(op);
                        break;
                    }

                case 0xe5:
                    {
                        op_push(reg.ix);
                        advance(op);
                        break;
                    }

                default:
                    {
                        //printf("%04X %02X %02X\n", reg.pc, op, b1);
                        state = cstate.debugging;
                        break;
                    }
            }
        }

        void exec_ed(int op)
        {
            byte b1 = mem.Rbd((ushort)(reg.pc + 1));

            switch (b1)
            {
                case 0x42:
                    {
                        reg.hl = op_sbc(reg.hl, reg.bc);
                        advance(op);
                        break;
                    }
                case 0x43:
                    {
                        mem.ww(mem.Rw((ushort)(reg.pc + 2)), reg.bc);
                        advance(op);
                        break;
                    }
                case 0x44:
                    {
                        op_neg();
                        advance(op);
                        break;
                    }

                case 0x47:
                    {
                        reg.i = reg.a;
                        advance(op);
                        break;
                    }

                case 0x4a:
                    {
                        reg.hl = op_adc(reg.hl, reg.bc);
                        advance(op);
                        break;
                    }
                case 0x4b:
                    {
                        reg.bc = mem.Rw(mem.Rw((ushort)(reg.pc + 2)));
                        advance(op);
                        break;
                    }

                case 0x52:
                    {
                        reg.hl = op_sbc(reg.hl, reg.de);
                        advance(op);
                        break;
                    }
                case 0x53:
                    {
                        mem.ww(mem.Rw((ushort)(reg.pc + 2)), reg.de);
                        advance(op);
                        break;
                    }

                case 0x5a:
                    {
                        reg.hl = op_adc(reg.hl, reg.de);
                        advance(op);
                        break;
                    }
                case 0x5b:
                    {
                        reg.de = mem.Rw(mem.Rw((ushort)(reg.pc + 2)));
                        advance(op);
                        break;
                    }

                case 0x5e:
                    {
                        im = 2;
                        advance(op);
                        break;
                    }

                case 0x62:
                    {
                        reg.hl = op_sbc(reg.hl, reg.hl);
                        advance(op);
                        break;
                    }

                case 0x67:
                    {
                        reg.wz = (ushort)(reg.hl + 1);
                        op_rrd();
                        advance(op);
                        break;
                    }

                case 0x6a:
                    {
                        reg.hl = op_adc(reg.hl, reg.hl);
                        advance(op);
                        break;
                    }

                case 0x6f:
                    {
                        reg.wz = (ushort)(reg.hl + 1);
                        op_rld();
                        advance(op);
                        break;
                    }

                case 0x72:
                    {
                        reg.hl = op_sbc(reg.hl, reg.sp);
                        advance(op);
                        break;
                    }
                case 0x73:
                    {
                        mem.ww(reg.wz = mem.Rw((ushort)(reg.pc + 2)), reg.sp);
                        reg.wz++;
                        advance(op);
                        break;
                    }

                case 0x7a:
                    {
                        reg.hl = op_adc(reg.hl, reg.sp);
                        advance(op);
                        break;
                    }
                case 0x7b:
                    {
                        reg.sp = mem.Rw(reg.wz = mem.Rw((ushort)(reg.pc + 2)));
                        reg.wz++;
                        advance(op);
                        break;
                    }

                case 0xa0:
                    {
                        op_ldi();
                        advance(op);
                        break;
                    }
                case 0xa1:
                    {
                        op_cpi();
                        advance(op);
                        break;
                    }

                case 0xa8:
                    {
                        op_ldd();
                        advance(op);
                        break;
                    }
                case 0xa9:
                    {
                        op_cpd();
                        advance(op);
                        break;
                    }

                case 0xb0:
                    {
                        op_ldir(op);
                        break;
                    }
                case 0xb1:
                    {
                        op_cpir(op);
                        break;
                    }

                case 0xb8:
                    {
                        op_lddr(op);
                        break;
                    }
                case 0xb9:
                    {
                        op_cpdr(op);
                        break;
                    }
                default:
                    {
                        //printf("%04X %02X %02X\n", reg.pc, op, b1);
                        state = cstate.debugging;
                        break;
                    }
            }
        }

        void exec_fd(int op)
        {
            byte b1 = mem.Rbd((ushort)(reg.pc + 1));

            switch (b1)
            {
                case 0x09:
                    {
                        reg.iy = op_add(reg.iy, reg.bc);
                        advance(op);
                        break;
                    }

                case 0x19:
                    {
                        reg.iy = op_add(reg.iy, reg.de);
                        advance(op);
                        break;
                    }

                case 0x21:
                    {
                        reg.iy = mem.Rw((ushort)(reg.pc + 2));
                        advance(op);
                        break;
                    }
                case 0x22:
                    {
                        mem.ww(mem.Rw((ushort)(reg.pc + 2)), reg.iy);
                        advance(op);
                        break;
                    }
                case 0x23:
                    {
                        reg.iy++;
                        advance(op);
                        break;
                    }

                case 0x26:
                    {
                        reg.iyh = mem.Rb((ushort)(reg.pc + 2));
                        advance(op);
                        break;
                    }

                case 0x29:
                    {
                        reg.iy = op_add(reg.iy, reg.iy);
                        advance(op);
                        break;
                    }
                case 0x2a:
                    {
                        reg.iy = mem.Rw(mem.Rw((ushort)(reg.pc + 2)));
                        advance(op);
                        break;
                    }
                case 0x2b:
                    {
                        reg.iy--;
                        advance(op);
                        break;
                    }

                case 0x2e:
                    {
                        reg.iyl = mem.Rb((ushort)(reg.pc + 2));
                        advance(op);
                        break;
                    }

                case 0x34:
                    {
                        op_inchl(reg.iy, reg.pc + 2);
                        advance(op);
                        break;
                    }
                case 0x35:
                    {
                        op_dechl(reg.ix, reg.pc + 2);
                        advance(op);
                        break;
                    }
                case 0x36:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, mem.Rb((ushort)(reg.pc + 3)));
                        advance(op);
                        break;
                    }

                case 0x39:
                    {
                        reg.iy = op_add(reg.iy, reg.sp);
                        advance(op);
                        break;
                    }

                case 0x40:
                case 0x41:
                    {
                        reg.pc += 2;
                        break;
                    }

                case 0x46:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.b = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x4e:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.c = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x56:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.d = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x5e:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.e = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x66:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.h = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x6e:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.l = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x70:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.b);
                        advance(op);
                        break;
                    }
                case 0x71:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.c);
                        advance(op);
                        break;
                    }
                case 0x72:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.d);
                        advance(op);
                        break;
                    }
                case 0x73:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.e);
                        advance(op);
                        break;
                    }
                case 0x74:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.h);
                        advance(op);
                        break;
                    }
                case 0x75:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.l);
                        advance(op);
                        break;
                    }

                case 0x77:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        mem.Wb(reg.wz, reg.a);
                        advance(op);
                        break;
                    }

                case 0x7e:
                    {
                        reg.wz = (ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)));
                        reg.a = mem.Rb(reg.wz);
                        advance(op);
                        break;
                    }

                case 0x84:
                    {
                        op_add8(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0x85:
                    {
                        op_add8(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0x86:
                    {
                        op_add8(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }


                case 0x8c:
                    {
                        op_adc8(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0x8d:
                    {
                        op_adc8(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0x8e:
                    {
                        op_adc8(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0x94:
                    {
                        op_sub8(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0x95:
                    {
                        op_sub8(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0x96:
                    {
                        op_sub8(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0x9c:
                    {
                        op_sbc8(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0x9d:
                    {
                        op_sbc8(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0x9e:
                    {
                        op_sbc8(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xa4:
                    {
                        op_and(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0xa5:
                    {
                        op_and(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0xa6:
                    {
                        op_and(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xac:
                    {
                        op_xor(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0xad:
                    {
                        op_xor(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0xae:
                    {
                        op_xor(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xb4:
                    {
                        op_or(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0xb5:
                    {
                        op_or(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0xb6:
                    {
                        op_or(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xbc:
                    {
                        op_cp(reg.iyh);
                        advance(op);
                        break;
                    }
                case 0xbd:
                    {
                        op_cp(reg.iyl);
                        advance(op);
                        break;
                    }
                case 0xbe:
                    {
                        op_cp(mem.Rb((ushort)(reg.iy + mem.Rb((ushort)(reg.pc + 2)))));
                        advance(op);
                        break;
                    }

                case 0xcb:
                    {
                        exec_ddfd(op, b1, reg.iy);
                        break;
                    }

                case 0xe1:
                    {
                        reg.iy = op_pop();
                        advance(op);
                        break;
                    }

                case 0xe5:
                    {
                        op_push(reg.iy);
                        advance(op);
                        break;
                    }
                default:
                    {
                        z80.tracer.open_close_log(false);
                        state = cstate.debugging;
                        break;
                    }
            }
        }

        void exec_ddfd(int op, int b1, int rx)
        {
            byte b3 = mem.Rbd((ushort)(reg.pc + 3));

            reg.wz = (ushort)(rx + mem.Rb((ushort)(reg.pc + 2)));

            switch (b3)
            {

                case 0x06:
                    {
                        mem.Wb(reg.wz, op_rlc(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x0e:
                    {
                        mem.Wb(reg.wz, op_rrc(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x16:
                    {
                        mem.Wb(reg.wz, op_rl(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x1e:
                    {
                        mem.Wb(reg.wz, op_rr(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x26:
                    {
                        mem.Wb(reg.wz, op_sla(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x2e:
                    {
                        mem.Wb(reg.wz, op_sra(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x36:
                    {
                        mem.Wb(reg.wz, op_sll(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x3e:
                    {
                        mem.Wb(reg.wz, op_srl(mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x46:
                    {
                        //reg.wz = reg + mem.rb(reg.pc + 2);
                        op_bit(0, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x4e:
                    {
                        op_bit(1, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x56:
                    {
                        op_bit(2, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x5e:
                    {
                        op_bit(3, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x66:
                    {
                        op_bit(4, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x6e:
                    {
                        op_bit(5, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x76:
                    {
                        op_bit(6, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x7e:
                    {
                        op_bit(7, mem.Rb(reg.wz), reg.wz);
                        advance(op);
                        break;
                    }

                case 0x86:
                    {
                        mem.Wb(reg.wz, op_res(0, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x8e:
                    {
                        mem.Wb(reg.wz, op_res(1, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x96:
                    {
                        mem.Wb(reg.wz, op_res(2, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0x9e:
                    {
                        mem.Wb(reg.wz, op_res(3, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xa6:
                    {
                        mem.Wb(reg.wz, op_res(4, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xae:
                    {
                        mem.Wb(reg.wz, op_res(5, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xb6:
                    {
                        mem.Wb(reg.wz, op_res(6, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xbe:
                    {
                        mem.Wb(reg.wz, op_res(7, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xc6:
                    {
                        mem.Wb(reg.wz, op_set(0, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xce:
                    {
                        mem.Wb(reg.wz, op_set(1, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xd6:
                    {
                        mem.Wb(reg.wz, op_set(2, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xde:
                    {
                        mem.Wb(reg.wz, op_set(3, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xe6:
                    {
                        mem.Wb(reg.wz, op_set(4, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xee:
                    {
                        mem.Wb(reg.wz, op_set(5, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xf6:
                    {
                        mem.Wb(reg.wz, op_set(6, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                case 0xfe:
                    {
                        mem.Wb(reg.wz, op_set(7, mem.Rb(reg.wz)));
                        advance(op);
                        break;
                    }

                default:
                    {
                        //printf("%04X %02X %02X %02X\n", reg.pc, op, b1, b3);
                        state = cstate.debugging;
                        break;
                    }
            }
        }

        void op_adc8(int r1)
        {
            int cf = ((byte)(reg.f & 1));
            int v = reg.a + r1 + cf;

            set_flag(v > 0xff, FC);
            set_flag(0, FN);
            set_flag((~(reg.a ^ r1) & (reg.a ^ v) & 0x80), FP);
            set_flag(v & FX, FX);
            set_flag(((reg.a & 0xf) + (r1 & 0xf) + cf) & FH, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        ushort op_adc(int r1, int r2)
        {
            int c = reg.f & FC;
            int v = r1 + r2 + c;
            reg.wz = (ushort)(r1 + 1);

            set_flag(v > 0xffff, FC);
            set_flag(0, FN);
            set_flag((~(r1 ^ r2) & (r1 ^ v) & 0x8000), FP);
            set_flag((v >> 11) & 1, FX);
            set_flag((((r1 & 0xfff) + (r2 & 0xfff) + c)) & 0x1000, FH);
            set_flag((v >> 13) & 1, FY);
            set_flag((v & 0xffff) == 0, FZ);
            set_flag(v & 0x8000, FS);

            return (ushort)v;
        }

        void op_add8(int r1)
        {
            int v = reg.a + r1;

            set_flag(v > 0xff, FC);
            set_flag(0, FN);
            set_flag((~(reg.a ^ r1) & (reg.a ^ v) & 0x80), FP);
            set_flag(v & FX, FX);
            set_flag(((reg.a & 0xf) + (r1 & 0xf)) & FH, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        ushort op_add(int r1, int r2)
        {
            int v = r1 + r2;
            reg.wz = (ushort)(r1 + 1);

            set_flag(0, FN);
            set_flag(v > 0xffff, FC);
            set_flag((((r1 & 0xfff) + (r2 & 0xfff))) & 0x1000, FH);
            set_flag((v >> 11) & 1, FX);
            set_flag((v >> 13) & 1, FY);

            return (ushort)v;
        }

        void op_and(int r1)
        {
            int v = reg.a & r1;

            set_flag(0, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(1, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        void op_bit(int r1, int r2, int addr = -1)
        {
            int v = r2 & (1 << r1);

            if (addr > -1)
            {
                reg.wz = (ushort)addr;
                set_flag(reg.w & FX, FX);
                set_flag(reg.w & FY, FY);
            }
            else
            {
                set_flag(r2 & FX, FX);
                set_flag(r2 & FY, FY);
            }

            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(1, FH);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

        }

        void op_call(int op, bool flag)
        {
            if (flag)
            {
                op_push((ushort)(reg.pc + 3));
                reg.wz = reg.pc = mem.Rw((ushort)(reg.pc + 1));
                cycles += disasm_00[op].cycles;
            }
            else
            {
                reg.pc += 3;
                cycles += disasm_00[op].cycles2;
            }
        }

        void op_ccf()
        {

            int c = reg.f & FC;
            set_flag(c, FH);
            set_flag(~reg.f & FC, FC);
            set_flag(0, FN);
            set_flag(reg.a & FX, FX);
            set_flag(reg.a & FY, FY);
        }

        void op_cp(int r1)
        {
            int v = reg.a - r1;

            set_flag(v < 0, FC);
            set_flag(1, FN);
            set_flag(((reg.a ^ v) & (reg.a ^ r1) & 0x80), FP);
            set_flag((r1 >> 3) & 1, FX);
            set_flag(((reg.a & 0xf) - (r1 & 0xf) & 0x10), FH);
            set_flag((r1 >> 5) & 1, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);
        }

        void op_cpd()
        {
            int v = reg.a - mem.Rb(reg.hl);

            set_flag(1, FN);
            set_flag(reg.bc - 1, FP);
            set_flag(((v & 0xf) > (reg.a & 0xf)), FH);
            if ((reg.f & FH) > 0) v--;
            set_flag(v & FX, FX);
            set_flag(v & 0x02, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.bc--;
            reg.hl--;
            reg.wz--;
        }

        void op_cpdr(int op)
        {
            op_cpd();

            if (reg.bc == 0 || ((reg.f & FZ) > 0))
            {
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles2;
                add_pc(op);
            }
            else
            {
                reg.wz = (ushort)(reg.pc + 1);
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles;
            }

        }

        void op_cpi()
        {
            int v = reg.a - mem.Rb(reg.hl);

            set_flag(1, FN);
            set_flag(reg.bc - 1, FP);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(((v & 0xf) > (reg.a & 0xf)), FH);
            if ((reg.f & FH) > 0) v--;
            set_flag(v & FX, FX);
            set_flag(v & 0x02, FY);
            set_flag(v & 0x80, FS);

            reg.bc--;
            reg.hl++;
            reg.wz++;
        }

        void op_cpir(int op)
        {
            op_cpi();

            if (reg.bc == 0 || (reg.f & FZ) > 0)
            {
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles2;
                add_pc(op);
            }
            else
            {
                reg.wz = (ushort)(reg.pc + 1);
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles;
            }
        }

        void op_cpl()
        {
            int r1 = ~reg.a;

            set_flag(1, FN);
            set_flag((r1 >> 3) & 1, FX);
            set_flag(1, FH);
            set_flag((r1 >> 5) & 1, FY);

            reg.a = (byte)r1;
        }

        void op_daa()
        {
            int v = reg.a;

            if ((reg.f & FN) > 0)
            {
                if ((reg.f & FH) > 0 || (reg.a & 0xf) > 9)
                    v -= 6;
                if ((reg.f & FC) > 0 || (reg.a > 0x99))
                    v -= 0x60;
            }
            else
            {
                if ((reg.f & FH) > 0 || (reg.a & 0xf) > 9)
                    v += 6;
                if ((reg.f & FC) > 0 || (reg.a > 0x99))
                    v += 0x60;
            }

            set_flag((reg.f & FC) > 0 || reg.a > 0x99, FC);
            set_flag(get_parity((byte)v), FP);
            set_flag(((reg.a & 0x10) ^ (v & 0x10)) & FH, FH);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);
            set_flag(v & FX, FX);
            set_flag(v & FY, FY);

            reg.a = (byte)v;
        }

        byte op_dec8(int r1)
        {
            int o = r1;
            int v = r1 - 1;

            set_flag(1, FN);
            set_flag(o == 0x80, FP);
            set_flag(v & FX, FX);
            set_flag((v & 0x0f) == 0x0f, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)v;
        }

        void op_dechl(int r1, int r2)
        {
            ushort a = get_addr(r1, r2);
            mem.Wb(a, op_dec8(mem.Rb(a)));
        }

        void op_djnz(int r1)
        {
            if (--reg.b > 0)
            {
                reg.wz = reg.pc += (ushort)((sbyte)mem.Rb((ushort)(reg.pc + 1)) + 2);
                cycles += disasm_00[mem.Rb(reg.pc)].cycles;
            }
            else
            {
                reg.pc += 2;
                cycles += disasm_00[mem.Rb(reg.pc)].cycles2;
            }

        }

        int[] op_ex(int op, int op2)
        {
            int[] t = { op2, op };
            return t;
        }

        void op_exx()
        {
            int[] v = op_ex(reg.bc, reg.sbc);
            reg.bc = (ushort)v[0]; reg.sbc = (ushort)v[1];
            v = op_ex(reg.de, reg.sde);
            reg.de = (ushort)v[0]; reg.sde = (ushort)v[1];
            v = op_ex(reg.hl, reg.shl);
            reg.hl = (ushort)v[0]; reg.shl = (ushort)v[1];
        }

        void op_halt()
        {
            while (halt)
            {
                if (cycles < CYCLES_PER_FRAME)
                {
                    cycles += 4;
                }
                else
                {
                    int intaddr = reg.i << 8 | mem.ports[0];
                    reg.sp -= 2;
                    reg.pc = mem.Rw((ushort)intaddr);
                    halt = false;
                    inte = false;
                }
            }
        }

        byte op_inc8(int r1)
        {
            int o = r1;
            int v = r1 + 1;

            set_flag(0, FN);
            set_flag(o == 0x7f, FP);
            set_flag(v & FX, FX);
            set_flag((o & 0xf) == 0xf, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)v;
        }

        void op_inchl(int r1, int r2)
        {
            ushort a = get_addr(r1, r2);
            mem.Wb(a, op_inc8(mem.Rb(a)));
        }

        void op_jp(int op, bool flag)
        {
            reg.wz = mem.Rw((ushort)(reg.pc + 1));
            if (flag)
            {
                reg.pc = reg.wz;
                cycles += disasm_00[op].cycles;
            }
            else
            {
                reg.pc += 3;
                cycles += disasm_00[op].cycles2;
            }
        }

        bool op_jr(int op, bool flag)
        {
            if (flag)
            {
                reg.pc += (ushort)((sbyte)mem.Rb((ushort)(reg.pc + 1)) + 2);
                cycles += disasm_00[op].cycles;
                return true;
            }

            reg.pc += 2;
            cycles += disasm_00[op].cycles2;

            return false;
        }

        void op_ldd()
        {
            mem.Wb(reg.de, mem.Rb(reg.hl));

            set_flag(0, FN);
            set_flag(reg.bc - 1, FP);
            set_flag(0, FH);
            int xy = mem.Rb(reg.hl);
            set_flag((xy + reg.a) & FX, FX);
            set_flag((xy + reg.a) & FN, FY);

            reg.bc--;
            reg.hl--;
            reg.de--;
        }

        void op_lddr(int op)
        {
            op_ldd();

            if ((int)reg.bc == 0)
            {
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles2;
                add_pc(op);
            }
            else
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles;
        }

        void op_ldi()
        {
            mem.Wb(reg.de, mem.Rb(reg.hl));

            set_flag(0, FN);
            set_flag(reg.bc - 1, FP);
            set_flag(0, FH);
            int xy = mem.Rb(reg.hl);
            set_flag((xy + reg.a) & FX, FX);
            set_flag((xy + reg.a) & FN, FY);

            reg.bc--;
            reg.hl++;
            reg.de++;
        }

        void op_ldir(int op)
        {
            op_ldi();

            if ((int)reg.bc == 0)
            {
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles2;
                add_pc(op);
            }
            else
            {
                reg.wz = (ushort)(reg.pc + 1);
                cycles += disasm_ed[mem.Rb((ushort)(reg.pc + 1))].cycles;
            }
        }

        void op_neg()
        {
            int v = (byte)(0 - reg.a);

            set_flag(reg.a != 0, FC);
            set_flag(1, FN);
            set_flag(reg.a == 0x80, FP);
            set_flag(v & FX, FX);
            set_flag(((reg.a & 0xf) + (v & 0xf) & 0x10), FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        void op_or(int r1)
        {
            int v = reg.a | r1;

            set_flag(0, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        int op_pop8()
        {
            return mem.Rb(reg.sp++); ;
        }

        ushort op_pop()
        {
            int h = 0, l = 0;
            l = mem.Rb(reg.sp++);
            h = mem.Rb(reg.sp++);

            return (ushort)(h << 8 | l);
        }

        void op_push8(int r1)
        {
            mem.Wb(--reg.sp, (byte)r1);
        }

        void op_push(int r1)
        {
            mem.Wb(--reg.sp, (byte)(r1 >> 8));
            mem.Wb(--reg.sp, (byte)(r1 & 0xff));
        }

        void op_ret(int op, bool flag)
        {
            if (flag)
            {
                reg.pc = op_pop();
                cycles += disasm_00[op].cycles;
                reg.wz = reg.pc;
            }
            else
            {
                reg.pc++;
                cycles += disasm_00[op].cycles2;
            }
        }

        byte op_res(int r1, int r2)
        {
            return (byte)(r2 & ~(1 << r1));
        }

        byte op_rl(int r1)
        {
            int v = r1;
            int oc = (byte)(v >> 7);
            int c = (byte)(reg.f & 1);
            v <<= 1;

            set_flag(oc, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)(v | c)), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)(v | c);
        }

        void op_rla8()
        {
            int v = (ushort)(reg.a << 1);
            int oc = (byte)(reg.f & 1);
            int c = (byte)(v >> 8);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);

            reg.a = (byte)(v | oc);
        }

        byte op_rlc(int r1)
        {
            int c;
            int v = r1;
            c = (byte)(v >> 7);
            v = v << 1;

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)(v | c)), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)(v | c);
        }

        void op_rlca()
        {
            int v = (ushort)(reg.a << 1);
            int c = (byte)(v >> 8);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);

            reg.a = (byte)(v | c);
        }

        void op_rld()
        {
            int v = mem.Rb(reg.hl);

            mem.Wb(reg.hl, (byte)((v << 4) + ((reg.a & 0xf))));
            reg.a = (byte)((reg.a & 0xf0) + (v >> 4));

            set_flag(0, FN);
            set_flag(get_parity(reg.a), FP);
            set_flag(reg.a & FX, FX);
            set_flag(0, FH);
            set_flag(reg.a & FY, FY);
            set_flag(reg.a == 0, FZ);
            set_flag(reg.a & 0x80, FS);
        }

        byte op_rr(int r1)
        {
            int oc = (byte)(reg.f & 1);
            int c = (byte)(r1 & 1);
            int v = r1;
            v = (v >> 1) | (oc << 7);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)v;
        }

        byte op_rrc(int r1)
        {
            int v = r1;
            int c = (byte)(v & 1);
            v = (v >> 1) | (c << 7);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)v;
        }

        void op_rrca()
        {
            int c = (byte)(reg.a & 1);
            int v = reg.a = (byte)(reg.a >> 1);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);

            reg.a = (byte)(reg.a | (c << 7));
        }

        void op_rra()
        {
            int oc = (byte)(reg.f & 1);
            int c = (byte)(reg.a & 1);
            int v = (byte)(reg.a >> 1);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);

            reg.a = (byte)(v | (oc << 7));
        }

        void op_rrd()
        {
            int v = mem.Rb(reg.hl);

            mem.Wb(reg.hl, (byte)((v >> 4) + ((reg.a & 0xf) << 4)));
            reg.a = (byte)((reg.a & 0xf0) + (v & 0xf));

            set_flag(0, FN);
            set_flag(get_parity(reg.a), FP);
            set_flag(reg.a & FX, FX);
            set_flag(0, FH);
            set_flag(reg.a & FY, FY);
            set_flag(reg.a == 0, FZ);
            set_flag(reg.a & 0x80, FS);
        }

        void op_rst(ushort r1)
        {
            op_push((ushort)(reg.pc + 1));
            reg.pc = reg.wz = r1;
        }

        byte op_set(int r1, int r2)
        {
            return (byte)(r2 | (1 << r1));
        }

        void op_sbc8(int r1)
        {
            int cf = (byte)(reg.f & 1);
            int v = reg.a - r1 - cf;

            set_flag(v < 0, FC);
            set_flag(1, FN);
            set_flag(((reg.a ^ r1) & (reg.a ^ v) & 0x80), FP);
            set_flag(v & FX, FX);
            set_flag(((reg.a & 0xf) - (r1 & 0xf) - cf) & FH, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        ushort op_sbc(int r1, int r2)
        {
            int cf = (byte)(reg.f & 1);
            int v = r1 - r2 - cf;
            reg.wz = (ushort)(r1 + 1);

            set_flag(v < 0, FC);
            set_flag(1, FN);
            set_flag(((r1 ^ r2) & (r1 ^ v) & 0x8000), FP);
            set_flag((v >> 11) & 1, FX);
            set_flag((((r1 & 0xfff) - (r2 & 0xfff) - cf)) & 0x1000, FH);
            set_flag((v >> 13) & 1, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x8000, FS);

            return (ushort)v;
        }

        void op_scf()
        {
            set_flag(1, FC);
            set_flag(0, FN);
            set_flag(reg.a & FX, FX);
            set_flag(0, FH);
            set_flag(reg.a & FY, FY);
        }

        byte op_sla(int r1)
        {
            int v = r1;
            int c = (byte)(v >> 7);
            v <<= 1;

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)v;
        }

        byte op_sra(int r1)
        {
            int v = r1;
            int c = (byte)(v & 1);
            v = (v >> 1) | (v & 0x80);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(r1 & 0x80, FS);

            return (byte)v;
        }

        byte op_sll(int r1)
        {
            int c;
            int v = r1;
            c = (byte)(v >> 7);
            v = (v << 1) + 1;

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            return (byte)v;
        }

        byte op_srl(int r1)
        {
            int v = r1;
            int c = (byte)(r1 & 1);
            v = (v >> 1);
            //int oc = f & 1;
            //int c = r1 & 1;
            //int v = r1;
            //v = (v >> 1) | (oc << 7);

            set_flag(c, FC);
            set_flag(0, FN);
            set_flag(get_parity((byte)v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag((v & 0xff) == 0, FZ);
            set_flag(0, FS);

            return (byte)v;
        }

        void op_sub8(int r1)
        {
            int o = reg.a;
            int v = reg.a - r1;

            set_flag(v < 0, FC);
            set_flag(1, FN);
            set_flag(((reg.a ^ r1) & (reg.a ^ v) & 0x80), FP);
            set_flag(v & FX, FX);
            set_flag(((reg.a & 0xf) - (r1 & 0xf)) & FH, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        void op_xor(int r1)
        {
            int v = reg.a ^ r1;

            set_flag(0, FC);
            set_flag(0, FN);
            set_flag(get_parity(v), FP);
            set_flag(v & FX, FX);
            set_flag(0, FH);
            set_flag(v & FY, FY);
            set_flag(v == 0, FZ);
            set_flag(v & 0x80, FS);

            reg.a = (byte)v;
        }

        void handle_interrupts()
        {
            if (inte)
            {
                int intaddr = reg.i << 8 | mem.ports[0];
                op_push(reg.pc);
                reg.pc = mem.Rw((ushort)intaddr);
                halt = false;
                inte = false;
            }

            //cycles -= CYCLES_PER_FRAME;
        }

        void exchange_values(ref int r1, ref int r2)
        {
            int r, n;
            r = r1;
            n = r2;
            r1 = n;
            r2 = r;
        }

        void add_pc(int op)
        {
            byte b1 = mem.Rbd((ushort)(reg.pc + 1));
            byte b3 = mem.Rbd((ushort)(reg.pc + 3));

            if (op == 0xcb)
                reg.pc += disasm_cb[mem.Rbd((ushort)(reg.pc + 1))].size;
            else if (op == 0xdd)
            {
                if (b1 == 0xcb)
                    reg.pc += disasm_ddcb[b3].size;
                else
                    reg.pc += disasm_dd[b1].size;
            }
            else if (op == 0xed)
                reg.pc += disasm_ed[mem.Rbd((ushort)(reg.pc + 1))].size;
            else if (op == 0xfd)
            {
                if (b1 == 0xcb)
                    reg.pc += disasm_fdcb[b3].size;
                else
                    reg.pc += disasm_fd[b1].size;
            }
            else
                reg.pc += disasm_00[op].size;
        }

        void add_cyc(int op)
        {
            //int op = mem.rb(rgt.reg.pc + 0);
            byte b1 = mem.Rbd((ushort)(reg.pc + 1));
            byte b3 = mem.Rbd((ushort)(reg.pc + 3));

            if (op == 0xcb)
                cycles += disasm_cb[mem.Rbd((ushort)(reg.pc + 1))].cycles;
            else if (op == 0xdd)
            {
                if (b1 == 0xcb)
                    cycles += disasm_ddcb[b3].cycles;
                else
                    cycles += disasm_dd[b1].cycles;
            }
            else if (op == 0xed)
                cycles += disasm_ed[mem.Rbd((ushort)(reg.pc + 1))].cycles;
            else if (op == 0xfd)
            {
                if (b1 == 0xcb)
                    cycles += disasm_ddcb[b3].cycles;
                else
                    cycles += disasm_fd[b1].cycles;
            }
            else
                cycles += disasm_00[op].cycles;
        }

        void set_flag(int flag, int v)
        {
            if (flag > 0)
                reg.f |= (byte)v;
            else
                reg.f = (byte)(reg.f & ~v);
        }

        void set_flag(bool flag, int v)
        {
            if (flag)
                reg.f |= (byte)v;
            else
                reg.f = (byte)(reg.f & ~v);
        }

        bool get_parity(int op)
        {
            int bits = 0;
            for (int i = 0; i < 8; i++)
            {
                bits += (byte)(op & 1);
                op >>= 1;
            }
            return (bits % 2) == 0;
        }

        ushort get_addr(int a1, int a2)
        {
            return (ushort)(a1 + mem.Rb((ushort)a2));
        }

        void advance(int op)
        {
            add_cyc(op);
            add_pc(op);
        }

        public void reset()
        {
            reg.af = 0x0000; reg.bc = 0x0000;
            reg.de = 0x0000; reg.hl = 0x0000;
            reg.saf = 0x0000; reg.sbc = 0x0000;
            reg.sde = 0x0000; reg.shl = 0x0000;
            reg.ix = 0x0000; reg.iy = 0x0000;
            reg.wz = 0x0000; reg.sp = 0x0000;
            reg.r = 0x00;
            reg.i = 0x00;
            im = 0; iff1 = false; iff2 = false; halt = false;
            reg.pc = 0x0000;

            im = 0;
            iff1 = false;
            iff2 = false;
            halt = false;

            reg.ix = 0xffff;
            reg.iy = 0xffff;

            waddr = -1;
            raddr = -1;

            cycles = 0;

            mem.Reset();
        }

    }
}
