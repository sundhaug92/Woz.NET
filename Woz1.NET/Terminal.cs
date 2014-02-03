using System;
using System.Collections.Generic;

namespace Woz1.NET
{
    internal class Terminal
    {
        internal static readonly ushort KBD = 0xD010, KBD_CR = 0xD011, DSP = 0xD012, DSP_CR = 0xD013;
        private byte tmpByte;
        private Machine machine;

        public Terminal(Machine machine)
        {
            this.machine = machine;
        }

        private long lastTicks = 0;

        internal void Step()
        {
            if ((machine.HWReadMemory(DSP) & (1 << 7)) > 0)
            {
                if (DateTime.Now.Ticks >= (lastTicks))
                {
                    tmpByte = (byte)(machine.HWReadMemory(DSP) & 0x7F);
                    machine.PutCharOut((char)tmpByte);
                    machine.HWWriteMemory(DSP, tmpByte);
                    lastTicks = DateTime.Now.Ticks;
                }
            }
            else
            {
                var d = machine.HWReadMemory(DSP);
            }
        }

        internal Queue<byte> inpQueue = new Queue<byte>();

        internal bool InpWaiting
        {
            get
            {
                return inpQueue.Count > 0;
            }
        }

        internal void AppendOut(char p)
        {
            inpQueue.Enqueue((byte)(char.ToUpper(p) | 0x80));
        }
    }
}