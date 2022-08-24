// See https://aka.ms/new-console-template for more information
using System;
using System.Text;
using Z80.Net.Core;
using reg = Z80.Net.Core.Registers;

namespace Z80.Tests // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        PacMachine z80;
        List<Test>? tests;
        private string? test_result;

        static void Main(string[] args)
        {
            new Program().run();
        }

        private void run()
        {
            z80 = new(true);

            tests = new();

            byte[] data = File.ReadAllBytes("Content/testfiles/zexall.bin");

            for (int i = 0; i < 67; i++)
            {
                string name = Encoding.ASCII.GetString(data, 0x0103 + (i * 0x60), 0x1e);
                int offset = 0x0103 + (i * 0x60);
                tests.Add(new Test(name, offset));
            }

            bool exit_app = false;
            int last_test = 0;

            while (!exit_app)
            {
                int test_number = -1;

                Console.WriteLine($"\n");
                for (int n = 0; n < tests.Count; n += 2)
                {
                    Console.Write($"{n + 1,2} - {tests[n].name} |");
                    if (n < tests.Count - 1)
                        Console.Write($"{n + 2,2} - {tests[n + 1].name}\n");
                    else
                        Console.Write("\n");
                }

                if (test_result != string.Empty)
                    Console.WriteLine($"\n{last_test} - {test_result}");

                if (last_test > tests.Count - 1)
                {
                    Console.Write("Invalid Test Number\n");
                }

                Console.WriteLine("\nPress Q to exit app\n");
                Console.Write("\nChoose Test Number: ");

                string? op = Console.ReadLine();
                var isnumber = int.TryParse(op, out test_number);
                test_number--;

                if (test_number > -1)
                    last_test = test_number + 1;

                if (op?.ToUpper() == "Q")
                {
                    exit_app = true;
                    continue;
                }

                if (test_number > -1 && test_number < tests.Count - 1 && isnumber)
                {
                    z80.mem.load_test();
                    z80.mem.set_test_number(test_number);
                    bool test_done = false;
                    test_result = "";

                    z80.cpu.state = Cpu.cstate.running;

                    z80.tracer.open_close_log(true);

                    while (!test_done)
                    {
                        z80.cpu.step();

                        //if (reg.pc == 0x1d43)// || reg.pc == 0x1d46)
                        //    z80.tracer.log_to_file();

                        if (get_test_messages() || z80.cpu.state == Cpu.cstate.debugging)
                            test_done = true;
                    }

                    z80.tracer.open_close_log(false);
                }
            }
        }

        public bool get_test_messages()
        {
            if (reg.pc == 0x0000)
            {
                z80.cpu.state = Cpu.cstate.debugging;
            }
            else if (reg.pc == 0x002b)
            {
                string c = Encoding.ASCII.GetString(new byte[] { z80.mem.Rb(0xffff) });
                test_result += c;
                Console.Write(c);

                if (test_result.Contains("complete"))
                    return true;

                switch (reg.c)
                {
                    case 2:
                        //Console.Write(Encoding.ASCII.GetString(new byte[] {reg.c}));
                        break;
                    case 9:
                        ushort addr = reg.de;

                        //if (addr == 0x1df6)
                        //    return true;
                        break;
                }
            }

            return false;
        }
    }

    internal class Test
    {
        public string name;
        public int offset;

        public Test(string name, int offset)
        {
            this.name = name;
            this.offset = offset;
        }
    }
}