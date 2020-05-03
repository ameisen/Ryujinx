using RyuASM.Common;
using System.Runtime.CompilerServices;

namespace RyuASM.X64
{
    internal static class InstructionTable
    {
        public const uint BadOp = 0;

        internal readonly struct InstructionInfo
        {
            public readonly uint OpRMR { get; }
            public readonly uint OpRMImm8 { get; }
            public readonly uint OpRMImm32 { get; }
            public readonly uint OpRImm64 { get; }
            public readonly uint OpRRM { get; }

            public readonly InstructionFlags Flags { get; }

            public InstructionInfo(
                uint opRMR,
                uint opRMImm8,
                uint opRMImm32,
                uint opRImm64,
                uint opRRM,
                InstructionFlags flags)
            {
                OpRMR = opRMR;
                OpRMImm8 = opRMImm8;
                OpRMImm32 = opRMImm32;
                OpRImm64 = opRImm64;
                OpRRM = opRRM;
                Flags = flags;
            }
        }

        private static readonly InstructionInfo[] Table = new InstructionInfo[(int)Instruction.Count];

        [MethodImpl(MethodFlags.FullInline)]
        internal static ref InstructionInfo Get(Instruction instruction) => ref Table[(int)instruction];

        static InstructionTable()
        {
            //  Name                                           RM/R        RM/I8       RM/I32      R/I64       R/RM        Flags
            Add(Instruction.Add, new InstructionInfo(0x00000001, 0x00000083, 0x00000081, BadOp, 0x00000003, InstructionFlags.None));
            Add(Instruction.Addpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Addps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, InstructionFlags.Vex));
            Add(Instruction.Addsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Addss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Aesdec, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38de, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesdeclast, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38df, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesenc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38dc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesenclast, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38dd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesimc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38db, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.And, new InstructionInfo(0x00000021, 0x04000083, 0x04000081, BadOp, 0x00000023, InstructionFlags.None));
            Add(Instruction.Andnpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f55, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Andnps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f55, InstructionFlags.Vex));
            Add(Instruction.Andpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f54, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Andps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f54, InstructionFlags.Vex));
            Add(Instruction.Blendvpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3815, InstructionFlags.Prefix66));
            Add(Instruction.Blendvps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3814, InstructionFlags.Prefix66));
            Add(Instruction.Bsr, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fbd, InstructionFlags.None));
            Add(Instruction.Bswap, new InstructionInfo(0x00000fc8, BadOp, BadOp, BadOp, BadOp, InstructionFlags.RegCoded));
            Add(Instruction.Call, new InstructionInfo(0x020000ff, BadOp, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Cmovcc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f40, InstructionFlags.None));
            Add(Instruction.Cmp, new InstructionInfo(0x00000039, 0x07000083, 0x07000081, BadOp, 0x0000003b, InstructionFlags.None));
            Add(Instruction.Cmppd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Cmpps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, InstructionFlags.Vex));
            Add(Instruction.Cmpsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cmpss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cmpxchg, new InstructionInfo(0x00000fb1, BadOp, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Cmpxchg16b, new InstructionInfo(0x01000fc7, BadOp, BadOp, BadOp, BadOp, InstructionFlags.RexW));
            Add(Instruction.Comisd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Comiss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2f, InstructionFlags.Vex));
            Add(Instruction.Cpuid, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fa2, InstructionFlags.RegCoded));
            Add(Instruction.Cvtdq2pd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe6, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cvtdq2ps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5b, InstructionFlags.Vex));
            Add(Instruction.Cvtpd2dq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe6, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtpd2ps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Cvtps2dq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Cvtps2pd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, InstructionFlags.Vex));
            Add(Instruction.Cvtsd2si, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2d, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtsd2ss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtsi2sd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2a, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtsi2ss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2a, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cvtss2sd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cvtss2si, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2d, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Div, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x060000f7, InstructionFlags.None));
            Add(Instruction.Divpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Divps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, InstructionFlags.Vex));
            Add(Instruction.Divsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Divss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Haddpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f7c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Haddps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f7c, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Idiv, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x070000f7, InstructionFlags.None));
            Add(Instruction.Imul, new InstructionInfo(BadOp, 0x0000006b, 0x00000069, BadOp, 0x00000faf, InstructionFlags.None));
            Add(Instruction.Imul128, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x050000f7, InstructionFlags.None));
            Add(Instruction.Insertps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a21, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Jmp, new InstructionInfo(0x040000ff, BadOp, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Lea, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x0000008d, InstructionFlags.None));
            Add(Instruction.Maxpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Maxps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, InstructionFlags.Vex));
            Add(Instruction.Maxsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Maxss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Minpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Minps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, InstructionFlags.Vex));
            Add(Instruction.Minsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Minss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Mov, new InstructionInfo(0x00000089, BadOp, 0x000000c7, 0x000000b8, 0x0000008b, InstructionFlags.None));
            Add(Instruction.Mov16, new InstructionInfo(0x00000089, BadOp, 0x000000c7, BadOp, 0x0000008b, InstructionFlags.Prefix66));
            Add(Instruction.Mov8, new InstructionInfo(0x00000088, 0x000000c6, BadOp, BadOp, 0x0000008a, InstructionFlags.Reg8Src | InstructionFlags.Reg8Dest));
            Add(Instruction.Movd, new InstructionInfo(0x00000f7e, BadOp, BadOp, BadOp, 0x00000f6e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Movdqu, new InstructionInfo(0x00000f7f, BadOp, BadOp, BadOp, 0x00000f6f, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Movhlps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f12, InstructionFlags.Vex));
            Add(Instruction.Movlhps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f16, InstructionFlags.Vex));
            Add(Instruction.Movq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f7e, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Movsd, new InstructionInfo(0x00000f11, BadOp, BadOp, BadOp, 0x00000f10, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Movss, new InstructionInfo(0x00000f11, BadOp, BadOp, BadOp, 0x00000f10, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Movsx16, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fbf, InstructionFlags.None));
            Add(Instruction.Movsx32, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000063, InstructionFlags.None));
            Add(Instruction.Movsx8, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fbe, InstructionFlags.Reg8Src));
            Add(Instruction.Movzx16, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fb7, InstructionFlags.None));
            Add(Instruction.Movzx8, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fb6, InstructionFlags.Reg8Src));
            Add(Instruction.Mul128, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x040000f7, InstructionFlags.None));
            Add(Instruction.Mulpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Mulps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, InstructionFlags.Vex));
            Add(Instruction.Mulsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Mulss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Neg, new InstructionInfo(0x030000f7, BadOp, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Not, new InstructionInfo(0x020000f7, BadOp, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Or, new InstructionInfo(0x00000009, 0x01000083, 0x01000081, BadOp, 0x0000000b, InstructionFlags.None));
            Add(Instruction.Paddb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Paddd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffe, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Paddq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fd4, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Paddw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pand, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fdb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pandn, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fdf, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pavgb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe0, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pavgw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pblendvb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3810, InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f74, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f76, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3829, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f75, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f64, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f66, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3837, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f65, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pextrb, new InstructionInfo(0x000f3a14, BadOp, BadOp, BadOp, BadOp, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pextrd, new InstructionInfo(0x000f3a16, BadOp, BadOp, BadOp, BadOp, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pextrq, new InstructionInfo(0x000f3a16, BadOp, BadOp, BadOp, BadOp, InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(Instruction.Pextrw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc5, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a20, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a22, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a22, InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc4, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxsb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxsw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fee, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxub, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fde, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxud, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxuw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminsb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3838, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3839, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminsw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fea, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminub, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fda, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminud, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminuw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovsxbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3820, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovsxdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3825, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovsxwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3823, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovzxbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3830, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovzxdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3835, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovzxwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3833, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmulld, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3840, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmullw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fd5, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pop, new InstructionInfo(0x0000008f, BadOp, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Popcnt, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fb8, InstructionFlags.PrefixF3));
            Add(Instruction.Por, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000feb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pshufb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3800, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pshufd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f70, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pslld, new InstructionInfo(BadOp, 0x06000f72, BadOp, BadOp, 0x00000ff2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pslldq, new InstructionInfo(BadOp, 0x07000f73, BadOp, BadOp, BadOp, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psllq, new InstructionInfo(BadOp, 0x06000f73, BadOp, BadOp, 0x00000ff3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psllw, new InstructionInfo(BadOp, 0x06000f71, BadOp, BadOp, 0x00000ff1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrad, new InstructionInfo(BadOp, 0x04000f72, BadOp, BadOp, 0x00000fe2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psraw, new InstructionInfo(BadOp, 0x04000f71, BadOp, BadOp, 0x00000fe1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrld, new InstructionInfo(BadOp, 0x02000f72, BadOp, BadOp, 0x00000fd2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrlq, new InstructionInfo(BadOp, 0x02000f73, BadOp, BadOp, 0x00000fd3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrldq, new InstructionInfo(BadOp, 0x03000f73, BadOp, BadOp, BadOp, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrlw, new InstructionInfo(BadOp, 0x02000f71, BadOp, BadOp, 0x00000fd1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ff8, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffa, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ff9, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f68, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f6a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhqdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f6d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f69, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpcklbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f60, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckldq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f62, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpcklqdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f6c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpcklwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f61, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Push, new InstructionInfo(BadOp, 0x0000006a, 0x00000068, BadOp, 0x060000ff, InstructionFlags.None));
            Add(Instruction.Pxor, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fef, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Rcpps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f53, InstructionFlags.Vex));
            Add(Instruction.Rcpss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f53, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Ror, new InstructionInfo(0x010000d3, 0x010000c1, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Roundpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a09, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Roundps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a08, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Roundsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a0b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Roundss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a0a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Rsqrtps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f52, InstructionFlags.Vex));
            Add(Instruction.Rsqrtss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f52, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Sar, new InstructionInfo(0x070000d3, 0x070000c1, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Setcc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f90, InstructionFlags.Reg8Dest));
            Add(Instruction.Shl, new InstructionInfo(0x040000d3, 0x040000c1, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Shr, new InstructionInfo(0x050000d3, 0x050000c1, BadOp, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Shufpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc6, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Shufps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc6, InstructionFlags.Vex));
            Add(Instruction.Sqrtpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Sqrtps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, InstructionFlags.Vex));
            Add(Instruction.Sqrtsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Sqrtss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Sub, new InstructionInfo(0x00000029, 0x05000083, 0x05000081, BadOp, 0x0000002b, InstructionFlags.None));
            Add(Instruction.Subpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Subps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, InstructionFlags.Vex));
            Add(Instruction.Subsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Subss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Test, new InstructionInfo(0x00000085, BadOp, 0x000000f7, BadOp, BadOp, InstructionFlags.None));
            Add(Instruction.Unpckhpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f15, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Unpckhps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f15, InstructionFlags.Vex));
            Add(Instruction.Unpcklpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f14, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Unpcklps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f14, InstructionFlags.Vex));
            Add(Instruction.Vblendvpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a4b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Vblendvps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a4a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Vpblendvb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a4c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Xor, new InstructionInfo(0x00000031, 0x06000083, 0x06000081, BadOp, 0x00000033, InstructionFlags.None));
            Add(Instruction.Xorpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f57, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Xorps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f57, InstructionFlags.Vex));
        }

        private static void Add(Instruction inst, in InstructionInfo info) => Table[(int)inst] = info;
    }
}
