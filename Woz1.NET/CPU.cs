using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Woz1.NET
{
    public class CPU
    {
        private Machine machine;
        string[] basicListing;

        public CPU(Machine machine)
        {
            // TODO: Complete member initialization
            this.machine = machine;

            machine.LoadRom(0xE000, "Roms\\basic.rom");
            machine.LoadRom(0xFF00, "Roms\\monitor.rom");
            basicListing = File.ReadAllLines("Roms\\a1basic.dis");

            rPC = 0xFF00;
        }

        public void Step()
        {
            if (resetNeeded) Reset();
            if (stopped)
            {
                /*Debug.Print("Processor requires restart")*/
                return;
            }
            else try
                {
                    Dump();
                    ExecuteOpcode(ReadMemory8(rPC++));
                }
                catch (Exception e)
                {
                    Crash(e.Message + "@" + e.StackTrace);
                }
        }

        public void ExecuteOpcode(byte op)
        {
            switch (op)
            {
                //JMP
                case 0x4C:
                    JMP(Mode.Absolute);
                    break;
                case 0x6C:
                    JMP(Mode.Indirect);
                    break;
                //DEC
                case 0xC6:
                    DecreaseMemory(Mode.ZeroPage);
                    break;
                case 0xD6:
                    DecreaseMemory(Mode.ZeroPageX);
                    break;
                case 0xCE:
                    DecreaseMemory(Mode.Absolute);
                    break;
                case 0xDE:
                    DecreaseMemory(Mode.AbsoluteX);
                    break;

                //INC
                case 0xE6:
                    IncreaseMemory(Mode.ZeroPage);
                    break;
                case 0xF6:
                    IncreaseMemory(Mode.ZeroPageX);
                    break;
                case 0xEE:
                    IncreaseMemory(Mode.Absolute);
                    break;
                case 0xFE:
                    IncreaseMemory(Mode.AbsoluteX);
                    break;

                //ADC
                case 0x69:
                    ADC(Mode.Immediate);
                    break;
                case 0x65:
                    ADC(Mode.ZeroPage);
                    break;
                case 0x75:
                    ADC(Mode.ZeroPageX);
                    break;
                case 0x6D:
                    ADC(Mode.Absolute);
                    break;
                case 0x7D:
                    ADC(Mode.AbsoluteX);
                    break;

                //SBC
                case 0xE9:
                    SBC(Mode.Immediate);
                    break;
                case 0xE5:
                    SBC(Mode.ZeroPage);
                    break;
                case 0xF5:
                    SBC(Mode.ZeroPageX);
                    break;
                case 0xED:
                    SBC(Mode.Absolute);
                    break;
                case 0xFD:
                    SBC(Mode.AbsoluteX);
                    break;

                //ROL
                case 0x2A:
                    ROL(Mode.Accumulator);
                    break;
                case 0x26:
                    ROL(Mode.ZeroPage);
                    break;
                case 0x36:
                    ROL(Mode.ZeroPageX);
                    break;
                case 0x2E:
                    ROL(Mode.Absolute);
                    break;
                case 0x3E:
                    ROL(Mode.AbsoluteX);
                    break;

                //Transfer
                case 0xAA:
                    Transfer(SelectedRegister.A, SelectedRegister.X);
                    break;
                case 0x8A:
                    Transfer(SelectedRegister.X, SelectedRegister.A);
                    break;
                case 0xA8:
                    Transfer(SelectedRegister.A, SelectedRegister.Y);
                    break;
                case 0x98:
                    Transfer(SelectedRegister.Y, SelectedRegister.A);
                    break;
                //Branch
                case 0x10:
                    Branch(!NegativeFlag);
                    break;
                case 0x30:
                    Branch(NegativeFlag);
                    break;
                case 0x50:
                    Branch(!OverflowFlag);
                    break;
                case 0x70:
                    Branch(OverflowFlag);
                    break;
                case 0x90:
                    Branch(!CarryFlag);
                    break;
                case 0xB0:
                    Branch(CarryFlag);
                    break;
                case 0xD0:
                    Branch(!ZeroFlag);
                    break;
                case 0xF0:
                    Branch(ZeroFlag);
                    break;

                //LSR
                case 0x4A:
                    LSR(Mode.Accumulator);
                    break;
                case 0x46:
                    LSR(Mode.ZeroPage);
                    break;
                case 0x56:
                    LSR(Mode.ZeroPageX);
                    break;
                case 0x4E:
                    LSR(Mode.Absolute);
                    break;
                case 0x5E:
                    LSR(Mode.AbsoluteX);
                    break;

                //LDA
                case 0xA9:
                    LoadRegister(SelectedRegister.A, Mode.Immediate);
                    break;
                case 0xA5:
                    LoadRegister(SelectedRegister.A, Mode.ZeroPage);
                    break;
                case 0xB5:
                    LoadRegister(SelectedRegister.A, Mode.ZeroPageX);
                    break;
                case 0xAD:
                    LoadRegister(SelectedRegister.A, Mode.Absolute);
                    break;
                case 0xBD:
                    LoadRegister(SelectedRegister.A, Mode.AbsoluteX);
                    break;
                case 0xB9:
                    LoadRegister(SelectedRegister.A, Mode.AbsoluteY);
                    break;
                case 0xA1:
                    LoadRegister(SelectedRegister.A, Mode.IndirectX);
                    break;
                case 0xB1:
                    LoadRegister(SelectedRegister.A, Mode.IndirectY);
                    break;

                //LDX
                case 0xA2:
                    LoadRegister(SelectedRegister.X, Mode.Immediate);
                    break;
                case 0xA6:
                    LoadRegister(SelectedRegister.X, Mode.ZeroPage);
                    break;
                case 0xB6:
                    LoadRegister(SelectedRegister.X, Mode.ZeroPageY);
                    break;
                case 0xAE:
                    LoadRegister(SelectedRegister.X, Mode.Absolute);
                    break;
                case 0xBE:
                    LoadRegister(SelectedRegister.X, Mode.AbsoluteY);
                    break;

                //LDY
                case 0xA0:
                    LoadRegister(SelectedRegister.Y, Mode.Immediate);
                    break;
                case 0xA4:
                    LoadRegister(SelectedRegister.Y, Mode.ZeroPage);
                    break;
                case 0xB4:
                    LoadRegister(SelectedRegister.Y, Mode.ZeroPageX);
                    break;
                case 0xAC:
                    LoadRegister(SelectedRegister.Y, Mode.Absolute);
                    break;
                case 0xBC:
                    LoadRegister(SelectedRegister.Y, Mode.AbsoluteX);
                    break;

                //STA
                case 0x85:
                    StoreRegister(SelectedRegister.A, Mode.ZeroPage);
                    break;
                case 0x95:
                    StoreRegister(SelectedRegister.A, Mode.ZeroPageX);
                    break;
                case 0x8D:
                    StoreRegister(SelectedRegister.A, Mode.Absolute);
                    break;
                case 0x9D:
                    StoreRegister(SelectedRegister.A, Mode.AbsoluteX);
                    break;
                case 0x99:
                    StoreRegister(SelectedRegister.A, Mode.AbsoluteY);
                    break;
                case 0x81:
                    StoreRegister(SelectedRegister.A, Mode.IndirectX);
                    break;
                case 0x91:
                    StoreRegister(SelectedRegister.A, Mode.IndirectY);
                    break;

                //STX
                case 0x86:
                    StoreRegister(SelectedRegister.X, Mode.ZeroPage);
                    break;
                case 0x96:
                    StoreRegister(SelectedRegister.X, Mode.ZeroPageY);
                    break;
                case 0x8E:
                    StoreRegister(SelectedRegister.X, Mode.Absolute);
                    break;

                //STY
                case 0x84:
                    StoreRegister(SelectedRegister.Y, Mode.ZeroPage);
                    break;
                case 0x94:
                    StoreRegister(SelectedRegister.Y, Mode.ZeroPageX);
                    break;
                case 0x8C:
                    StoreRegister(SelectedRegister.Y, Mode.Absolute);
                    break;

                //Flags
                case 0x18: //CLC
                    CarryFlag = false;
                    break;
                case 0x38: //SEC
                    CarryFlag = true;
                    break;
                case 0x58: //CLI
                    InterruptDisableFlag = false;
                    break;
                case 0x78: //SEI
                    InterruptDisableFlag = true;
                    break;
                case 0xB8: //CLV
                    OverflowFlag = false;
                    break;
                case 0xD8: //CLD
                    DecimalFlag = false;
                    break;
                case 0xF8: //SED
                    DecimalFlag = false;
                    break;

                //Compare
                case 0xC9:
                    Compare(SelectedRegister.A, Mode.Immediate);
                    break;
                case 0xC5:
                    Compare(SelectedRegister.A, Mode.ZeroPage);
                    break;
                case 0xD5:
                    Compare(SelectedRegister.A, Mode.ZeroPageX);
                    break;
                case 0xCD:
                    Compare(SelectedRegister.A, Mode.Absolute);
                    break;
                case 0xDD:
                    Compare(SelectedRegister.A, Mode.AbsoluteX);
                    break;
                case 0xD9:
                    Compare(SelectedRegister.A, Mode.AbsoluteY);
                    break;
                case 0xC1:
                    Compare(SelectedRegister.A, Mode.IndirectX);
                    break;
                case 0xD1:
                    Compare(SelectedRegister.A, Mode.IndirectY);
                    break;


                //Increase/Decrease
                case 0xCA:
                    Decrease(SelectedRegister.X);
                    break;
                case 0x88:
                    Decrease(SelectedRegister.Y);
                    break;
                case 0xE8:
                    Increase(SelectedRegister.X);
                    break;
                case 0xC8:
                    Increase(SelectedRegister.Y);
                    break;

                //Call
                case 0x20:
                    JumpSubroutine();
                    break;
                case 0x40:
                    rFlags = Pop8();
                    ReturnFromSubroutine();
                    break;
                case 0x60:
                    ReturnFromSubroutine();
                    break;

                //BIT
                case 0x24:
                    Bit(Mode.ZeroPage);
                    break;
                case 0x2C:
                    Bit(Mode.Absolute);
                    break;

                //ASL
                case 0x0A:
                    ASL(Mode.Accumulator);
                    break;
                case 0x06:
                    ASL(Mode.ZeroPage);
                    break;
                case 0x16:
                    ASL(Mode.ZeroPageX);
                    break;
                case 0x0E:
                    ASL(Mode.Absolute);
                    break;
                case 0x1E:
                    ASL(Mode.AbsoluteX);
                    break;

                //EOR
                case 0x49:
                    EOR(Mode.Immediate);
                    break;
                case 0x45:
                    EOR(Mode.ZeroPage);
                    break;
                case 0x55:
                    EOR(Mode.ZeroPageX);
                    break;
                case 0x4D:
                    EOR(Mode.Absolute);
                    break;
                case 0x5D:
                    EOR(Mode.AbsoluteX);
                    break;
                case 0x59:
                    EOR(Mode.AbsoluteY);
                    break;
                case 0x41:
                    EOR(Mode.IndirectX);
                    break;
                case 0x51:
                    EOR(Mode.IndirectY);
                    break;

                //CPY
                case 0xC0:
                    Compare(SelectedRegister.Y, Mode.Immediate);
                    break;
                case 0xC4:
                    Compare(SelectedRegister.Y, Mode.ZeroPage);
                    break;
                case 0xCC:
                    Compare(SelectedRegister.Y, Mode.Absolute);
                    break;

                //Stack
                case 0x9A:
                    Transfer(SelectedRegister.X, SelectedRegister.SP);
                    break;
                case 0xBA:
                    Transfer(SelectedRegister.SP, SelectedRegister.X);
                    break;
                case 0x48:
                    Push8(rA);
                    break;
                case 0x68:
                    rA = Pop8();
                    break;
                case 0x08:
                    Push8(rFlags);
                    break;
                case 0x28:
                    rFlags = Pop8();
                    break;


                //AND
                case 0x29:
                    AND(Mode.Immediate);
                    break;
                case 0x25:
                    AND(Mode.ZeroPage);
                    break;
                case 0x35:
                    AND(Mode.ZeroPageX);
                    break;
                case 0x2D:
                    AND(Mode.Absolute);
                    break;
                case 0x3D:
                    AND(Mode.AbsoluteX);
                    break;
                case 0x39:
                    AND(Mode.AbsoluteY);
                    break;
                case 0x21:
                    AND(Mode.IndirectX);
                    break;
                case 0x31:
                    AND(Mode.IndirectY);
                    break;

                //ORA
                case 0x09:
                    ORA(Mode.Immediate);
                    break;
                case 0x05:
                    ORA(Mode.ZeroPage);
                    break;
                case 0x15:
                    ORA(Mode.ZeroPageX);
                    break;
                case 0x0D:
                    ORA(Mode.Absolute);
                    break;
                case 0x1D:
                    ORA(Mode.AbsoluteX);
                    break;
                case 0x19:
                    ORA(Mode.AbsoluteY);
                    break;
                case 0x01:
                    ORA(Mode.IndirectX);
                    break;
                case 0x11:
                    ORA(Mode.IndirectY);
                    break;

                //NOP/BRK
                case 0xEA://NOP
                    break;
                case 0x00:
                    Debug.WriteLine("BRK");
                    break;


                //Crash on illegal
                default:
                    Crash("Illegal opcode");
                    break;
            }
        }

        public void JMP(Mode mode)
        {
            ushort address = getWordByMode(mode);

            JumpTo(address);
        }

        private ushort getWordByMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Absolute:
                    return ReadMemory16(rPC);
                case Mode.Indirect:
                    return ReadMemory16(ReadMemory16(rPC));
                default:
                    throw new NotImplementedException("Unknown mode");
            }
        }

        public void LSR(Mode mode)
        {
            ushort _pc = rPC;
            byte data = getByteByMode(mode);
            rPC = _pc;
            setByteByMode(mode, (byte)(data >> 1));
            CarryFlag = (data & 1) > 0;
        }
        public void ADC(Mode mode)
        {
            byte aCopy = rA, data = getByteByMode(mode);
            if (DecimalFlag) throw new NotImplementedException("Decimal-mode");

            uint tmp = (uint)(rA + data + (CarryFlag ? 1 : 0));
            rA = (byte)(tmp & 0xFF);

            CheckC((int)tmp);
            CheckSZ((int)rA);
            OverflowFlag = (((aCopy ^ rA) & ~(aCopy ^ data)) & 0x80) > 0;
        }
        public void SBC(Mode mode)
        {
            byte data = getByteByMode(mode);
            uint tmp, aCopy = rA;
            if (DecimalFlag) throw new NotImplementedException("Decimal-mode");

            tmp = (uint)(aCopy - data - (CarryFlag ? 0 : 1));
            rA = (byte)(tmp & 0xFF);

            CheckBorrow((int)tmp);
            CheckSZ(rA);
            OverflowFlag = ((aCopy ^ data) & (aCopy ^ rA) & 0x80)>0;
        }

        public void CheckBorrow(int data)
        {
            CarryFlag = (data & 0x100) == 0;
        }
        public void ROL(Mode mode)
        {
            ushort _pc = rPC;
            byte data = getByteByMode(mode), data2 = (byte)(!CarryFlag ? (data << 1) : ((data << 1) + 1));
            rPC = _pc;
            setByteByMode(mode, data2);
            CarryFlag = data >= 0x80;
            CheckSZ(data2);
        }

        public void ASL(Mode mode)
        {
            ushort _pc = rPC;
            byte data = getByteByMode(mode);
            rPC = _pc;
            setByteByMode(mode, (byte)(data << 1));

            CarryFlag = data >= 0x80;
        }

        public void Transfer(SelectedRegister selectedRegister1, SelectedRegister selectedRegister2)
        {
            byte data = getRegister(selectedRegister1);

            CheckSZ(data);

            setRegister(selectedRegister2, data);
        }

        public void Reset()
        {
            rA = rX = rY = rFlags = 0;
            rSP = 0xFF;
            rPC = ReadMemory16(0xFFFC);
            resetNeeded = false;
        }



        public void ReturnFromSubroutine()
        {
            rPC = Pop16();
            rPC++;
        }

        public void JumpSubroutine()
        {
            Push16((ushort)(rPC + 1));
            JumpTo(ReadMemory16(rPC));
        }


        public void LoadRegister(SelectedRegister selectedRegister, Mode mode)
        {
            byte data =  getByteByMode(mode);

            CheckSZ(data);

            setRegister(selectedRegister, data);
        }

        public void setRegister(SelectedRegister selectedRegister, byte data)
        {
            switch (selectedRegister)
            {
                case SelectedRegister.A:
                    rA = data;
                    break;
                case SelectedRegister.X:
                    rX = data;
                    break;
                case SelectedRegister.Y:
                    rY = data;
                    break;
                case SelectedRegister.SP:
                    rSP = data;
                    break;
                default: throw new NotImplementedException("Unknown register");
            }
        }

        public byte getByteByMode(Mode mode)
        {
            byte data = 0;
            switch (mode)
            {
                case Mode.Accumulator:
                    data = rA;
                    break;
                case Mode.IndirectX:
                    data = ReadMemory8(ReadMemory16((byte)(ReadMemory8(rPC) + rX)));
                    JumpOffset(1);
                    break;
                case Mode.IndirectY:
                    data = ReadMemory8((ushort)(ReadMemory16(ReadMemory8(rPC)) + rY));
                    JumpOffset(1);
                    break;
                case Mode.Immediate:
                    data = ReadMemory8(rPC);
                    JumpOffset(1);
                    break;
                case Mode.ZeroPage:
                    data = ReadMemory8(ReadMemory8(rPC));
                    JumpOffset(1);
                    break;
                case Mode.ZeroPageX:
                    data = ReadMemory8((ushort)(ReadMemory8(rPC) + rX));
                    JumpOffset(1);
                    break;
                case Mode.Absolute:
                    data = ReadMemory8(ReadMemory16(rPC));
                    JumpOffset(2);
                    break;
                case Mode.AbsoluteX:
                    data = ReadMemory8((ushort)(ReadMemory16(rPC) + rX));
                    JumpOffset(2);
                    break;
                case Mode.AbsoluteY:
                    data = ReadMemory8((ushort)(ReadMemory16(rPC) + rY));
                    JumpOffset(2);
                    break;
                default: throw new NotImplementedException("Unknown mode: "+mode.ToString());
            }
            return data;
        }

        
        public void StoreRegister(SelectedRegister selectedRegister, Mode mode)
        {
            byte data = getRegister(selectedRegister);
            setByteByMode(mode, data);
        }

        public void setByteByMode(Mode mode, byte data)
        {
            switch (mode)
            {
                case Mode.Accumulator:
                    rA = data;
                    break;
                case Mode.ZeroPage:
                    WriteMemory8(ReadMemory8(rPC), data);
                    JumpOffset(1);
                    break;
                case Mode.ZeroPageX:
                    WriteMemory8((ushort)(ReadMemory8(rPC) + rX), data);
                    JumpOffset(1);
                    break;
                case Mode.Absolute:
                    WriteMemory8(ReadMemory16(rPC), data);
                    JumpOffset(2);
                    break;
                case Mode.AbsoluteX:
                    WriteMemory8((ushort)(ReadMemory16(rPC) + rX), data);
                    JumpOffset(2);
                    break;
                case Mode.AbsoluteY:
                    WriteMemory8((ushort)(ReadMemory16(rPC) + rY), data);
                    JumpOffset(2);
                    break;
                case Mode.IndirectX:
                    WriteMemory8(ReadMemory16((byte)(ReadMemory8(rPC) + rX)), data);
                    JumpOffset(1);
                    break;
                case Mode.IndirectY:
                    WriteMemory8((ushort)(ReadMemory16(ReadMemory8(rPC)) + rY), data);
                    JumpOffset(1);
                    break;
                default: throw new NotImplementedException("Unknown mode");
            }
        }

        public byte getRegister(SelectedRegister selectedRegister)
        {
            byte data = 0;
            switch (selectedRegister)
            {
                case SelectedRegister.A:
                    data = rA;
                    break;
                case SelectedRegister.X:
                    data = rX;
                    break;
                case SelectedRegister.Y:
                    data = rY;
                    break;
                case SelectedRegister.SP:
                    data = rSP;
                    break;
                default: throw new NotImplementedException("Unknown register");
            }
            return data;
        }
        public void Compare(SelectedRegister selectedRegister, Mode mode)
        {
            byte register = getRegister(selectedRegister), data = getByteByMode(mode);
            CarryFlag = register >= data;
            ZeroFlag = register == data;
            CheckN(rA);
        }
        public void ORA(Mode mode)
        {
            LogicalOp(mode, (d) =>
            {
                return (byte)(rA | d);
            });
        }
        public void AND(Mode mode)
        {
            LogicalOp(mode, (d) =>
            {
                return (byte)(rA & d);
            });
        }
        public void EOR(Mode mode)
        {
            LogicalOp(mode, (d) =>
            {
                return (byte)(rA ^ d);
            });
        }
        void LogicalOp(Mode mode, Func<byte, byte> exec)
        {
            var data = exec(getByteByMode(mode));
            CheckSZ(data);
            rA = data;
        }

        private void CheckSZ(int data)
        {
            CheckN(data);
            CheckZ(data);
        }

        public void CheckN(int data)
        {
            NegativeFlag = (data & 0xFF) >= 0x80;
        }

        public void CheckZ(int data)
        {
            ZeroFlag = (data & 0xFF) == 0;
        }

        public void CheckC(int data)
        {
            CarryFlag = (data & 0x100) > 0;
        }
        public byte ReadMemory8(ushort p)
        {
            return machine.ReadMemory(p);
        }
        public ushort ReadMemory16(ushort p)
        {
            return (ushort)((ReadMemory8((ushort)(p + 1)) << 8) + ReadMemory8(p));
        }
        public void WriteMemory8(ushort p, byte data)
        {
            machine.WriteMemory(p,data);
        }
        public void WriteMemory16(ushort p, ushort data)
        {
            WriteMemory8(p, (byte)(data & 0xFF));
            WriteMemory8((ushort)(p+1), (byte)(data >> 8));
        }

        private void JumpOffset(int p)
        {
            JumpTo((ushort)(rPC + p));
        }
        public void Branch(bool p)
        {
            int offset = ReadMemory8(rPC++);
            offset = offset > 0 ? offset : (int)(offset+0xffff0000);
            if (p) JumpOffset((sbyte)offset);
            //Jumped = true;
            
        }


        public void Push8(byte data)
        {
            WriteMemory8((ushort)(0x100 + rSP), data);
            if (rSP > 0) rSP--;
        }
        public void Push16(ushort data)
        {
            Push8((byte)(data >> 8));
            Push8((byte)(data & 0xFF));
        }
        public byte Pop8()
        {
            if (rSP < 0xFF) rSP++;
            byte r = ReadMemory8((ushort)(0x100 + rSP));
            return r;
        }
        public ushort Pop16()
        {
            ushort r = (ushort)(Pop8() + ((ushort)(Pop8() << 8)));

            return r;
        }

        public void JumpTo(ushort address)
        {
            //Jumped = true;
            rPC = address;
        }
        public void Increase(SelectedRegister selectedRegister)
        {
            IncreaseDecrease(selectedRegister, 1);
        }
        public void Decrease(SelectedRegister selectedRegister)
        {
            IncreaseDecrease(selectedRegister, -1);
        }

        private void IncreaseDecrease(SelectedRegister selectedRegister, int p)
        {
            byte data = 0;

            data = getRegister(selectedRegister);
            setRegister(selectedRegister, (byte)(data + p));
            CheckSZ(data + p);
        }
        public void IncreaseMemory(Mode mode)
        {
            IncreaseDecreaseMemory(mode, 1);
        }
        public void DecreaseMemory(Mode mode)
        {
            IncreaseDecreaseMemory(mode, -1);
        }

        public void IncreaseDecreaseMemory(Mode mode, int p)
        {
            byte data = 0;
            var _pc = rPC;
            data = getByteByMode(mode);
            rPC = _pc;
            setByteByMode(mode, (byte)(data + p));
            CheckSZ(data + p);
        }

        public void Bit(Mode mode)
        {
            byte d = getByteByMode(mode);

            CheckZ(rA & d);
            NegativeFlag = (d & (1 << 7)) > 0;
            OverflowFlag = (d & (1 << 6)) > 0;
        }
        private void Crash(string p)
        {
            stopped = true;
            Debug.Print("CPU crashed: " + p);
        }

        public void Dump()
        {
            if (rPC >= 0xE000 && rPC < 0xF000) Debug.Print("A={0:X02}|X={1:X02}|Y={2:X02}|SP={3:X02}|FLAG={4:X02}|PC={5:X04}|OP={6:X02} -- {7}", rA, rX, rY, rSP, rFlags, rPC, ReadMemory8(rPC), basicListing.Where((str) => { return str.StartsWith(toHex(rPC)); }).FirstOrDefault().Substring(17));
            else Debug.Print("A={0:X02}|X={1:X02}|Y={2:X02}|SP={3:X02}|FLAG={4:X02}|PC={5:X04}|OP={6:X02}", rA, rX, rY, rSP, rFlags, rPC, ReadMemory8(rPC));
        }

        public static string toHex(ushort us)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0:x04}", us);
            return sb.ToString();
        }


        public byte rA = 0, rX = 0, rY = 0, rSP = 0xFF;
        public ushort rPC;
        internal bool stopped=false;
        //private bool Jumped=false;
        private bool resetNeeded=true;

        public byte rFlags
        {
            get
            {
                byte r = 1 << 5;

                if (NegativeFlag) r += (byte)(1 << 7);
                if (OverflowFlag) r += (byte)(1 << 6);


                if (BreakFlag) r += (byte)(1 << 4);
                if (DecimalFlag) r += (byte)(1 << 3);
                if (InterruptDisableFlag) r += (byte)(1 << 2);
                if (ZeroFlag) r += (byte)(1 << 1);
                if (CarryFlag) r += (byte)(1 << 0);

                return r;
            }
            set
            {
                NegativeFlag = (value & 0x80) > 0;
                OverflowFlag = (value & 0x40) > 0;

                BreakFlag = (value & 0x10) > 0;
                DecimalFlag = (value & 0x8) > 0;
                InterruptDisableFlag = (value & 0x4) > 0;
                ZeroFlag = (value & 0x2) > 0;
                CarryFlag = (value & 0x1) > 0;
            }
        }

        public bool CarryFlag { get; set; }

        public bool InterruptDisableFlag { get; set; }

        public bool OverflowFlag { get; set; }

        public bool DecimalFlag { get; set; }

        public bool ZeroFlag { get; set; }

        public bool NegativeFlag { get; set; }

        public bool BreakFlag { get; set; }
    }

    public enum Mode
    {
        Immediate,
        AbsoluteX,
        Absolute,
        ZeroPageX,
        ZeroPage,
        AbsoluteY,
        IndirectX,
        IndirectY,
        ZeroPageY,
        Accumulator,
        Indirect
    }

    public enum SelectedRegister
    {
        A, X, Y,
        SP
    }
}
