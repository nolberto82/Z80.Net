using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reg = Z80.Net.Core.Registers;

namespace Z80.Net.Core
{
    public class Memory
    {
        private const int TEST_START = 0x13a;

        public byte[] ram;

        public byte[] ports;
        public byte[] tiles;
        public byte[] sprites;
        public byte[] colors;
        public byte[] palettes;
        public byte[] sprite_data;
        public byte[] sound_data;

        public bool rom_loaded;

        public DipSwitches dip;

        public int this[int id]
        {
            get { return ports[id]; }
            set { ports[id] = (byte)value; }
        }
        private Machine z80;

        public Memory(Machine z80)
        {
            this.z80 = z80;

            ram = new byte[0x10000];
            tiles = new byte[0x1000];
            sprites = new byte[0x1000];
            colors = new byte[0x1000];
            palettes = new byte[0x1000];
            ports = new byte[0xa0];
            sprite_data = new byte[0x10];
            sound_data = new byte[0x200];

            dip = new DipSwitches(1, 2, 0, 1, 1);
        }

        public bool load_roms(string filename, int gameid)
        {
            switch (gameid)
            {
                case 0:
                    if (!load_file("roms/pacman.6e", ref ram, 0x0000))
                        return false;
                    if (!load_file("roms/pacman.6f", ref ram, 0x1000))
                        return false;
                    if (!load_file("roms/pacman.6h", ref ram, 0x2000))
                        return false;
                    if (!load_file("roms/pacman.6j", ref ram, 0x3000))
                        return false;
                    if (!load_file("roms/pacman.5e", ref tiles, 0x0000))
                        return false;
                    if (!load_file("roms/pacman.5f", ref sprites, 0x0000))
                        return false;
                    if (!load_file("roms/82s123.7f", ref colors, 0x0000))
                        return false;
                    if (!load_file("roms/82s126.4a", ref palettes, 0x0000))
                        return false;
                    if (!load_file("roms/82s126.1m", ref sound_data, 0x0000))
                        return false;
                    if (!load_file("roms/82s126.3m", ref sound_data, 0x0100))
                        return false;
                    break;
                case 1:
                    if (!load_file("roms/mspac/pacman.6e", ref ram, 0x0000))
                        return false;
                    if (!load_file("roms/mspac/pacman.6f", ref ram, 0x1000))
                        return false;
                    if (!load_file("roms/mspac/pacman.6h", ref ram, 0x2000))
                        return false;
                    if (!load_file("roms/mspac/pacman.6j", ref ram, 0x3000))
                        return false;
                    if (!load_file("roms/mspac/5e", ref tiles, 0x0000))
                        return false;
                    if (!load_file("roms/mspac/5f", ref sprites, 0x0000))
                        return false;
                    if (!load_file("roms/mspac/82s123.7f", ref colors, 0x0000))
                        return false;
                    if (!load_file("roms/mspac/82s126.4a", ref palettes, 0x0000))
                        return false;
                    if (!load_file("roms/mspac/u5", ref ram, 0x8000))
                        return false;
                    if (!load_file("roms/mspac/u6", ref ram, 0x9800))
                        return false;
                    if (!load_file("roms/mspac/u7", ref ram, 0x9000))
                        return false;
                    break;
            }


            Machine.test_running = false;
            return rom_loaded = true;
        }

        bool load_file(string filename, ref byte[] rom, int offset)
        {
            bool res = true;
            if (!File.Exists(filename))
                return false;
            byte[] data = File.ReadAllBytes(filename);
            Buffer.BlockCopy(data, 0, rom, offset, data.Length);

            return res;
        }

        public void reset()
        {
            reg.wz = 0x0000;
            for (int i = 0x4000; i < ram.Length; i++)
                ram[i] = 0x00;

            for (int i = 0x5000; i < 0x5080; i++)
                ram[i] = 0xff;

            for (int i = 0x5080; i < 0x50c0; i++)
                ram[i] = 0xc9;
        }

        private void reset_test()
        {
            reg.pc = 0x0100;
            reg.wz = 0x0000;
        }

