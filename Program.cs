using MID;
using System;
using System.IO;

namespace MID
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Setup the Machine
            // 65536 bytes = 64KB RAM (plenty for now)
            MemoryManager memory = new MemoryManager(65536);
            CPU cpu = new CPU(memory);

            // 2. Create a test file
            string testFile = "test_prog.txt";
            string[] programLines = {
                "movi r1, #10",     // Set r1 to 10
                "movi r2, #5",      // Set r2 to 5
                "addr r1, r2",      // r1 = r1 + r2 (Should be 15)
                "printr r1",        // Print r1
                "exit"              // Stop
            };
            File.WriteAllLines(testFile, programLines);

            // 3. Load the program
            // We load it at address 0 for simplicity right now
            Console.WriteLine("Loading program...");
            Loader.LoadProgram(testFile, memory, 0);

            // 4. Start the CPU
            Console.WriteLine("Starting CPU...");
            cpu.Run();

            Console.WriteLine("CPU Halted.");
            Console.ReadKey();
        }
    }
}