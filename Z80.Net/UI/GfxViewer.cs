using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z80.Net.Core;

namespace Z80.Net.UI
{
    public partial class GfxViewer : Form
    {
        Bitmap bmpTiles, bmpSprites;
        PacMachine z80;
        int[] gfx;
        int[] spr;

        public GfxViewer(PacMachine z80)
        {
            InitializeComponent();

            this.z80 = z80;

            DrawGraphics();
        }

        private void DrawGraphics()
        {
            int x = 0;
            int y = 0;

            bmpTiles = new Bitmap(128, 128);
            bmpSprites = new Bitmap(128, 128);
            BitmapData bmpData = null, bmpData2 = null;
            IntPtr ptr = IntPtr.Zero, ptr2 = IntPtr.Zero;

            bmpData = LockBits(ref ptr, ref gfx, bmpTiles);

            for (int i = 0; i < z80.mem.tiles.Length / 16; i++)
            {
                z80.gpu.draw_tile_viewer(ref gfx, z80.gpu.get_tile_ids(), x * 8, y * 8, i, 1);

                x++;

                if (x % 16 == 0)
                {
                    y++;
                    x = 0;
                }
            }

            UnlockBits(ref ptr, ref gfx, ref bmpTiles, bmpData);

            picTiles.Image = bmpTiles;
            Bitmap newbmp = (Bitmap)picTiles.Image;
            newbmp = new Bitmap(bmpTiles.Width * 2, bmpTiles.Height * 2);

            using (Graphics g = Graphics.FromImage(newbmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmpTiles, new Rectangle(Point.Empty, newbmp.Size));

                for (x = 0; x < 16; x++)
                {
                    for (y = 0; y < 16; y++)
                    {
                        g.DrawRectangle(Pens.White, x * 16, y * 16, 16, 16);
                    }
                }
            }

            picTiles.Image = newbmp;

            bmpData2 = LockBits(ref ptr2, ref spr, bmpSprites);

            x = 0;
            y = 0;
            for (int i = 0; i < z80.mem.sprites.Length / 64; i++)
            {
                z80.gpu.draw_sprite_viewer(ref spr, x * 16, y * 16, i);
                x++;

                if (x % 8 == 0)
                {
                    y++;
                    x = 0;
                }
            }

            UnlockBits(ref ptr2, ref spr, ref bmpSprites, bmpData2);

            picSprites.Image = bmpSprites;
            Bitmap newbmp2 = (Bitmap)picSprites.Image;
            newbmp2 = new Bitmap(bmpSprites.Width * 2, bmpSprites.Height * 2);

            Pen pen = new Pen(Color.White);

            using (Graphics g = Graphics.FromImage(newbmp2))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmpSprites, new Rectangle(Point.Empty, newbmp2.Size));

                for (x = 0; x < 8; x++)
                {
                    for (y = 0; y < 8; y++)
                    {
                        g.DrawRectangle(Pens.White, x * 32, y * 32, 32, 32);
                    }
                }
            }
            picSprites.Image = newbmp2;
        }

        private BitmapData LockBits(ref IntPtr ptr, ref int[] pixels, Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            ptr = bmpData.Scan0;
            pixels = new int[bmpData.Width * bmp.Height];
            Marshal.Copy(ptr, pixels, 0, pixels.Length);
            return bmpData;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DrawGraphics();
        }

        private void UnlockBits(ref IntPtr ptr, ref int[] pixels, ref Bitmap bmp, BitmapData bmpData)
        {
            Marshal.Copy(pixels, 0, ptr, pixels.Length);
            bmp.UnlockBits(bmpData);
        }
    }
}