        public byte rb(ushort addr)
        {
            z80.cpu.raddr = addr;

            if (addr == 0x5000)
            {
                int v = 0xff;

                for (int i = 0; i < z80.p1_keys.Length; i++)
                {
                    if (z80.p1_keys[i])
                    {
                        v = ~(Convert.ToByte(z80.p1_keys[i]) << i);
                        //z80.cpu.state = Cpu.cstate.debugging;
                    }
                }

                return (byte)v;
            }
            else if (addr == 0x5040)
            {
                int v = 0xff;

                for (int i = 0; i < z80.p2_keys.Length; i++)
                {
                    if (z80.p2_keys[i])
                    {
                        v = ~(Convert.ToByte(z80.p2_keys[i]) << i);
                        //z80.cpu.state = Cpu.cstate.debugging;
                    }
                }
                return (byte)v;
            }
            else if (addr >= 0x5080 && addr <= 0x50bf)
            {
                byte v = 0x0;

                v = (byte)(v | dip.coins);
                v = (byte)(v | dip.lives << 2);
                v = (byte)(v | dip.score << 4);
                v = (byte)(v | dip.difficulty << 6);
                v = (byte)(v | dip.ghosts_names << 7);
                return v;
            }

            return ram[addr];
        }

        public byte rbd(ushort addr)
        {
            return ram[addr];
        }

        public ushort rw(ushort addr)
        {

            //z80.cpu.raddr = addr;

            return (ushort)(rb((ushort)(addr + 1)) << 8 | rb(addr));
        }

        public void wb(ushort addr, byte v)
        {
            z80.cpu.waddr = addr;
            ram[addr] = v;
            z80.raddr.addr = (short)addr;
            z80.raddr.v = v;

            if (!Machine.test_running)
                z80.wsg.wb(addr, v);

            if (addr >= 0x5000 && addr < 0x6000)
            {
                if (addr == 0x5000)
                    z80.cpu.inte = v > 0;
                else if (addr >= 0x5060 && addr <= 0x506f)
                    sprite_data[addr & 0xf] = v;
                return;
            }
            else if (addr >= 0x8000)
            {
                ram[addr] = v;
                ram[addr & 0x4fff] = v;
            }

            if (Machine.test_running)
            {
                if (addr == 0xfffe)
                {
                    ram[addr - 1] = v;
                }
            }
        }

        public void ww(int addr, int v)
        {
            if (!Machine.test_running)
            {
                if (addr < 0x4000)
                    return;
                if (addr >= 0x5000)
                    return;
            }

            wb((ushort)(addr + 0), (byte)v);
            wb((ushort)(addr + 1), (byte)(v >> 8));

            z80.cpu.waddr = addr;
        }

        public bool load_test()
        {
            if (!load_file("Content/testfiles/interface.bin", ref ram, 0x0000))
                return false;
            if (!load_file("Content/testfiles/zexall.bin", ref ram, 0x0100))
                return false;

            //z80.cpu.reset();

            ram[0xfffd] = 0x00;
            ram[0xfffe] = 0x00;
            ram[0xffff] = 0x00;

            Machine.test_running = true;
            return true;
        }

        public void set_test_number(int test)
        {
            z80.cpu.reset();

            int offset = TEST_START + (test * 2);

            int l = ram[offset + 0];
            int h = ram[offset + 1];

            ram[TEST_START + 0] = (byte)l;
            ram[TEST_START + 1] = (byte)h;
            ram[TEST_START + 2] = 0;
            ram[TEST_START + 3] = 0;
        }

        public byte[] get_palettes()
        {
            return palettes;
        }
    }

    public class RamAddress
    {
        public short addr;
        public byte v;
    }

    public class DipSwitches
    {
        public byte coins;
        public byte lives;
        public byte score;
        public byte difficulty;
        public byte ghosts_names;

        public DipSwitches(byte coins, byte lives, byte score, byte difficulty, byte ghosts_names)
        {
            this.coins = coins;
            this.lives = lives;
            this.score = score;
            this.difficulty = difficulty;
            this.ghosts_names = ghosts_names;
        }
    }
}
