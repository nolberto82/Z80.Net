using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80.Net.Core
{
    public class Wsg
    {
        private const int SAMPLES_PER_FRAME = 3072000 / 32;

        private Voices voice1, voice2, voice3;

        public byte[] samples;

        public bool sound_enabled;

        private PacMachine z80;

        private int accu1, accu2, accu3;

        public const int sample_length = 1600;

        public Wsg(PacMachine z80)
        {
            this.z80 = z80;

            voice1 = new Voices(0, new byte[5], 0, new byte[5]);
            voice2 = new Voices(0, new byte[4], 0, new byte[4]);
            voice3 = new Voices(0, new byte[4], 0, new byte[4]);

            samples = new byte[sample_length];
        }

        public void wb(ushort addr, byte v)
        {
            if (addr == 0x5001)
                sound_enabled = v > 0;

            if (addr == 0x5045)
                voice1.waveform = v;
            else if (addr == 0x504a)
                voice2.waveform = v;
            else if (addr == 0x504f)
                voice3.waveform = v;

            if (addr == 0x5055)
                voice1.volume = v;
            else if (addr == 0x505a)
                voice2.volume = v;
            else if (addr == 0x505f)
                voice3.volume = v;

            //voice 1
            else if (addr == 0x5050)
                voice1.freq[0] = v;
            else if (addr == 0x5051)
                voice1.freq[1] = v;
            else if (addr == 0x5052)
                voice1.freq[2] = v;
            else if (addr == 0x5053)
                voice1.freq[3] = v;
            else if (addr == 0x5054)
                voice1.freq[4] = v;

            //voice 2
            else if (addr == 0x5056)
                voice2.freq[0] = v;
            else if (addr == 0x5057)
                voice2.freq[1] = v;
            else if (addr == 0x5058)
                voice2.freq[2] = v;
            else if (addr == 0x5059)
                voice2.freq[3] = v;

            //voice 3
            else if (addr == 0x505b)
                voice3.freq[0] = v;
            else if (addr == 0x505c)
                voice3.freq[1] = v;
            else if (addr == 0x505d)
                voice3.freq[2] = v;
            else if (addr == 0x505e)
                voice3.freq[3] = v;
        }

        public void update()
        {
            //Array.Clear(samples, 0, samples.Length);

            for (int i = 0; i < sample_length; i++)
            {
                sbyte[] res = get_sample();

                int total = res[0] / 3 + res[1] / 3 + res[2] / 3;
                samples[i] = (byte)(total);
            }
        }

        private sbyte[] get_sample()
        {
            sbyte[] temp = new sbyte[3];
            int freq1 = 0, freq2 = 0, freq3 = 0;

            for (int n = 0; n < 5; n++)
                freq1 = freq1 | (voice1.freq[n] & 0xf) << n * 4;

            accu1 = (accu1 + freq1) & 0xfffff;

            for (int n = 0; n < 4; n++)
                freq2 = freq2 | (voice2.freq[n] & 0xf) << n * 4;

            accu2 = (accu2 + freq2) & 0xffff;

            for (int n = 0; n < 4; n++)
                freq3 = freq3 | (voice3.freq[n] & 0xf) << n * 4;

            accu3 = (accu3 + freq3) & 0xffff;

            int pos = 0, s = 0;
            pos = (voice1.waveform & 7) * 32;
            s = z80.mem.sound_data[pos + (accu1 >> 15)];
            temp[0] = (sbyte)(s & voice1.volume & 0xf);

            pos = (voice2.waveform & 7) * 32;
            s = z80.mem.sound_data[pos + (accu2 >> 11)];
            temp[1] = (sbyte)(s & voice2.volume & 0xf);

            pos = (voice3.waveform & 7) * 32;
            s = z80.mem.sound_data[pos + (accu3 >> 11)];
            temp[2] = (sbyte)(s & voice3.volume & 0xf);

            return temp;
        }

        public void reset()
        {
            samples = new byte[sample_length];
        }
    }

    internal class Voices
    {
        public byte waveform;
        public byte[] freq;
        public byte volume;
        public byte[] accum;

        public Voices(byte waveform, byte[] freq, byte volume, byte[] accum)
        {
            this.waveform = waveform;
            this.freq = freq;
            this.volume = volume;
            this.accum = accum;
        }
    }
}
