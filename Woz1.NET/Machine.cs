using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woz1.NET
{
    public class Machine
    {
        internal CPU cpu;
        internal Terminal terminal;
        byte[] Memory = new byte[64 * 1024];
        bool[] writeLock = new bool[64 * 1024];

        int cpuMultiplier = int.MaxValue;
        int terminalMultiplier = 1;
        private Form1 form1;
        private bool terminalEarly;

        public Machine()
        {
            cpu = new CPU(this);
            terminal = new Terminal(this);
        }

        public Machine(Form1 form1):this()
        {
            this.form1 = form1;
        }
        public void MainLoop()
        {
            while (true)
            {
                terminalEarly = false;
                for (int i = 0; i < cpuMultiplier; i++)
                {
                    cpu.Step();
                    if (terminalEarly | cpu.stopped) break;
                }
                int termTimes = 0;
                while ((termTimes++ < terminalMultiplier) || ReadMemory(Terminal.DSP) > 0x80) terminal.Step();
                byte[] IN = new byte[0x80];
                
                for (int i = 0; i < 0x80; i++) IN[i] = ReadMemory((ushort)(0x200 + i));
                byte KBD = HWReadMemory(Terminal.KBD), KBD_CR = HWReadMemory(Terminal.KBD_CR), DSP = HWReadMemory(Terminal.DSP), DSP_CR = HWReadMemory(Terminal.DSP_CR);
                byte XAML = HWReadMemory(0x24), XAMH = HWReadMemory(0x25), STL = HWReadMemory(0x26), STH = HWReadMemory(0x27), L = HWReadMemory(0x28), H = HWReadMemory(0x29), YSAV = HWReadMemory(0x2A), MODE = HWReadMemory(0x2B);

                Debug.WriteLine("XAML={0:X02},XAMH={1:X02},STL={2:X02},STH={3:X02},L={4:X02},H={5:X02}", XAML, XAMH, STL, STH, L, H);
            }
        }

        public byte ReadMemory(ushort address)
        {
            if (address == Terminal.KBD) { WriteMemory(Terminal.KBD_CR, 0); if (terminal.inpQueue.Count == 0)terminalEarly = true; else return terminal.inpQueue.Dequeue(); }
            if (address == Terminal.KBD_CR) { terminalEarly = true; return (byte)(terminal.InpWaiting ? 0x80 : 0); }
            return Memory[address];
        }
        public byte HWReadMemory(ushort address) { return Memory[address]; }

        public void HWWriteMemory(ushort address, byte data)
        {
            Memory[address] = data;
        }
        public void WriteMemory(ushort address, byte data)
        {
            if (address>>8==0xD0||address>>8==0x0)
            {
                terminalEarly = true;
            }
            if (!writeLock[address]) Memory[address] = data;
        }

        public void LockMemory(ushort address)
        {
            writeLock[address] = true;
        }
        public void LoadRom(ushort address, string file)
        {
            foreach (byte b in File.ReadAllBytes(file))
            {
                this.WriteMemory(address, b);
                this.LockMemory(address);
                address++;
            }
        }

        string termFB = "";
        internal void PutCharOut(char c)
        {
            form1.termOut.Invoke(new Action(() =>
            {
                if (c == '\r')
                {

                    termFB += '\r'.ToString() + '\n'.ToString();
                }
                else if (c == '\n') return;
                else if (c == '\b' && termFB.Length > 0)
                {
                    termFB = termFB.Substring(0, termFB.Length - 1);
                    form1.Invalidate();
                    form1.termOut.Text = termFB;
                }
                else
                {
                    if (termFB.Contains('\n') && termFB.Substring(termFB.LastIndexOf('\n')).Length > 25)
                    {
                        termFB += '\n';
                    }
                    if (termFB.Count((ch) =>
                    {
                        return ch == '\n';
                    }) > 25)
                    {
                        termFB = termFB.Substring(termFB.IndexOf('\n') + 1);
                        form1.termOut.ResetText();
                    }
                    termFB += !char.IsControl(c) ? char.ToUpper(c).ToString() : " ";
                    form1.Invalidate();
                    form1.termOut.Text = termFB;
                }
            }));
        }

        internal void Reset()
        {
            termFB = "";
            form1.termOut.ResetText();
            cpu.stopped = false;
            cpu.Reset();
        }
    }
}
