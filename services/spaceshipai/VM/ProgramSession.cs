using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using SpaceshipAI.UI.Backend;

namespace SpaceshipAI.VM
{
    public class ProgramSession
    {
        private readonly WriteLineDelegate _writeLine;

        public ProgramSession(User user, WriteLineDelegate writeLine)
        {
            Eax = new Register();
            Ebx = new Register();
            Ecx = new Register();
            Edx = new Register();
            Ebp = new Register();
            Esp = new Register();
            Esi = new Register();
            Edi = new Register();
            Esp.Value = SpaceVM.VirtualMemorySize;
            HeapOffset = 0;
            Offset = user.Id*SpaceVM.VirtualMemorySize;
            _writeLine = writeLine;
        }

        public Register Eax { get; set; }
        public Register Ebx { get; set; }
        public Register Ecx { get; set; }
        public Register Edx { get; set; }
        public Register Ebp { get; set; }
        public Register Esp { get; set; }
        public Register Esi { get; set; }
        public Register Edi { get; set; }
        public int Offset { get; set; }

        private int HeapOffset { get; set; }

        public byte this[int address]
        {
            get
            {
                int absAddress = Offset + address;
                if (absAddress < 0 || absAddress >= SpaceVM.MemorySize)
                    throw new IndexOutOfRangeException("Access violation @ Addr  0x" + address.ToString("X"));
                return SpaceVM.Memory[absAddress];
            }
            set
            {
                int absAddress = Offset + address;
                if (absAddress < 0 || absAddress >= SpaceVM.MemorySize)
                    throw new IndexOutOfRangeException("Access violation @ Addr  0x" + address.ToString("X"));
                SpaceVM.Memory[absAddress] = value;
            }
        }

        public int AddBytes(params byte[] bytes)
        {
            if (HeapOffset + bytes.Length >= SpaceVM.VirtualMemorySize)
                throw new OutOfMemoryException("Out of memory.");
            for (int i = 0; i < bytes.Length; i++)
            {
                this[HeapOffset + i] = bytes[i];
            }
            int originalAddress = HeapOffset;
            HeapOffset += bytes.Length;
            return originalAddress;
        }

        public void Push(int value)
        {
            if (Esp.Value < 0 || Esp.Value > SpaceVM.VirtualMemorySize)
                throw new Exception("Access violation @ Addr  0x" + Esp.Value.ToString("X"));
            this[Esp.Value] = (byte) (value & 0xFF);
            this[Esp.Value + 1] = (byte) ((value >> 8) & 0xFF);
            this[Esp.Value + 2] = (byte) ((value >> 16) & 0xFF);
            this[Esp.Value + 3] = (byte) ((value >> 24) & 0xFF);
            Esp.Value -= 4;
        }

        public int Pop()
        {
            if (Esp.Value < 0 || Esp.Value > SpaceVM.VirtualMemorySize)
                throw new Exception("Access violation @ Addr  0x" + Esp.Value.ToString("X"));
            Esp.Value += 4;
            int value = this[ Esp.Value + 3];
            value = (value << 8) + this[Esp.Value + 2];
            value = (value << 8) + this[Esp.Value + 1];
            value = (value << 8) + this[Esp.Value];
            return value;
        }


        private string ReadString()
        {
            int i = Eax.Value;
            var result = new StringBuilder();
            while (this[i] != 0)
            {
                result.Append((char) this[i]);
                i++;
            }
            return result.ToString();
        }


        public void print()
        {
            _writeLine(ReadString());
        }

        public void printint()
        {
            _writeLine(Eax.Value.ToString(CultureInfo.InvariantCulture));
        }

        public void procaddr()
        {
            Eax.Value = Offset;
        }

        public void memsize()
        {
            Eax.Value = SpaceVM.VirtualMemorySize;
        }

        public void usercount()
        {
            Eax.Value = Users.UserCount;
        }
    }
}