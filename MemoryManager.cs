using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//The point of this program is to allocate RAM so programs dont have
//to do that shit for themslevs 

//So what I have to do here is to create functions that I would use
//in a memory manager

//Such functions would be 
// Read
// Write
// Allocate??
// 

namespace MID
{
    public class MemoryManager
    {
        //Storage
        private byte[] _physicalMemory;

        // Constructor that will create array of byte size 
        // Kinda similar to plugging in a ram stick 
        public MemoryManager(int bytesize)
        {
            _physicalMemory = new byte[bytesize];
        }

        //The read and write byte functions
        //These will serve as basic functions
        //Read byte will obtain the byte
        // Write will overwrite 
        public byte ReadByte(int address)
        {
            return _physicalMemory[address];
        }

        public void WriteByte(int address, byte value)
        {
            _physicalMemory[address] = value;

        }


        //This will take byte values and turn them into readable int values
        public int ReadInt32(int address)
        {
            return BitConverter.ToInt32(_physicalMemory, address);
        }

        //
        public void WriteInt32(int address, int value)
        {
            // Convert int into byte array 
            byte[] bytes = BitConverter.GetBytes(value);

            //Then takes the byte array and copies it into physical memory
            Array.Copy(bytes, 0, _physicalMemory, address, 4);
        }

        public int Size
        {
            get { return _physicalMemory.Length; }
        }




    }
}
