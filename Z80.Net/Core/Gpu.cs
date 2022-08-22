using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Z80.Net.Core
{
    public class Gpu
    {
        public uint[] display_pix;
        uint[] tile_ids, sprite_ids;
        public uint[] tile;

        Memory mem;

        uint[,] tile16;

        private struct sprite_dec_t
        {
            public int x, y;

            public sprite_dec_t(int x, int y) : this()
            {
                this.x = x;
                this.y = y;
            }
        }
        sprite_dec_t[] spr_off;

        public Gpu(Machine z80)
        {
            mem = z80.mem;
            display_pix = new uint[224 * 288];
            tile_ids = new uint[128 * 128];
            sprite_ids = new uint[128 * 128];
            tile = new uint[8 * 4];
            tile16 = new uint[16, 16];

            spr_off = new sprite_dec_t[8];
            spr_off[0] = new sprite_dec_t(8, 12);
            spr_off[1] = new sprite_dec_t(8, 0);
            spr_off[2] = new sprite_dec_t(8, 4);
            spr_off[3] = new sprite_dec_t(8, 8);
            spr_off[4] = new sprite_dec_t(0, 12);
            spr_off[5] = new sprite_dec_t(0, 0);
            spr_off[6] = new sprite_dec_t(0, 4);
            spr_off[7] = new sprite_dec_t(0, 8);

            decode_graphics();
        }

        public void render_screen()
        {
            int i = 0x3dd;

            //top
            for (int y = 0; y <= 8; y += 8)
            {
                for (int x = 0; x <= 216; x += 8)
                {
                    int palid = mem.rb((ushort)(0x4400 + i)) & 0x3f;
                    int offset = mem.rb((ushort)(0x4000 + i)) * 64;

                    draw_tile(ref display_pix, x, y, offset, palid);
                    i--;
                }
                i += 0x3c;
            }

            i = 0x3a0;

            //middle
            for (int y = 16; y <= 264; y += 8)
            {
                for (int x = 0; x <= 216; x += 8)
                {
                    int palid = mem.rb((ushort)(0x4400 + i)) & 0x3f;
                    int offset = mem.rb((ushort)(0x4000 + i)) * 64;

                    draw_tile(ref display_pix, x, y, offset, palid);
                    i -= 0x20;
                }
                i = i + 0x380 + 1;
            }

            i = 0x1d;

            //bottom
            for (int y = 272; y < 288; y += 8)
            {
                for (int x = 0; x <= 216; x += 8)
                {
                    int palid = mem.rb((ushort)(0x4400 + i)) & 0x3f;
                    int offset = mem.rb((ushort)(0x4000 + i)) * 64;

                    draw_tile(ref display_pix, x, y, offset, palid);
                    i--;
                }
                i += 0x3c;
            }

            render_sprites();
        }

        void render_sprites()
        {
            for (int i = 7; i > 0; i--)
            {
                int sprite_id = mem.ram[0x4ff0 + i * 2] >> 2;
                bool sprite_flipx = (mem.ram[0x4ff0 + i * 2] & 0x02) > 0;
                bool sprite_flipy = (mem.ram[0x4ff0 + i * 2] & 0x01) > 0;
                byte sprite_pal = mem.ram[0x4ff0 + i * 2 + 1];
                int sprite_x = 224 - mem.sprite_data[i * 2] + 15;
                int sprite_y = 288 - mem.sprite_data[i * 2 + 1] - 16;

                if (sprite_x < 0)
                    continue;

                draw_sprites(sprite_x, sprite_y, sprite_id, sprite_pal, sprite_flipx, sprite_flipy);
            }

            //draw_sprites(0, 32, 0x24, 5, false, false);
        }

        void draw_sprites(int x, int y, int i, int palid, bool flipx, bool flipy)
        {
            int offset = i * 256;

            if (x < 0 || x >= 224)
                return;

            byte[] pl = new byte[4];
            pl[0] = mem.palettes[palid * 4 + 0];
            pl[1] = mem.palettes[palid * 4 + 1];
            pl[2] = mem.palettes[palid * 4 + 2];
            pl[3] = mem.palettes[palid * 4 + 3];

            for (int yy = 0; yy < 16; yy++)
            {
                for (int xx = 0; xx < 16; xx++)
                {
                    uint pix = sprite_ids[offset++];
                    byte pal = pl[pix];
                    byte col = mem.colors[pal];

                    if (col == 0)
                        continue;

                    int fx = flipx ? 15 - xx : xx;
                    int fy = flipy ? 15 - yy : yy;

                    if (x + fx < 0 || x + fx > 224)
                        continue;

                    uint c = get_palette(col);
                    int pos = (y + fy) * 224 + (x + fx);

                    display_pix[pos] = c;
                }
            }
        }

        void draw_tile(ref uint[] pixels, int x, int y, int offset, int palid)
        {
            uint[] temp = new uint[8 * 4];

            int[] pl = new int[4];
            pl[0] = mem.palettes[palid * 4 + 0];
            pl[1] = mem.palettes[palid * 4 + 1];
            pl[2] = mem.palettes[palid * 4 + 2];
            pl[3] = mem.palettes[palid * 4 + 3];

            for (int j = 0; j < 2; j++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    for (int xx = 0; xx < 8; xx++)
                    {
                        int pix = (int)tile_ids[offset++];
                        int pal = pl[pix];
                        int col = mem.colors[pal];

                        uint c = get_palette(col);

                        int pos = (y + (j * 4) + yy) * 224 + (x + xx);

                        pixels[pos] = c;
                        //pixels[pos] = 0xffffffff;

                        if (offset == 0x3f7 && j == 1)
                        {
                            temp[yy * 8 + xx] = c;
                        }
                    }
                }
            }
        }

        public void decode_graphics()
        {
            int x = 0;
            int y = 0;
            int pos = 0;

            for (int i = 0; i < mem.tiles.Length / 16; i++)
            {
                decode_tiles(x, y, i, ref pos);
                x++;

                if (x % 16 == 0)
                {
                    y++;
                    x = 0;
                }
            }

            x = 0;
            y = 0;
            pos = 0;

            for (int i = 0; i < mem.sprites.Length / 64; i++)
            {
                decode_sprites(i, ref pos);
            }

            //using (var writer = new BinaryWriter(File.Open("tilepix.bin", FileMode.Create)))
            //{
            //    Span<byte> bytes = MemoryMarshal.Cast<uint, byte>(tile_ids.AsSpan());
            //    writer.Write(bytes.ToArray());
            //}

            //using (var writer = new BinaryWriter(File.Open("spritepix.bin", FileMode.Create)))
            //{
            //    Span<byte> bytes = MemoryMarshal.Cast<uint, byte>(sprite_ids.AsSpan());
            //    writer.Write(bytes.ToArray());
            //}
        }

        void decode_sprites(int i, ref int pos)
        {
            int offset = i * 64;
            int basepos = i * 256;
            int[,] temp = new int[16, 16];

            for (int j = 0; j < 8; j++)
            {
                for (int xx = 7; xx >= 0; xx--)
                {
                    int tid = mem.sprites[offset++];

                    int h = 3;
                    int l = 0;

                    for (int yy = 3; yy >= 0; yy--)
                    {
                        int pix = ((tid >> h++)) & 2 | (tid >> l++) & 1;
                        //temp[yy + spr_off[j].y, xx + spr_off[j].x] = pix;

                        //if (pix > 0)
                        //    tile16[yy + spr_off[j].y, xx + spr_off[j].x] = 0xff0000ff;
                        //else
                        //    tile16[yy + spr_off[j].y, xx + spr_off[j].x] = 0;

                        int p = basepos + (yy + spr_off[j].y) * 16 + (xx + spr_off[j].x);
                        sprite_ids[p] = (byte)pix;
                    }
                }
            }
        }

        void decode_tiles(int x, int y, int i, ref int pos)
        {
            int offset = i * 16 + 15;
            uint[] temp;

            for (int j = 0; j < 2; j++)
            {
                temp = new uint[8 * 4];
                for (int xx = 7; xx >= 0; xx--)
                {
                    int tid = mem.tiles[offset - xx];

                    int h = 3, l = 0;

                    for (int yy = 3; yy >= 0; yy--)
                    {
                        uint pix = (uint)(((tid >> h++)) & 2 | (tid >> l++) & 1);
                        temp[yy * 8 + xx] = pix;
                    }
                }

                if (pos == 0)
                    Array.Copy(temp, 0, tile, pos, temp.Length);

                Array.Copy(temp, 0, tile_ids, pos, temp.Length);
                offset -= 8;
                pos += 32;
            }
        }

        uint get_palette(int col)
        {
            byte r = (byte)(((col >> 0) & 1) * 0x21 | ((col >> 1) & 1) * 0x47 | ((col >> 2) & 1) * 0x97);
            byte g = (byte)(((col >> 3) & 1) * 0x21 | ((col >> 4) & 1) * 0x47 | ((col >> 5) & 1) * 0x97);
            byte b = (byte)(((col >> 6) & 1) * 0x51 | ((col >> 7) & 1) * 0xae);
            return (uint)(0xff000000 | ((b << 16) | g << 8 | r));
        }
    }
}
