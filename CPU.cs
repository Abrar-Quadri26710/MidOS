using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MID
{
    public class CPU
    {
        //[cite_start]
        private int[] registers = new int[15];

        private bool zeroFlag; // Compartive, both quantities equal to 0
        private bool signFlag; // Checks if the result is negative

        private MemoryManager _memory;

        private const int IP_REGISTER = 11; // Instruction Pointer Register Index
        private const int SP_REGISTER = 13; // Stack Pointer Register Index

        public CPU(MemoryManager memory)
        {
            _memory = memory;
        }



        public void FetchArgs(out int arg1, out int arg2)
        {
            int ip = registers[11];
            //arguement 1 starts at IP + 1 and then 
            // arg 2 starts at IP + 5 aka the end of arg 1 
            arg1 = _memory.ReadInt32(ip + 1);

            arg2 = _memory.ReadInt32(ip + 5);
        }

        //will probably do the FDE
        public void Run()
        {
            bool running = true;
            while (running)
            {
                // Fetch
                //int currentIP = registers[IP_REGISTER];
                int ip = registers[11];

                if(ip < 0 || ip >= _memory.Size)
                {
                    throw new Exception("Instruction Pointer out of bounds");
                }

                //read the opcode from memory
                byte opcodeByte = _memory.ReadByte(ip);
                Opcode opCode = (Opcode)opcodeByte;

                //Get Arguments
                int arg1, arg2;
                FetchArgs(out arg1, out arg2);

                bool jumped = false; 

                /*
                //Console.WriteLine($"Executing Opcode: {opCode} at IP: {ip}");

                // Decode
                // Increment IP to point to next byte so we dont loop forvever
                registers[IP_REGISTER] += 9;

                if (opCode == Opcode.EXIT)
                {
                    running = false;
                    
                }

                */

                // Execute
                //Refer to module 1 for opcode meanings
                //not that immediate values refer to actual numbers
                switch (opCode)
                {
                    case Opcode.EXIT:
                        running = false;
                        break;

                    case Opcode.PUSHI: //Push Immediate value onto stack
                        registers[13] -= 4; // Move Stack Pointer down
                        _memory.WriteInt32(registers[13], arg2); // Write immediate value to stack
                        break;
                    case Opcode.PUSHR: //Push Register value onto stack
                        registers[13] -= 4; // Move Stack Pointer down
                        _memory.WriteInt32(registers[13], registers[arg1]); // Write register value to stack
                        break;
                    case Opcode.POPR: //Pop value from stack into Register
                        registers[arg1] = _memory.ReadInt32(registers[13]); // Read value from stack into register
                        registers[13] += 4; // Move Stack Pointer up
                        break;

                    case Opcode.CMPI: //Compare Register to Immediate value
                        int resultI = registers[arg1] - arg2;
                        zeroFlag = (resultI == 0);
                        signFlag = (resultI < 0);
                        break;
                    case Opcode.CMPR: //Compare Register to Register
                        int resultR = registers[arg1] - registers[arg2];
                        zeroFlag = (resultR == 0);
                        signFlag = (resultR < 0);
                        break;

                    case Opcode.JMP: //Unconditional Jump to address in Register
                    case Opcode.JMPI: //Unconditional Jump to immediate address
                        int targetAddress = (opCode == Opcode.JLT) ? registers[arg1] : arg1;
                        registers[11] = targetAddress;
                        jumped = true;
                        break;
                    case Opcode.JMPA: //Jump to address in Register if sign flag is set (less than)
                        registers[11] = arg1;
                        jumped = true;
                        break;
                    case Opcode.JLT: //Jump if Less Than (sign flag set)
                    case Opcode.JLTI:
                        if (signFlag)
                        {
                            int targetAddrLT = (opCode == Opcode.JE) ? registers[arg1] : arg1;
                            registers[11] = targetAddrLT;
                            jumped = true;
                        }
                        break;
                    case Opcode.JE:

                    case Opcode.JEI: //Jump if Equal (zero flag set)
                        if (zeroFlag)
                        {
                            int targetAddrEQ = (opCode == Opcode.JE) ? registers[arg1] : arg1;
                            registers[11] = targetAddrEQ;
                            jumped = true;
                        }
                        break;

                    case Opcode.CALL: //Call subroutine at address in Register
                        registers[13] -= 4; // Move Stack Pointer down
                        _memory.WriteInt32(registers[13], registers[11] + 9); // Push return address onto stack
                        registers[11] = registers[arg1]; // Jump to subroutine
                        jumped = true;
                        break;
                    case Opcode.RET:
                        int returnaddress = _memory.ReadInt32(registers[13]); // Pop return address from stack
                        registers[13] += 4; // Move Stack Pointer up
                        registers[11] = returnaddress; // Jump back to return address
                        jumped = true;
                        break;


                    case Opcode.MOVI: //What movi does is move immediate value to register
                        registers[arg1] = arg2;
                        break;
                    case Opcode.MOVR: //Move Register to Register
                        registers[arg1] = registers[arg2];
                        break;
                    case Opcode.MOVMR: //Move Memory to Register
                        int sourceAddress = registers[arg1];
                        _memory.WriteInt32(sourceAddress, registers[arg2]);
                        break;
                    case Opcode.INCR: //Increment Register (by 1)
                        registers[arg1]++;
                        break;
                    case Opcode.ADDI: //Add immediate value to Register 
                        registers[arg1] += arg2;
                        break;
                    case Opcode.ADDR: //Add Register to Register
                        registers[arg1] += registers[arg2];
                        break;
                    case Opcode.PRINTR: //Print Register
                        Console.WriteLine($"Register {arg1}: {registers[arg1]}");
                        break;

                }

                registers[11] += 9; // Move to the next instruction

            }


        }

    }
}
