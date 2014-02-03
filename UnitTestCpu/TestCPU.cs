using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Woz1.NET;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class TestCPU
    {
        CPU cpu = new CPU(new Machine());
        [TestMethod]
        public void ReadWriteMemory8()
        {
            ushort testAddr=0;
            byte testData = (byte)(new Random()).Next(255);
            cpu.WriteMemory8(testAddr, testData);

            //
            Assert.AreEqual(testData, cpu.ReadMemory8(testAddr));
        }
        [TestMethod]
        public void ReadWriteMemory16()
        {
            ushort testAddr = 0;
            ushort testData = (ushort)(new Random()).Next(65535);
            cpu.WriteMemory16(testAddr, testData);

            //
            Assert.AreEqual(testData, cpu.ReadMemory16(testAddr));
            Assert.AreEqual(testData & 0xFF, cpu.ReadMemory8(testAddr));
            Assert.AreEqual(testData >> 8, cpu.ReadMemory8((ushort)(testAddr+1)));
        }
        [TestMethod]
        public void CheckNZ()
        {

            int[] testData = { 0, 1, 2, 4, 8, 16, 32, 64, 128, 255, 256 };
            bool[] correctN = { false, false, false, false, false, false, false, false, true, true, false };
            bool[] correctZ = { true, false, false, false, false, false, false, false, false, false, true };

            for (int i = 0; i < testData.Length;i++ )
            {
                cpu.CheckN(testData[i]);
                cpu.CheckZ(testData[i]);

                Assert.AreEqual(correctN[i], cpu.NegativeFlag);
                Assert.AreEqual(correctZ[i], cpu.ZeroFlag);
            }
        }
        [TestMethod]
        public void getByteByMode()
        {
            for (uint i = 0; i < 0x10000; i++) cpu.WriteMemory8((ushort)i, (byte)(i & 0xFF));
            cpu.rPC = 0x10;
            cpu.rFlags = 0;
            Assert.AreEqual(0x10, cpu.getByteByMode(Mode.Absolute));
            cpu.rPC = 0x10;
            Assert.AreEqual(0x10, cpu.getByteByMode(Mode.ZeroPage));
        }

        [TestMethod]
        public void ASL()
        {
            cpu.CarryFlag = true;
            cpu.setRegister(SelectedRegister.A,0);
            cpu.ASL(Mode.Accumulator);
            Assert.AreEqual(0, cpu.getRegister(SelectedRegister.A));
            Assert.AreEqual(false, cpu.CarryFlag);

            cpu.setRegister(SelectedRegister.A, 0x80);
            cpu.ASL(Mode.Accumulator);
            Assert.AreEqual(0, cpu.getRegister(SelectedRegister.A));
            Assert.AreEqual(true, cpu.CarryFlag);
        }
        [TestMethod]
        public void ROL()
        {
            cpu.CarryFlag = true;
            cpu.setRegister(SelectedRegister.A, 0);
            cpu.ROL(Mode.Accumulator);
            Assert.AreEqual(1, cpu.getRegister(SelectedRegister.A));
            Assert.AreEqual(false, cpu.CarryFlag);

            cpu.setRegister(SelectedRegister.A, 0x80);
            cpu.ROL(Mode.Accumulator);
            Assert.AreEqual(0, cpu.getRegister(SelectedRegister.A));
            Assert.AreEqual(true, cpu.CarryFlag);
        }
        /*[TestMethod]
        public void ROR()
        {
            cpu.CarryFlag = true;
            cpu.rA = 0;
            cpu.ROR(Mode.Accumulator);
            Assert.AreEqual(0x80, cpu.rA);
            Assert.AreEqual(false, cpu.CarryFlag);

            cpu.rA = 0x80;
            cpu.ROR(Mode.Accumulator);
            Assert.AreEqual(0x40, cpu.rA);
            Assert.AreEqual(false, cpu.CarryFlag);
        }*/
        [TestMethod]
        public void LSR()
        {
            cpu.CarryFlag = true;
            cpu.setRegister(SelectedRegister.A, 0);
            cpu.LSR(Mode.Accumulator);
            Assert.AreEqual(0, cpu.getRegister(SelectedRegister.A));
            Assert.AreEqual(false, cpu.CarryFlag);

            cpu.setRegister(SelectedRegister.A, 1);
            cpu.LSR(Mode.Accumulator);
            Assert.AreEqual(0, cpu.getRegister(SelectedRegister.A));
            Assert.AreEqual(true, cpu.CarryFlag);
        }
        [TestMethod]
        public void rFlags()
        {
            cpu.rFlags = 0xFF;
            Assert.AreEqual(0xFF, cpu.rFlags);
            Assert.AreEqual(true, cpu.NegativeFlag);
            Assert.AreEqual(true, cpu.OverflowFlag);
            Assert.AreEqual(true, cpu.BreakFlag);
            Assert.AreEqual(true, cpu.DecimalFlag);
            Assert.AreEqual(true, cpu.InterruptDisableFlag);
            Assert.AreEqual(true, cpu.ZeroFlag);
            Assert.AreEqual(true, cpu.CarryFlag);

            cpu.rFlags = 0x00;
            Assert.AreEqual((byte)(1 << 5), cpu.rFlags);
            Assert.AreEqual(false, cpu.NegativeFlag);
            Assert.AreEqual(false, cpu.OverflowFlag);
            Assert.AreEqual(false, cpu.BreakFlag);
            Assert.AreEqual(false, cpu.DecimalFlag);
            Assert.AreEqual(false, cpu.InterruptDisableFlag);
            Assert.AreEqual(false, cpu.ZeroFlag);
            Assert.AreEqual(false, cpu.CarryFlag);

            cpu.rFlags = 0x01;
            Assert.AreEqual(true, cpu.CarryFlag);

            cpu.rFlags = 0x02;
            Assert.AreEqual(true, cpu.ZeroFlag);

            cpu.rFlags = 0x04;
            Assert.AreEqual(true, cpu.InterruptDisableFlag);

            cpu.rFlags = 0x08;
            Assert.AreEqual(true, cpu.DecimalFlag);

            cpu.rFlags = 0x10;
            Assert.AreEqual(true, cpu.BreakFlag);

            //

            cpu.rFlags = 0x40;
            Assert.AreEqual(true, cpu.OverflowFlag);

            cpu.rFlags = 0x80;
            Assert.AreEqual(true, cpu.NegativeFlag);
        }
        [TestMethod]
        public void PushPop8()
        {
            cpu.rSP = 0xFF;
            byte testData = (byte)(new Random()).Next(255);
            cpu.Push8(testData);
            Assert.AreEqual(0xFE, cpu.rSP);
            Assert.AreEqual(testData, cpu.ReadMemory8(0x1FF));

            Assert.AreEqual(testData, cpu.Pop8());
            Assert.AreEqual(0xFF, cpu.rSP);
            Assert.AreEqual(testData, cpu.ReadMemory8(0x1FF));
        }
        [TestMethod]
        public void PushPop16()
        {
            cpu.rSP = 0xFF;
            ushort testData = (ushort)(new Random()).Next(255);
            cpu.Push16(testData);
            Assert.AreEqual(0xFD, cpu.rSP);
            //Assert.AreEqual(testData, cpu.ReadMemory8(0x1FF));

            Assert.AreEqual(testData, cpu.Pop16());
            Assert.AreEqual(0xFF, cpu.rSP);
            //Assert.AreEqual(testData, cpu.ReadMemory8(0x1FF));
        }

        [TestMethod]
        public void GetSetRegister()
        {
            cpu.setRegister(SelectedRegister.X, 1);
            Assert.AreEqual(1, cpu.getRegister(SelectedRegister.X));

            cpu.setRegister(SelectedRegister.Y, 2);
            Assert.AreEqual(2, cpu.getRegister(SelectedRegister.Y));

            cpu.setRegister(SelectedRegister.SP, 3);
            Assert.AreEqual(3, cpu.getRegister(SelectedRegister.SP));
        }

        [TestMethod]
        public void ADC()
        {
            cpu.rA = 0;
            cpu.CarryFlag = true;
            cpu.ADC(Mode.Accumulator); //Not a real op, but for a 0+0+C test, ok
            Assert.AreEqual(1, cpu.rA);
            Assert.AreEqual(0x20, cpu.rFlags);

            cpu.rA = 0;
            cpu.CarryFlag = false;
            cpu.ADC(Mode.Accumulator); //Not a real op, but for a 0+0+C test, ok
            Assert.AreEqual(0, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);

            cpu.rA = 1;
            cpu.CarryFlag = false;
            cpu.ADC(Mode.Accumulator); //Not a real op, but for a 0+0+C test, ok
            Assert.AreEqual(2, cpu.rA);
            Assert.AreEqual(0x20, cpu.rFlags);
        }

        [TestMethod]
        public void SBC()
        {
            cpu.rA = 0;
            cpu.CarryFlag = true;
            cpu.SBC(Mode.Accumulator); //Not a real op, but for a 0+0+C test, ok
            Assert.AreEqual(0, cpu.rA);
            Assert.AreEqual(0x23, cpu.rFlags);

            cpu.rA = 0;
            cpu.CarryFlag = false;
            cpu.SBC(Mode.Accumulator); //Not a real op, but for a 0+0+C test, ok
            Assert.AreEqual(0xFF, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);

            cpu.rA = 1;
            cpu.CarryFlag = false;
            cpu.SBC(Mode.Accumulator); //Not a real op, but for a 0+0+C test, ok
            Assert.AreEqual(0xFF, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);
        }
        [TestMethod]
        public void IncreaseMemory()
        {
            cpu.rFlags = 0;
            cpu.rA = 1;
            cpu.IncreaseMemory(Mode.Accumulator);
            Assert.AreEqual(2, cpu.rA);
            Assert.AreEqual(0x20, cpu.rFlags);

            cpu.rFlags = 0;
            cpu.rA = 0x7F;
            cpu.IncreaseMemory(Mode.Accumulator);
            Assert.AreEqual(0x80, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);

            cpu.rFlags = 0;
            cpu.rA = 0xFF;
            cpu.IncreaseMemory(Mode.Accumulator);
            Assert.AreEqual(0x00, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);
        }
        [TestMethod]
        public void Increase()
        {
            cpu.rFlags = 0;
            cpu.rA = 1;
            cpu.Increase(SelectedRegister.A);
            Assert.AreEqual(2, cpu.rA);
            Assert.AreEqual(0x20, cpu.rFlags);

            cpu.rFlags = 0;
            cpu.rA = 0x7F;
            cpu.Increase(SelectedRegister.A);
            Assert.AreEqual(0x80, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);

            cpu.rFlags = 0;
            cpu.rA = 0xFF;
            cpu.Increase(SelectedRegister.A);
            Assert.AreEqual(0x00, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);
        }
        [TestMethod]
        public void DecreaseMemory()
        {
            cpu.rFlags = 0;
            cpu.rA = 1;
            cpu.DecreaseMemory(Mode.Accumulator);
            Assert.AreEqual(0, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);

            cpu.rFlags = 0;
            cpu.rA = 0x81;
            cpu.DecreaseMemory(Mode.Accumulator);
            Assert.AreEqual(0x80, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);
        }
        [TestMethod]
        public void Decrease()
        {
            cpu.rFlags = 0;
            cpu.rA = 1;
            cpu.Decrease(SelectedRegister.A);
            Assert.AreEqual(0, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);

            cpu.rFlags = 0;
            cpu.rA = 0x81;
            cpu.Decrease(SelectedRegister.A);
            Assert.AreEqual(0x80, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);
        }
        
        [TestMethod]
        public void LoadStoreRegister()
        {
            cpu.rA = (byte)~cpu.rX;
            cpu.LoadRegister(SelectedRegister.X, Mode.Accumulator);
            Assert.AreEqual(cpu.rA, cpu.rX);

            cpu.rA = (byte)~cpu.rX;
            cpu.StoreRegister(SelectedRegister.X, Mode.Accumulator);
            Assert.AreEqual(cpu.rA, cpu.rX);
        }

        [TestMethod]
        public void JumpRoutineRestore()
        {
            var pc = cpu.rPC;
            cpu.JumpSubroutine();
            Assert.AreEqual(cpu.ReadMemory16(pc), cpu.rPC);
            cpu.ReturnFromSubroutine();
            Assert.AreEqual(pc + 2, cpu.rPC);
        }

        [TestMethod]
        public void EOR()
        {
            cpu.rPC = 0x200;
            cpu.rFlags = 0;
            cpu.WriteMemory8(cpu.rPC, (byte)~cpu.rA);
            cpu.EOR(Mode.Immediate);
            Assert.AreEqual(0xFF, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);

            cpu.WriteMemory8(cpu.rPC, 0xFF);
            cpu.EOR(Mode.Immediate);
            Assert.AreEqual(0x00, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);
        }

        [TestMethod]
        public void AND()
        {
            cpu.rPC = 0x200;
            cpu.rFlags = 0;
            cpu.WriteMemory8(cpu.rPC, (byte)~cpu.rA);
            cpu.AND(Mode.Immediate);
            Assert.AreEqual(0x00, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);

            cpu.rA = 0xFF;
            cpu.WriteMemory8(cpu.rPC, 0xFF);
            cpu.AND(Mode.Immediate);
            Assert.AreEqual(0xFF, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);
        }

        [TestMethod]
        public void ORA()
        {
            cpu.rPC = 0x200;
            cpu.rFlags = 0;
            cpu.WriteMemory8(cpu.rPC, 0xFF);
            cpu.ORA(Mode.Immediate);
            Assert.AreEqual(0xFF, cpu.rA);
            Assert.AreEqual(0xA0, cpu.rFlags);

            cpu.rA = 0;
            cpu.WriteMemory8(cpu.rPC, 0);
            cpu.ORA(Mode.Immediate);
            Assert.AreEqual(0x00, cpu.rA);
            Assert.AreEqual(0x22, cpu.rFlags);
        }

        [TestMethod]
        public void Bit()
        {
            cpu.rPC = 0x200;
            cpu.rA = 0;
            cpu.WriteMemory8(cpu.rPC, 0xFF);
            cpu.Bit(Mode.Immediate);
            Assert.AreEqual(0xE2, cpu.rFlags);
        }

        [TestMethod]
        public void JMP()
        {
            cpu.Reset();
            cpu.rPC = 0xFF91;
            cpu.Step();
            Assert.AreEqual(0xFF44, cpu.rPC);

            cpu.rPC = 0xFF94;
            cpu.Step();
            Assert.AreEqual(cpu.ReadMemory16(0x24), cpu.rPC);
        }

        [TestMethod]
        public void JumpTo()
        {
            ushort us=(ushort)~cpu.rPC;
            cpu.JumpTo(us);
            Assert.AreEqual(cpu.rPC, us);
        }
        
        [TestMethod]
        public void Reset()
        {
            cpu.rA = cpu.rX = cpu.rY = cpu.rFlags = 0xFF;
            cpu.rSP = 0;
            cpu.rPC = 0xD010;
            cpu.Reset();
            Assert.AreEqual(0, cpu.rA);
            Assert.AreEqual(0, cpu.rX);
            Assert.AreEqual(0, cpu.rY);
            Assert.AreEqual(0xFF, cpu.rSP);
            Assert.AreEqual(0x20, cpu.rFlags);
            Assert.AreEqual(0xFF00, cpu.rPC);
        }
        [TestMethod]
        public void Transfer()
        {
            foreach (SelectedRegister sr1 in (SelectedRegister[])Enum.GetValues(typeof(SelectedRegister)))
            {
                cpu.setRegister(sr1, (byte)new Random().Next(256));
                foreach (SelectedRegister sr2 in (SelectedRegister[])Enum.GetValues(typeof(SelectedRegister)))
                {
                    if (sr1 != sr2)
                    {
                        cpu.Transfer(sr1, sr2);
                        Assert.AreEqual(cpu.getRegister(sr1), cpu.getRegister(sr2));
                    }
                }
            }
        }
        [TestMethod]
        public void Compare()
        {
            cpu.Reset();
            cpu.rA = 0xFF;
            cpu.Compare(SelectedRegister.A, Mode.Accumulator);
            Assert.AreEqual(0xA3, cpu.rFlags);
        }
        [TestMethod]
        public void GetSetByModeSymmetry()
        {
            foreach (Mode mode in (Mode[])Enum.GetValues(typeof(Mode)))
            {
                if (mode == Mode.Immediate || mode==Mode.ZeroPageY||mode==Mode.Indirect) continue;//ZPY not yet implemented,Indirect only for 16b
                byte b = (byte)new Random().Next(256);
                ushort _pc = cpu.rPC;
                cpu.setByteByMode(mode, b);
                cpu.rPC = _pc;
                Assert.AreEqual(b, cpu.getByteByMode(mode), mode.ToString());
            }
        }
    }
}
