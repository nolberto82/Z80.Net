using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Forms.Controls;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using XAudioBuffer = SharpDX.XAudio2.AudioBuffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Z80.Net.Core;
using reg = Z80.Net.Core.Registers;

namespace Z80.Net
{

    internal class Display : MonoGameControl
    {
        private static Machine z80;
        private static bool rom_loaded;
        private int divide = 1;
        private const int CYCLES_PER_FRAME = 3072000 / 60;

        private Texture2D screen;
        private DynamicSoundEffectInstance audio;
        private KeyboardState ks;
        private GamePadState gs;

        public static void set_obj(Machine z)
        {
            z80 = z;
            rom_loaded = z.mem.rom_loaded;
        }

        protected override void Initialize()
        {
            base.Initialize();

            screen = new Texture2D(GraphicsDevice, 224, 288, false, SurfaceFormat.Color);
            audio = new DynamicSoundEffectInstance(48000, AudioChannels.Mono);

            audio.Play();
            z80.gpu.decode_graphics();

            Editor.ShowCursorPosition = false;
            Editor.ShowFPS = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            audio.Stop();
            audio.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ks = Keyboard.GetState();
            gs = GamePad.GetState(PlayerIndex.One);

            z80.p1_keys[0] = ks.IsKeyDown(Keys.Up) | gs.IsButtonDown(Buttons.DPadUp);
            z80.p1_keys[1] = ks.IsKeyDown(Keys.Left) | gs.IsButtonDown(Buttons.DPadLeft);
            z80.p1_keys[2] = ks.IsKeyDown(Keys.Right) | gs.IsButtonDown(Buttons.DPadRight);
            z80.p1_keys[3] = ks.IsKeyDown(Keys.Down) | gs.IsButtonDown(Buttons.DPadDown);
            z80.p2_keys[5] = ks.IsKeyDown(Keys.Enter) | gs.IsButtonDown(Buttons.Start);
            //z80.p2_keys[5] = z80.p1_keys[5];
            z80.p1_keys[7] = ks.IsKeyDown(Keys.Space) | gs.IsButtonDown(Buttons.Back);

            int max_cycles = CYCLES_PER_FRAME;
            if (Machine.test_running)
            {
                divide = z80.tracer.logging ? 10 : 1;
                max_cycles = 1000000;
            }

            if (rom_loaded)
            {
                if (z80.cpu.state == Cpu.cstate.running)
                {
                    while (z80.cpu.cycles < max_cycles)
                    {
                        if (z80.tracer.logging)
                            z80.tracer.log_to_file();

                        foreach (Breakpoint bp in Cpu.breakpoints)
                        {
                            if (bp.addr == reg.pc)
                            {
                                z80.cpu.state = Cpu.cstate.debugging;
                                Debugger.upd_dbg.Invoke();
                                return;
                            }

                            if (bp.addr == z80.cpu.waddr && bp.type == BPType.bp_write)
                            {
                                z80.cpu.waddr = -1;
                                z80.cpu.state = Cpu.cstate.debugging;
                                Debugger.upd_dbg();
                                return;
                            }

                            if (bp.addr == z80.cpu.raddr && bp.type == BPType.bp_read)
                            {
                                z80.cpu.raddr = -1;
                                z80.cpu.state = Cpu.cstate.debugging;
                                Debugger.upd_dbg();
                                return;
                            }
                        }

                        z80.cpu.step();

                        if (Machine.test_running)
                        {
                            if (z80.cpu.waddr == 0xfffe)
                            {
                                Debugger.upd_hex((ushort)z80.raddr.addr, z80.raddr.v);
                            }
                        }

                        //if (!Machine.test_running)
                        //{
                        if (Debugger.upd_hex != null)
                        {
                            if (z80.raddr.addr > -1)
                            {
                                Debugger.upd_hex((ushort)z80.raddr.addr, z80.raddr.v);
                                z80.raddr.addr = -1;
                            }
                        }
                        //}

                        if (Machine.test_running)
                            Debugger.upd_test_info();

                        if (z80.cpu.state == Cpu.cstate.debugging)
                            break;
                    }

                    z80.cpu.cycles -= max_cycles;
                }

                //audio.Volume = 2f;
                z80.gpu.render_screen();

                if (z80.wsg.sound_enabled && z80.cpu.state == Cpu.cstate.running)
                    play_sound();
            }
        }

        protected override void Draw()
        {
            base.Draw();

            GraphicsDevice.Textures[0] = null;
            GraphicsDevice.Clear(Color.Black);
            screen.SetData(z80.gpu.display_pix, 0, 224 * 288);
            Editor.Pixel = screen;
            Editor.spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            Editor.spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width * 2, screen.Height * 2), Color.White);

            Editor.spriteBatch.End();

            Editor.DrawDisplay();
        }

        private void play_sound()
        {
            z80.wsg.update();

            while (audio.PendingBufferCount < 15)
            {
                audio.SubmitBuffer(z80.wsg.samples);
                
            }
        }
    }
}
