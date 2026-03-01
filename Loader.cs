using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MID
{
    public static class Loader
    {
        public static void LoadProgram(string filename, MemoryManager memory, int startAddress)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException($"The file {filename} was not found.");
            }

            string[] lines = File.ReadAllLines(filename);
            int currentAddress = startAddress;

            foreach (string line in lines)
            {
                // 1. Clean the line
                // Remove comments (everything after ';') and whitespace
                string cleanLine = line.Split(';')[0].Trim();
                if (string.IsNullOrEmpty(cleanLine)) continue;

                // 2. Split into parts (OpCode Arg1, Arg2)
                // We split by spaces and commas to handle "movi r1, #1"
                string[] parts = cleanLine.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) continue;

                // 3. Parse OpCode
                string opCodeStr = parts[0];
                Opcode opCode;

                if (!Enum.TryParse(opCodeStr, true, out opCode)) // true = ignore case
                {
                    Console.WriteLine($"Error: Unknown opcode '{opCodeStr}'");
                    continue;
                }

                // 4. Parse Arguments (Default to 0 if missing)
                int arg1 = (parts.Length > 1) ? ParseArgument(parts[1]) : 0;
                int arg2 = (parts.Length > 2) ? ParseArgument(parts[2]) : 0;

                // 5. Write to Memory (9 Bytes total)
                // Byte 1: OpCode
                memory.WriteByte(currentAddress, (byte)opCode);

                // Bytes 2-5: Arg1
                memory.WriteInt32(currentAddress + 1, arg1);

                // Bytes 6-9: Arg2
                memory.WriteInt32(currentAddress + 5, arg2);

                // Advance address by 9 bytes
                currentAddress += 9;
            }



        }

        private static int ParseArgument(string arg)
        {
            arg = arg.Trim();

            // Case 1: Register (e.g., "r1")
            if (arg.StartsWith("r", StringComparison.OrdinalIgnoreCase) && char.IsDigit(arg[1]))
            {
                return int.Parse(arg.Substring(1));
            }

            // Case 2: Numeric Constant (e.g., "#10")
            if (arg.StartsWith("#"))
            {
                return int.Parse(arg.Substring(1));
            }

            // Case 3: Character Constant (e.g., "@a")
            if (arg.StartsWith("@"))
            {
                char c = arg[1];
                return (int)c; // Return ASCII value
            }

            // Case 4: Memory Address / Raw Number (e.g., "1234" or "-5")
            // This handles negative numbers too if they don't have '#' prefix
            int result;
            if (int.TryParse(arg, out result))
            {
                return result;
            }

            return 0; // Default fallback




        }
    }
    
}
