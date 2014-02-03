using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Woz1.NET
{
    public partial class Form1 : Form
    {
        private Machine machine;
        private Thread loaderThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            machine = new Machine(this);
            (new Thread(machine.MainLoop)).Start();
        }

        private void termKeyPress(object sender, KeyPressEventArgs e)
        {
            if (machine.terminal.InpWaiting) return;
            machine.terminal.AppendOut(e.KeyChar);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loaderThread != null && loaderThread.IsAlive) loaderThread.Abort();
            machine.Reset();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            loaderThread = new Thread(FileLoader);
            loaderThread.Start();
        }

        private void FileLoader(object obj)
        {
            foreach (char ch in File.ReadAllText(openFileDialog1.FileName))
            {
                while (machine.terminal.InpWaiting) Thread.Yield();
                machine.terminal.AppendOut(ch);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            machine.cpu.stopped = true;
            if (!File.Exists(saveFileDialog1.FileName)) File.Create(saveFileDialog1.FileName);
            byte[] mem = new byte[0x10000];
            for (int i = 0; i < 0x10000; i++)
            {
                mem[i] = machine.HWReadMemory((ushort)i);
            }
            File.WriteAllBytes(saveFileDialog1.FileName, mem);
            machine.cpu.stopped = false;
            MessageBox.Show("Save complete");
        }
    }
}