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
        public byte[] rom_aux;

        public byte[] ports;
        public byte[] tiles;
        public byte[] sprites;
        public byte[] colors;
        public byte[] palettes;
        public byte[] sprite_data;
        public byte[] sound_data;

        public bool rom_loaded;

        public DipSwitches dip;

        private bool AuxBoardEnabled;

        public int this[int id]
        {
            get { return ports[id]; }
            set { ports[id] = (byte)value; }
        }
        private PacMachine z80;

        public Memory(PacMachine z80)
        {
            this.z80 = z80;

            ram = new byte[0x10000];
            rom_aux = new byte[0x6800];

            tiles = new byte[0x1000];
            sprites = new byte[0x1000];
            colors = new byte[0x1000];
            palettes = new byte[0x1000];
            ports = new byte[0xa0];
            sprite_data = new byte[0x10];
            sound_data = new byte[0x200];

            dip = new DipSwitches(1, 2, 0, 1, 1);
        }

        public bool LoadRoms(string filename, int gameid)
        {
            if (!LoadFile("roms/mspac/82s123.7f", ref colors, 0x0000))
                return false;
            if (!LoadFile("roms/mspac/82s126.4a", ref palettes, 0x0000))
                return false;

            switch (gameid)
            {
                case 0:
                    if (!LoadFile("roms/pacman.6e", ref ram, 0x0000))
                        return false;
                    if (!LoadFile("roms/pacman.6f", ref ram, 0x1000))
                        return false;
                    if (!LoadFile("roms/pacman.6h", ref ram, 0x2000))
                        return false;
                    if (!LoadFile("roms/pacman.6j", ref ram, 0x3000))
                        return false;
                    if (!LoadFile("roms/pacman.5e", ref tiles, 0x0000))
                        return false;
                    if (!LoadFile("roms/pacman.5f", ref sprites, 0x0000))
                        return false;
                    if (!LoadFile("roms/82s126.1m", ref sound_data, 0x0000))
                        return false;
                    if (!LoadFile("roms/82s126.3m", ref sound_data, 0x0100))
                        return false;
                    break;
                case 1:
                    if (!LoadFile("roms/mspac/pacman.6e", ref ram, 0x0000))
                        return false;
                    if (!LoadFile("roms/mspac/pacman.6f", ref ram, 0x1000))
                        return false;
                    if (!LoadFile("roms/mspac/pacman.6h", ref ram, 0x2000))
                        return false;
                    if (!LoadFile("roms/mspac/pacman.6j", ref ram, 0x3000))
                        return false;
                    if (!LoadFile("roms/mspac/5e", ref tiles, 0x0000))
                        return false;
                    if (!LoadFile("roms/mspac/5f", ref sprites, 0x0000))
                        return false;

                    LoadAuxRoms();
                    break;
            }

            PacMachine.test_running = false;
            return rom_loaded = true;
        }

        bool LoadFile(string filename, ref byte[] rom, int offset)
        {
            bool res = true;
            if (!File.Exists(filename))
                return false;
            byte[] data = File.ReadAllBytes(filename);
            Buffer.BlockCopy(data, 0, rom, offset, data.Length);

            return res;
        }

        public void Reset()
        {
            reg.wz = 0x0000;
            for (int i = 0x4000; i < ram.Length; i++)
                ram[i] = 0x00;

            for (int i = 0x5000; i < 0x5080; i++)
                ram[i] = 0xff;

            for (int i = 0x5080; i < 0x50c0; i++)
                ram[i] = 0xc9;
        }

        private void ResetTest()
        {
            reg.pc = 0x0100;
            reg.wz = 0x0000;
        }

        public byte Rb(ushort addr)
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

            if (AuxBoardEnabled)
            {
                if (addr < 0x4000)
                    return rom_aux[addr];
            }

            return ram[addr];
        }

        public byte Rbd(ushort addr)
        {
            return ram[addr];
        }

        public ushort Rw(ushort addr)
        {
            return (ushort)(Rb((ushort)(addr + 1)) << 8 | Rb(addr));
        }

        public void Wb(ushort addr, byte v)
        {
            z80.cpu.waddr = addr;
            ram[addr] = v;
            z80.raddr.addr = (short)addr;
            z80.raddr.v = v;

            if (!PacMachine.test_running)
                z80.wsg.wb(addr, v);

            if (addr >= 0x5000 && addr < 0x6000)
            {
                if (addr == 0x5000)
                    z80.cpu.inte = v > 0;
                else if (addr == 0x5002)
                    AuxBoardEnabled = false;
                else if (addr >= 0x5060 && addr <= 0x506f)
                    sprite_data[addr & 0xf] = v;
                return;
            }
            else if (addr >= 0x8000)
            {
                ram[addr] = v;
                ram[addr & 0x4fff] = v;
            }

            if (PacMachine.test_running)
            {
                if (addr == 0xfffe)
                {
                    ram[addr - 1] = v;
                }
            }
        }

        public void ww(int addr, int v)
        {
            if (!PacMachine.test_running)
            {
                if (addr < 0x4000)
                    return;
                if (addr >= 0x5000)
                    return;
            }

            Wb((ushort)(addr + 0), (byte)v);
            Wb((ushort)(addr + 1), (byte)(v >> 8));

            z80.cpu.waddr = addr;
        }

        public bool load_test()
        {
            if (!LoadFile("Content/testfiles/interface.bin", ref ram, 0x0000))
                return false;
            if (!LoadFile("Content/testfiles/zexall.bin", ref ram, 0x0100))
                return false;

            //z80.cpu.reset();

            ram[0xfffd] = 0x00;
            ram[0xfffe] = 0x00;
            ram[0xffff] = 0x00;

            PacMachine.test_running = true;
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

        //Decryption taken from MAME and PIE
        private bool LoadAuxRoms()
        {
            byte[] u5 = new byte[0x0800];
            byte[] u6 = new byte[0x1000];
            byte[] u7 = new byte[0x1000];

            if (!LoadFile("roms/mspac/u5", ref u5, 0x0000))
                return false;
            if (!LoadFile("roms/mspac/u6", ref u6, 0x0000))
                return false;
            if (!LoadFile("roms/mspac/u7", ref u7, 0x0000))
                return false;

            // Decrypt aux ROMs
            for (int i = 0; i < 0x1000; i++)
            {
                rom_aux[DecryptA1(i) + 0x4000] = DecryptD(u7[i]);
                rom_aux[DecryptA1(i) + 0x5000] = DecryptD(u6[i]);
            }

            for (int i = 0; i < 0x0800; i++)
            {
                rom_aux[DecryptA2(i) + 0x6000] = DecryptD(u5[i]);
            }

            // Copy original ROM, but replace 6J with U7
            Array.Copy(ram, 0x0000, rom_aux, 0x0000, 0x1000);
            Array.Copy(ram, 0x1000, rom_aux, 0x1000, 0x1000);
            Array.Copy(ram, 0x2000, rom_aux, 0x2000, 0x1000);
            Array.Copy(rom_aux, 0x3000, rom_aux, 0x4000, 0x1000);

            // Apply ROM patches (from scattered U5 locations)
            for (int i = 0; i < 8; i++)
            {
                rom_aux[0x0410 + i] = rom_aux[0x6008 + i];
                rom_aux[0x08E0 + i] = rom_aux[0x61D8 + i];
                rom_aux[0x0A30 + i] = rom_aux[0x6118 + i];
                rom_aux[0x0BD0 + i] = rom_aux[0x60D8 + i];
                rom_aux[0x0C20 + i] = rom_aux[0x6120 + i];
                rom_aux[0x0E58 + i] = rom_aux[0x6168 + i];
                rom_aux[0x0EA8 + i] = rom_aux[0x6198 + i];

                rom_aux[0x1000 + i] = rom_aux[0x6020 + i];
                rom_aux[0x1008 + i] = rom_aux[0x6010 + i];
                rom_aux[0x1288 + i] = rom_aux[0x6098 + i];
                rom_aux[0x1348 + i] = rom_aux[0x6048 + i];
                rom_aux[0x1688 + i] = rom_aux[0x6088 + i];
                rom_aux[0x16B0 + i] = rom_aux[0x6188 + i];
                rom_aux[0x16D8 + i] = rom_aux[0x60C8 + i];
                rom_aux[0x16F8 + i] = rom_aux[0x61C8 + i];
                rom_aux[0x19A8 + i] = rom_aux[0x60A8 + i];
                rom_aux[0x19B8 + i] = rom_aux[0x61A8 + i];

                rom_aux[0x2060 + i] = rom_aux[0x6148 + i];
                rom_aux[0x2108 + i] = rom_aux[0x6018 + i];
                rom_aux[0x21A0 + i] = rom_aux[0x61A0 + i];
                rom_aux[0x2298 + i] = rom_aux[0x60A0 + i];
                rom_aux[0x23E0 + i] = rom_aux[0x60E8 + i];
                rom_aux[0x2418 + i] = rom_aux[0x6000 + i];
                rom_aux[0x2448 + i] = rom_aux[0x6058 + i];
                rom_aux[0x2470 + i] = rom_aux[0x6140 + i];
                rom_aux[0x2488 + i] = rom_aux[0x6080 + i];
                rom_aux[0x24B0 + i] = rom_aux[0x6180 + i];
                rom_aux[0x24D8 + i] = rom_aux[0x60C0 + i];
                rom_aux[0x24F8 + i] = rom_aux[0x61C0 + i];
                rom_aux[0x2748 + i] = rom_aux[0x6050 + i];
                rom_aux[0x2780 + i] = rom_aux[0x6090 + i];
                rom_aux[0x27B8 + i] = rom_aux[0x6190 + i];
                rom_aux[0x2800 + i] = rom_aux[0x6028 + i];
                rom_aux[0x2B20 + i] = rom_aux[0x6100 + i];
                rom_aux[0x2B30 + i] = rom_aux[0x6110 + i];
                rom_aux[0x2BF0 + i] = rom_aux[0x61D0 + i];
                rom_aux[0x2CC0 + i] = rom_aux[0x60D0 + i];
                rom_aux[0x2CD8 + i] = rom_aux[0x60E0 + i];
                rom_aux[0x2CF0 + i] = rom_aux[0x61E0 + i];
                rom_aux[0x2D60 + i] = rom_aux[0x6160 + i];
            }

            return true;
        }

        //Decryption taken from MAME and PIE
        private byte DecryptD(byte d)
        {
            int v = 0;

            v = (d & 0xC0) >> 3;
            v |= (d & 0x10) << 2;
            v |= (d & 0x0E) >> 1;
            v |= (d & 0x01) << 7;
            v |= (d & 0x20);

            return (byte)v;
        }

        //Decryption taken from MAME and PIE
        private ushort DecryptA1(int d)
        {
            int v = 0;

            v = (d & 0x807);
            v |= (d & 0x400) >> 7;
            v |= (d & 0x200) >> 2;
            v |= (d & 0x080) << 3;
            v |= (d & 0x040) << 2;
            v |= (d & 0x138) << 1;

            return (ushort)v;
        }

        //Decryption taken from MAME and PIE
        private ushort DecryptA2(int d)
        {
            int v = 0;

            v = (d & 0x807);
            v |= (d & 0x040) << 4;
            v |= (d & 0x100) >> 3;
            v |= (d & 0x080) << 2;
            v |= (d & 0x600) >> 2;
            v |= (d & 0x028) << 1;
            v |= (d & 0x010) >> 1;

            return (ushort)v;
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
