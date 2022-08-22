using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80.Net.Core
{
    public static class Registers
    {
        public static ushort pc { get; set; }
        public static ushort sp { get; set; }

        public static byte i;
        public static byte r;

        public static byte a, f, b, c, d, e, h, l;
        public static byte ixh, ixl, iyh, iyl, w, z;
        public static byte sa, sf, sb, sc, sd, se, sh, sl;

        public static int sxy { get; set; }

        public static ushort af
        {
            get { return (ushort)(a << 8 | f); }
            set { a = (byte)(value >> 8); f = (byte)value; }
        }

        public static ushort bc
        {
            get { return (ushort)(b << 8 | c); }
            set { b = (byte)(value >> 8); c = (byte)value; }
        }

        public static ushort de
        {
            get { return (ushort)(d << 8 | e); }
            set { d = (byte)(value >> 8); e = (byte)value; }
        }

        public static ushort hl
        {
            get { return (ushort)(h << 8 | l); }
            set { h = (byte)(value >> 8); l = (byte)value; }
        }

        public static ushort ix
        {
            get { return (ushort)(ixh << 8 | ixl); }
            set { ixh = (byte)(value >> 8); ixl = (byte)value; }
        }

        public static ushort iy
        {
            get { return (ushort)(iyh << 8 | iyl); }
            set { iyh = (byte)(value >> 8); iyl = (byte)value; }
        }

        public static ushort wz
        {
            get { return (ushort)(w << 8 | z); }
            set { w = (byte)(value >> 8); z = (byte)value; }
        }

        public static ushort saf
        {
            get { return (ushort)(sa << 8 | sf); }
            set { sa = (byte)(value >> 8); sf = (byte)value; }
        }

        public static ushort sbc
        {
            get { return (ushort)(sb << 8 | sc); }
            set { sb = (byte)(value >> 8); sc = (byte)value; }
        }

        public static ushort sde
        {
            get { return (ushort)(sd << 8 | se); }
            set { sd = (byte)(value >> 8); se = (byte)value; }
        }

        public static ushort shl
        {
            get { return (ushort)(sh << 8 | sl); }
            set { sh = (byte)(value >> 8); sl = (byte)value; }
        }
    }
}
