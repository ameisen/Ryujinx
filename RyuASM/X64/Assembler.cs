using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace RyuASM.X64
{
    public class Assembler
    {
        private const uint BadOp = InstructionTable.BadOp;
        private const int OpModRMBits = 24;

        private const byte RexPrefix = 0x40;
        private const byte RexWPrefix = 0x48;
        private const byte LockPrefix = 0xf0;

        private readonly Stream OutStream;
        private readonly ICapabilities Capabilities;

        public Assembler([NotNull] Stream stream, [NotNull] ICapabilities capabilities)
        {
            OutStream = stream;
            Capabilities = capabilities;
        }

        public void Add(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Add);
        }

        public void Addsd(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Addsd);
        }

        public void Addss(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Addss);
        }

        public void And(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.And);
        }

        public void Bsr(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Bsr);
        }

        public void Bswap(IOperand dest)
        {
            WriteInstruction(dest, null, dest.Type, Instruction.Bswap);
        }

        public void Call(IOperand dest)
        {
            WriteInstruction(dest, null, OperandType.None, Instruction.Call);
        }

        public void Cdq()
        {
            WriteByte(0x99);
        }

        public void Cmovcc(IOperand dest, IOperand source, OperandType type, Condition condition)
        {
            ref var info = ref Instruction.Cmovcc.GetInfo();

            WriteOpCode(dest, null, source, type, info.Flags, info.OpRRM | (uint)condition, rrm: true);
        }

        public void Cmp(IOperand src1, IOperand src2, OperandType type)
        {
            WriteInstruction(src1, src2, type, Instruction.Cmp);
        }

        public void Cqo()
        {
            WriteByte(0x48);
            WriteByte(0x99);
        }

        public void Cmpxchg(IMemoryOperand memOp, IOperand src)
        {
            WriteByte(LockPrefix);

            WriteInstruction(memOp, src, src.Type, Instruction.Cmpxchg);
        }

        public void Cmpxchg16b(IMemoryOperand memOp)
        {
            WriteByte(LockPrefix);

            WriteInstruction(memOp, null, OperandType.None, Instruction.Cmpxchg16b);
        }

        public void Comisd(IOperand src1, IOperand src2)
        {
            WriteInstruction(src1, null, src2, Instruction.Comisd);
        }

        public void Comiss(IOperand src1, IOperand src2)
        {
            WriteInstruction(src1, null, src2, Instruction.Comiss);
        }

        public void Cpuid()
        {
            WriteInstruction(null, null, OperandType.None, Instruction.Cpuid);
        }

        public void Cvtsd2ss(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Cvtsd2ss);
        }

        public void Cvtsi2sd(IOperand dest, IOperand src1, IOperand src2, OperandType type)
        {
            WriteInstruction(dest, src1, src2, Instruction.Cvtsi2sd, type);
        }

        public void Cvtsi2ss(IOperand dest, IOperand src1, IOperand src2, OperandType type)
        {
            WriteInstruction(dest, src1, src2, Instruction.Cvtsi2ss, type);
        }

        public void Cvtss2sd(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Cvtss2sd);
        }

        public void Div(IOperand source)
        {
            WriteInstruction(null, source, source.Type, Instruction.Div);
        }

        public void Divsd(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Divsd);
        }

        public void Divss(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Divss);
        }

        public void Idiv(IOperand source)
        {
            WriteInstruction(null, source, source.Type, Instruction.Idiv);
        }

        public void Imul(IOperand source)
        {
            WriteInstruction(null, source, source.Type, Instruction.Imul128);
        }

        public void Imul(IOperand dest, IOperand source, OperandType type)
        {
            if (!source.IsRegister)
            {
                throw new ArgumentException($"Invalid source operand kind \"{source.KindName}\".");
            }

            WriteInstruction(dest, source, type, Instruction.Imul);
        }

        public void Imul(IOperand dest, IOperand src1, IOperand src2, OperandType type)
        {
            ref var info = ref Instruction.Imul.GetInfo();

            if (!src2.IsConstant)
            {
                throw new ArgumentException($"Invalid source 2 operand kind \"{src2.KindName}\".");
            }

            if (IsImm8(src2.Value, src2.Type) && info.OpRMImm8 != BadOp)
            {
                WriteOpCode(dest, null, src1, type, info.Flags, info.OpRMImm8, rrm: true);

                WriteByte(src2.AsByte);
            }
            else if (IsImm32(src2.Value, src2.Type) && info.OpRMImm32 != BadOp)
            {
                WriteOpCode(dest, null, src1, type, info.Flags, info.OpRMImm32, rrm: true);

                WriteInt32(src2.AsInt32);
            }
            else
            {
                throw new ArgumentException($"Failed to encode constant 0x{src2.Value:X}.");
            }
        }

        public void Insertps(IOperand dest, IOperand src1, IOperand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, Instruction.Insertps);

            WriteByte(imm);
        }

        public void Jcc(Condition condition, long offset)
        {
            if (ConstFitsOnS8(offset))
            {
                WriteByte((byte)(0x70 | (int)condition));

                WriteByte((byte)offset);
            }
            else if (ConstFitsOnS32(offset))
            {
                WriteByte(0x0f);
                WriteByte((byte)(0x80 | (int)condition));

                WriteInt32((int)offset);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        public void Jmp(long offset)
        {
            if (ConstFitsOnS8(offset))
            {
                WriteByte(0xeb);

                WriteByte((byte)offset);
            }
            else if (ConstFitsOnS32(offset))
            {
                WriteByte(0xe9);

                WriteInt32((int)offset);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        public void Jmp(IOperand dest)
        {
            WriteInstruction(dest, null, OperandType.None, Instruction.Jmp);
        }

        public void Lea(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Lea);
        }

        public void Mov(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Mov);
        }

        public void Mov16(IOperand dest, IOperand source)
        {
            WriteInstruction(dest, source, OperandType.None, Instruction.Mov16);
        }

        public void Mov8(IOperand dest, IOperand source)
        {
            WriteInstruction(dest, source, OperandType.None, Instruction.Mov8);
        }

        public void Movd(IOperand dest, IOperand source)
        {
            ref var info = ref Instruction.Movd.GetInfo();

            if (source.Type.IsIntegral() || source.IsMemory)
            {
                WriteOpCode(dest, null, source, OperandType.None, info.Flags, info.OpRRM, rrm: true);
            }
            else
            {
                WriteOpCode(dest, null, source, OperandType.None, info.Flags, info.OpRMR);
            }
        }

        public void Movdqu(IOperand dest, IOperand source)
        {
            WriteInstruction(dest, null, source, Instruction.Movdqu);
        }

        public void Movhlps(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Movhlps);
        }

        public void Movlhps(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Movlhps);
        }

        public void Movq(IOperand dest, IOperand source)
        {
            ref var info = ref Instruction.Movd.GetInfo();

            var flags = info.Flags | InstructionFlags.RexW;

            if (source.Type.IsIntegral() || source.IsMemory)
            {
                WriteOpCode(dest, null, source, OperandType.None, flags, info.OpRRM, rrm: true);
            }
            else if (dest.Type.IsIntegral() || dest.IsMemory)
            {
                WriteOpCode(dest, null, source, OperandType.None, flags, info.OpRMR);
            }
            else
            {
                WriteInstruction(dest, source, OperandType.None, Instruction.Movq);
            }
        }

        public void Movsd(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Movsd);
        }

        public void Movss(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Movss);
        }

        public void Movsx16(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Movsx16);
        }

        public void Movsx32(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Movsx32);
        }

        public void Movsx8(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Movsx8);
        }

        public void Movzx16(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Movzx16);
        }

        public void Movzx8(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Movzx8);
        }

        public void Mul(IOperand source)
        {
            WriteInstruction(null, source, source.Type, Instruction.Mul128);
        }

        public void Mulsd(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Mulsd);
        }

        public void Mulss(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Mulss);
        }

        public void Neg(IOperand dest)
        {
            WriteInstruction(dest, null, dest.Type, Instruction.Neg);
        }

        public void Not(IOperand dest)
        {
            WriteInstruction(dest, null, dest.Type, Instruction.Not);
        }

        public void Or(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Or);
        }

        public void Pcmpeqw(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Pcmpeqw);
        }

        public void Pextrb(IOperand dest, IOperand source, byte imm)
        {
            WriteInstruction(dest, null, source, Instruction.Pextrb);

            WriteByte(imm);
        }

        public void Pextrd(IOperand dest, IOperand source, byte imm)
        {
            WriteInstruction(dest, null, source, Instruction.Pextrd);

            WriteByte(imm);
        }

        public void Pextrq(IOperand dest, IOperand source, byte imm)
        {
            WriteInstruction(dest, null, source, Instruction.Pextrq);

            WriteByte(imm);
        }

        public void Pextrw(IOperand dest, IOperand source, byte imm)
        {
            WriteInstruction(dest, null, source, Instruction.Pextrw);

            WriteByte(imm);
        }

        public void Pinsrb(IOperand dest, IOperand src1, IOperand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, Instruction.Pinsrb);

            WriteByte(imm);
        }

        public void Pinsrd(IOperand dest, IOperand src1, IOperand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, Instruction.Pinsrd);

            WriteByte(imm);
        }

        public void Pinsrq(IOperand dest, IOperand src1, IOperand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, Instruction.Pinsrq);

            WriteByte(imm);
        }

        public void Pinsrw(IOperand dest, IOperand src1, IOperand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, Instruction.Pinsrw);

            WriteByte(imm);
        }

        public void Pop(IOperand dest)
        {
            if (dest.IsRegister)
            {
                WriteCompactInst(dest, 0x58);
            }
            else
            {
                WriteInstruction(dest, null, dest.Type, Instruction.Pop);
            }
        }

        public void Popcnt(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Popcnt);
        }

        public void Pshufd(IOperand dest, IOperand source, byte imm)
        {
            WriteInstruction(dest, null, source, Instruction.Pshufd);

            WriteByte(imm);
        }

        public void Push(IOperand source)
        {
            if (source.IsRegister)
            {
                WriteCompactInst(source, 0x50);
            }
            else
            {
                WriteInstruction(null, source, source.Type, Instruction.Push);
            }
        }

        public void Return()
        {
            WriteByte(0xc3);
        }

        public void Ror(IOperand dest, IOperand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, Instruction.Ror);
        }

        public void Sar(IOperand dest, IOperand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, Instruction.Sar);
        }

        public void Shl(IOperand dest, IOperand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, Instruction.Shl);
        }

        public void Shr(IOperand dest, IOperand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, Instruction.Shr);
        }

        public void Setcc(IOperand dest, Condition condition)
        {
            ref var info = ref Instruction.Setcc.GetInfo();

            WriteOpCode(dest, null, null, OperandType.None, info.Flags, info.OpRRM | (uint)condition);
        }

        public void Sub(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Sub);
        }

        public void Subsd(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Subsd);
        }

        public void Subss(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Subss);
        }

        public void Test(IOperand src1, IOperand src2, OperandType type)
        {
            WriteInstruction(src1, src2, type, Instruction.Test);
        }

        public void Xor(IOperand dest, IOperand source, OperandType type)
        {
            WriteInstruction(dest, source, type, Instruction.Xor);
        }

        public void Xorps(IOperand dest, IOperand src1, IOperand src2)
        {
            WriteInstruction(dest, src1, src2, Instruction.Xorps);
        }

        public void WriteInstruction(
            Instruction inst,
            IOperand dest,
            IOperand source,
            OperandType type = OperandType.None)
        {
            WriteInstruction(dest, null, source, inst, type);
        }

        public void WriteInstruction(Instruction inst, IOperand dest, IOperand src1, IOperand src2)
        {
            if (src2.IsConstant)
            {
                WriteInstruction(src1, dest, src2, inst);
            }
            else
            {
                WriteInstruction(dest, src1, src2, inst);
            }
        }

        public void WriteInstruction(
            Instruction inst,
            IOperand dest,
            IOperand src1,
            IOperand src2,
            OperandType type)
        {
            WriteInstruction(dest, src1, src2, inst, type);
        }

        public void WriteInstruction(Instruction inst, IOperand dest, IOperand source, byte imm)
        {
            WriteInstruction(dest, null, source, inst);

            WriteByte(imm);
        }

        public void WriteInstruction(
            Instruction inst,
            IOperand dest,
            IOperand src1,
            IOperand src2,
            IOperand src3)
        {
            // 3+ operands can only be encoded with the VEX encoding scheme.
            Debug.Assert(Capabilities.VexEncoding);

            WriteInstruction(dest, src1, src2, inst);

            WriteByte((byte)(src3.AsByte << 4));
        }

        public void WriteInstruction(
            Instruction inst,
            IOperand dest,
            IOperand src1,
            IOperand src2,
            byte imm)
        {
            WriteInstruction(dest, src1, src2, inst);

            WriteByte(imm);
        }

        private void WriteShiftInst(IOperand dest, IOperand source, OperandType type, Instruction inst)
        {
            if (source.IsRegister)
            {
                var shiftReg = source.Register;

                Debug.Assert(
                    shiftReg == Register.C,
                    $"Invalid shift register \"{shiftReg}\"."
                );

                source = null;
            }

            WriteInstruction(dest, source, type, inst);
        }

        private void WriteInstruction(IOperand dest, IOperand source, OperandType type, Instruction inst)
        {
            ref var info = ref inst.GetInfo();

            if (source != null)
            {
                if (source.IsConstant)
                {
                    ulong imm = source.Value;

                    if (inst == Instruction.Mov8)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm8);

                        WriteByte((byte)imm);
                    }
                    else if (inst == Instruction.Mov16)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm32);

                        WriteInt16((short)imm);
                    }
                    else if (IsImm8(imm, type) && info.OpRMImm8 != BadOp)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm8);

                        WriteByte((byte)imm);
                    }
                    else if (IsImm32(imm, type) && info.OpRMImm32 != BadOp)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm32);

                        WriteInt32((int)imm);
                    }
                    else if (dest?.IsRegister == true && info.OpRImm64 != BadOp)
                    {
                        uint rexPrefix = GetRexPrefix(dest, source, type, rrm: false);

                        if (rexPrefix != 0)
                        {
                            WriteByte((byte)rexPrefix);
                        }

                        WriteByte((byte)(info.OpRImm64 + (dest.RegisterIndex & 0b111)));

                        WriteUInt64(imm);
                    }
                    else
                    {
                        throw new ArgumentException($"Failed to encode constant 0x{imm:X}.");
                    }
                }
                else if (source.IsRegister && info.OpRMR != BadOp)
                {
                    WriteOpCode(dest, null, source, type, info.Flags, info.OpRMR);
                }
                else if (info.OpRRM != BadOp)
                {
                    WriteOpCode(dest, null, source, type, info.Flags, info.OpRRM, rrm: true);
                }
                else
                {
                    throw new ArgumentException($"Invalid source operand kind \"{source.KindName}\".");
                }
            }
            else if (info.OpRRM != BadOp)
            {
                WriteOpCode(dest, null, source, type, info.Flags, info.OpRRM, rrm: true);
            }
            else if (info.OpRMR != BadOp)
            {
                WriteOpCode(dest, null, source, type, info.Flags, info.OpRMR);
            }
            else
            {
                throw new ArgumentNullException(nameof(source));
            }
        }

        private void WriteInstruction(
            IOperand dest,
            IOperand src1,
            IOperand src2,
            Instruction inst,
            OperandType type = OperandType.None)
        {
            ref var info = ref inst.GetInfo();

            if (src2 != null)
            {
                if (src2.IsConstant)
                {
                    ulong imm = src2.Value;

                    if ((byte)imm == imm && info.OpRMImm8 != BadOp)
                    {
                        WriteOpCode(dest, src1, null, type, info.Flags, info.OpRMImm8);

                        WriteByte((byte)imm);
                    }
                    else
                    {
                        throw new ArgumentException($"Failed to encode constant 0x{imm:X}.");
                    }
                }
                else if (src2.IsRegister && info.OpRMR != BadOp)
                {
                    WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRMR);
                }
                else if (info.OpRRM != BadOp)
                {
                    WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRRM, rrm: true);
                }
                else
                {
                    throw new ArgumentException($"Invalid source operand kind \"{src2.KindName}\".");
                }
            }
            else if (info.OpRRM != BadOp)
            {
                WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRRM, rrm: true);
            }
            else if (info.OpRMR != BadOp)
            {
                WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRMR);
            }
            else
            {
                throw new ArgumentNullException(nameof(src2));
            }
        }

        private void WriteOpCode(
            IOperand dest,
            IOperand src1,
            IOperand src2,
            OperandType type,
            InstructionFlags flags,
            uint opCode,
            bool rrm = false)
        {
            uint rexPrefix = GetRexPrefix(dest, src2, type, rrm);

            if ((flags & InstructionFlags.RexW) != 0)
            {
                rexPrefix |= RexWPrefix;
            }

            uint modRM = (opCode >> OpModRMBits) << 3;

            IMemoryOperand memOp = null;

            if (dest != null)
            {
                if (dest.IsRegister)
                {
                    var regIndex = dest.RegisterIndex;

                    modRM |= (regIndex & 0b111) << (rrm ? 3 : 0);

                    if ((flags & InstructionFlags.Reg8Dest) != 0 && regIndex >= (uint)Register.R4)
                    {
                        rexPrefix |= RexPrefix;
                    }
                }
                else if (dest.IsMemory)
                {
                    memOp = dest as IMemoryOperand;
                }
                else
                {
                    throw new ArgumentException($"Invalid destination operand kind \"{dest.KindName}\".");
                }
            }

            if (src2 != null)
            {
                if (src2.IsRegister)
                {
                    var regIndex = src2.RegisterIndex;

                    modRM |= (regIndex & 0b111) << (rrm ? 0 : 3);

                    if ((flags & InstructionFlags.Reg8Src) != 0 && regIndex >= 4)
                    {
                        rexPrefix |= RexPrefix;
                    }
                }
                else if (src2.IsMemory && memOp == null)
                {
                    memOp = src2 as IMemoryOperand;
                }
                else
                {
                    throw new ArgumentException("Invalid source operand kind \"" + src2.KindName + "\".");
                }
            }

            bool needsSibByte = false;
            bool needsDisplacement = false;

            uint sib = 0;

            if (memOp != null)
            {
                // Either source or destination is a memory operand.
                var baseReg = memOp.BaseAddress.Register;

                var baseRegLow = (Register)((uint)baseReg & 0b111);

                needsSibByte = memOp.Index != null || baseRegLow == Register.Sp;
                needsDisplacement = memOp.Displacement != 0 || baseRegLow == Register.Bp;

                if (needsDisplacement)
                {
                    if (ConstFitsOnS8(memOp.Displacement))
                    {
                        modRM |= 0x40;
                    }
                    else /* if (ConstFitsOnS32(memOp.Displacement)) */
                    {
                        modRM |= 0x80;
                    }
                }

                if (baseReg >= Register.R8)
                {
                    Debug.Assert(baseReg <= Register.RMax);

                    rexPrefix |= RexPrefix | ((uint)baseReg >> 3);
                }

                if (needsSibByte)
                {
                    sib = (uint)baseRegLow;

                    if (memOp.Index != null)
                    {
                        var indexReg = memOp.Index.Register;

                        Debug.Assert(indexReg != Register.Sp, "Using RSP as index register on the memory operand is not allowed.");

                        if (indexReg >= Register.R8)
                        {
                            Debug.Assert(indexReg <= Register.RMax);

                            rexPrefix |= RexPrefix | ((uint)indexReg >> 3) << 1;
                        }

                        sib |= ((uint)indexReg & 0b111) << 3;
                    }
                    else
                    {
                        sib |= 0b100 << 3;
                    }

                    sib |= (uint)memOp.Shift << 6;

                    modRM |= 0b100;
                }
                else
                {
                    modRM |= (uint)baseRegLow;
                }
            }
            else
            {
                // Source and destination are registers.
                modRM |= 0xc0;
            }

            Debug.Assert(opCode != BadOp, "Invalid opcode value.");

            if ((flags & InstructionFlags.Vex) != 0 && Capabilities.VexEncoding)
            {
                uint vexByte2 = (uint)(flags & InstructionFlags.PrefixMask) >> (int)InstructionFlags.PrefixBit;

                if (src1 != null)
                {
                    vexByte2 |= (src1.RegisterIndex ^ 0xf) << 3;
                }
                else
                {
                    vexByte2 |= 0b1111 << 3;
                }

                ushort opCodeHigh = (ushort)(opCode >> 8);

                if ((rexPrefix & 0b1011) == 0 && opCodeHigh == 0xf)
                {
                    // Two-byte form.
                    WriteByte(0xc5);

                    vexByte2 |= (~rexPrefix & 4) << 5;

                    WriteByte((byte)vexByte2);
                }
                else
                {
                    // Three-byte form.
                    WriteByte(0xc4);

                    uint vexByte1 = (~rexPrefix & 7) << 5;

                    switch (opCodeHigh)
                    {
                        case 0xf: vexByte1 |= 1; break;
                        case 0xf38: vexByte1 |= 2; break;
                        case 0xf3a: vexByte1 |= 3; break;

                        default: Debug.Fail($"Failed to VEX encode opcode 0x{opCode:X}."); break;
                    }

                    vexByte2 |= (rexPrefix & 8) << 4;

                    WriteByte((byte)vexByte1);
                    WriteByte((byte)vexByte2);
                }

                opCode &= 0xff;
            }
            else
            {
                switch (flags & InstructionFlags.PrefixMask)
                {
                    case InstructionFlags.Prefix66: WriteByte(0x66); break;
                    case InstructionFlags.PrefixF2: WriteByte(0xf2); break;
                    case InstructionFlags.PrefixF3: WriteByte(0xf3); break;
                }

                if (rexPrefix != 0)
                {
                    WriteByte((byte)rexPrefix);
                }
            }

            if (dest != null && (flags & InstructionFlags.RegCoded) != 0)
            {
                opCode += dest.RegisterIndex & 7;
            }

            if ((opCode & 0xff0000) != 0)
            {
                WriteByte((byte)(opCode >> 16));
            }

            if ((opCode & 0xff00) != 0)
            {
                WriteByte((byte)(opCode >> 8));
            }

            WriteByte((byte)opCode);

            if ((flags & InstructionFlags.RegCoded) == 0)
            {
                WriteByte((byte)modRM);

                if (needsSibByte)
                {
                    WriteByte((byte)sib);
                }

                if (needsDisplacement)
                {
                    if (ConstFitsOnS8(memOp.Displacement))
                    {
                        WriteByte((byte)memOp.Displacement);
                    }
                    else /* if (ConstFitsOnS32(memOp.Displacement)) */
                    {
                        WriteInt32(memOp.Displacement);
                    }
                }
            }
        }

        private void WriteCompactInst(IOperand operand, uint opCode)
        {
            uint regIndex = operand.RegisterIndex;

            if (regIndex >= (uint)Register.R8)
            {
                WriteByte(0x41);
            }

            WriteByte((byte)(opCode + (regIndex & 0b111)));
        }

        private static uint GetRexPrefix(IOperand dest, IOperand source, OperandType type, bool rrm)
        {
            uint rexPrefix = 0;

            if (Is64Bits(type))
            {
                rexPrefix = RexWPrefix;
            }

            void SetRegisterHighBit(Register reg, int bit)
            {
                if (reg >= Register.R8)
                {
                    rexPrefix |= RexPrefix | ((uint)reg >> 3) << bit;
                }
            }

            if (dest?.IsRegister == true)
            {
                SetRegisterHighBit(dest.Register, rrm ? 2 : 0);
            }

            if (source?.IsRegister == true)
            {
                SetRegisterHighBit(source.Register, rrm ? 0 : 2);
            }

            return rexPrefix;
        }

        private static bool Is64Bits(OperandType type)
        {
            return type == OperandType.Integer64 || type == OperandType.Float64;
        }

        private static bool IsImm8(ulong immediate, OperandType type)
        {
            long value = type == OperandType.Integer32 ? (int)immediate : (long)immediate;

            return ConstFitsOnS8(value);
        }

        private static bool IsImm32(ulong immediate, OperandType type)
        {
            long value = type == OperandType.Integer32 ? (int)immediate : (long)immediate;

            return ConstFitsOnS32(value);
        }

        public static int GetJccLength(long offset)
        {
            if (ConstFitsOnS8(offset < 0 ? offset - 2 : offset))
            {
                return 2;
            }
            else if (ConstFitsOnS32(offset < 0 ? offset - 6 : offset))
            {
                return 6;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        public static int GetJmpLength(long offset)
        {
            if (ConstFitsOnS8(offset < 0 ? offset - 2 : offset))
            {
                return 2;
            }
            else if (ConstFitsOnS32(offset < 0 ? offset - 5 : offset))
            {
                return 5;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        private static bool ConstFitsOnS8(long value) => value == (sbyte)value;

        private static bool ConstFitsOnS32(long value) => value == (int)value;

        private void WriteInt16(short value) => WriteUInt16((ushort)value);

        private void WriteInt32(int value) => WriteUInt32((uint)value);

        private void WriteByte(byte value) => OutStream.WriteByte(value);

        private static unsafe ReadOnlySpan<byte> GetSpan<T>(T value) where T : unmanaged {
            return new ReadOnlySpan<byte>(Unsafe.AsPointer(ref value), sizeof(T));
        }

        private void WriteUInt16(ushort value) => OutStream.Write(GetSpan(value));

        private void WriteUInt32(uint value) => OutStream.Write(GetSpan(value));

        private void WriteUInt64(ulong value) => OutStream.Write(GetSpan(value));
    }
}