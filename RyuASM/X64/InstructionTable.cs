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
                uint rmr = BadOp,
                uint rmImm8 = BadOp,
                uint rmImm32 = BadOp,
                uint rmImm64 = BadOp,
                uint rrm = BadOp,
                InstructionFlags flags = InstructionFlags.None)
            {
                OpRMR = rmr;
                OpRMImm8 = rmImm8;
                OpRMImm32 = rmImm32;
                OpRImm64 = rmImm64;
                OpRRM = rrm;
                Flags = flags;
            }
        }

        private static readonly InstructionInfo[] Table = new InstructionInfo[(int)Instruction.Count];

        [MethodImpl(MethodFlags.FullInline)]
        internal static ref InstructionInfo Get(Instruction instruction) => ref Table[(int)instruction];

        static InstructionTable()
        {
            //  Name                                           RM/R        RM/I8       RM/I32      R/I64       R/RM        Flags
            Add(Instruction.Add, new InstructionInfo(rmr: 0x00000001, rmImm8: 0x00000083, rmImm32: 0x00000081, rrm: 0x00000003, flags: InstructionFlags.None));
            Add(Instruction.Addpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Addps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, flags: InstructionFlags.Vex));
            Add(Instruction.Addsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Addss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f58, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Aesdec, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38de, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesdeclast, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38df, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesenc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38dc, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesenclast, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38dd, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Aesimc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f38db, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.And, new InstructionInfo(0x00000021, 0x04000083, 0x04000081, BadOp, 0x00000023, flags: InstructionFlags.None));
            Add(Instruction.Andnpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f55, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Andnps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f55, flags: InstructionFlags.Vex));
            Add(Instruction.Andpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f54, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Andps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f54, flags: InstructionFlags.Vex));
            Add(Instruction.Blendvpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3815, flags: InstructionFlags.Prefix66));
            Add(Instruction.Blendvps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3814, flags: InstructionFlags.Prefix66));
            Add(Instruction.Bsr, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fbd, flags: InstructionFlags.None));
            Add(Instruction.Bswap, new InstructionInfo(0x00000fc8, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.RegCoded));
            Add(Instruction.Call, new InstructionInfo(0x020000ff, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Cmovcc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f40, flags: InstructionFlags.None));
            Add(Instruction.Cmp, new InstructionInfo(0x00000039, 0x07000083, 0x07000081, BadOp, 0x0000003b, flags: InstructionFlags.None));
            Add(Instruction.Cmppd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Cmpps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, flags: InstructionFlags.Vex));
            Add(Instruction.Cmpsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cmpss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc2, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cmpxchg, new InstructionInfo(0x00000fb1, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Cmpxchg16b, new InstructionInfo(0x01000fc7, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.RexW));
            Add(Instruction.Comisd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2f, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Comiss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2f, flags: InstructionFlags.Vex));
            Add(Instruction.Cpuid, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fa2, flags: InstructionFlags.RegCoded));
            Add(Instruction.Cqo, new InstructionInfo(0x00000099, BadOp, BadOp, BadOp, 0x00000099, flags: InstructionFlags.None)); // TODO : provide some flags that specify that this only works with the A register
            Add(Instruction.Cvtdq2pd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe6, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cvtdq2ps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5b, flags: InstructionFlags.Vex));
            Add(Instruction.Cvtpd2dq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe6, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtpd2ps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Cvtps2dq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5b, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Cvtps2pd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, flags: InstructionFlags.Vex));
            Add(Instruction.Cvtsd2si, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2d, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtsd2ss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtsi2sd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2a, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Cvtsi2ss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2a, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cvtss2sd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5a, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Cvtss2si, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f2d, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Div, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x060000f7, flags: InstructionFlags.None));
            Add(Instruction.Divpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Divps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, flags: InstructionFlags.Vex));
            Add(Instruction.Divsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Divss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5e, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Haddpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f7c, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Haddps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f7c, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Idiv, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x070000f7, flags: InstructionFlags.None));
            Add(Instruction.Imul, new InstructionInfo(BadOp, 0x0000006b, 0x00000069, BadOp, 0x00000faf, flags: InstructionFlags.None));
            Add(Instruction.Imul128, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x050000f7, flags: InstructionFlags.None));
            Add(Instruction.Insertps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a21, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Jmp, new InstructionInfo(0x040000ff, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Lea, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x0000008d, flags: InstructionFlags.None));
            Add(Instruction.Maxpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Maxps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, flags: InstructionFlags.Vex));
            Add(Instruction.Maxsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Maxss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5f, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Minpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Minps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, flags: InstructionFlags.Vex));
            Add(Instruction.Minsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Minss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5d, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Mov, new InstructionInfo(0x00000089, BadOp, 0x000000c7, 0x000000b8, 0x0000008b, flags: InstructionFlags.None));
            Add(Instruction.Mov16, new InstructionInfo(0x00000089, BadOp, 0x000000c7, BadOp, 0x0000008b, flags: InstructionFlags.Prefix66));
            Add(Instruction.Mov8, new InstructionInfo(0x00000088, 0x000000c6, BadOp, BadOp, 0x0000008a, flags: InstructionFlags.Reg8Src | InstructionFlags.Reg8Dest));
            Add(Instruction.Movd, new InstructionInfo(0x00000f7e, BadOp, BadOp, BadOp, 0x00000f6e, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Movdqu, new InstructionInfo(0x00000f7f, BadOp, BadOp, BadOp, 0x00000f6f, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Movhlps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f12, flags: InstructionFlags.Vex));
            Add(Instruction.Movlhps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f16, flags: InstructionFlags.Vex));
            Add(Instruction.Movq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f7e, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Movsd, new InstructionInfo(0x00000f11, BadOp, BadOp, BadOp, 0x00000f10, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Movss, new InstructionInfo(0x00000f11, BadOp, BadOp, BadOp, 0x00000f10, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Movsx16, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fbf, flags: InstructionFlags.None));
            Add(Instruction.Movsx32, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000063, flags: InstructionFlags.None));
            Add(Instruction.Movsx8, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fbe, flags: InstructionFlags.Reg8Src));
            Add(Instruction.Movzx16, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fb7, flags: InstructionFlags.None));
            Add(Instruction.Movzx8, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fb6, flags: InstructionFlags.Reg8Src));
            Add(Instruction.Mul128, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x040000f7, flags: InstructionFlags.None));
            Add(Instruction.Mulpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Mulps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, flags: InstructionFlags.Vex));
            Add(Instruction.Mulsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Mulss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f59, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Neg, new InstructionInfo(0x030000f7, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Not, new InstructionInfo(0x020000f7, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Or, new InstructionInfo(0x00000009, 0x01000083, 0x01000081, BadOp, 0x0000000b, flags: InstructionFlags.None));
            Add(Instruction.Paddb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffc, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Paddd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffe, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Paddq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fd4, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Paddw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffd, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pand, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fdb, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pandn, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fdf, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pavgb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe0, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pavgw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fe3, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pblendvb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3810, flags: InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f74, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f76, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3829, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpeqw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f75, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f64, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f66, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3837, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pcmpgtw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f65, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pextrb, new InstructionInfo(0x000f3a14, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pextrd, new InstructionInfo(0x000f3a16, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pextrq, new InstructionInfo(0x000f3a16, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(Instruction.Pextrw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc5, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a20, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a22, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a22, flags: InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(Instruction.Pinsrw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc4, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxsb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383c, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383d, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxsw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fee, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxub, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fde, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxud, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383f, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmaxuw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383e, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminsb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3838, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3839, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminsw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fea, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminub, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fda, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminud, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383b, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pminuw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f383a, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovsxbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3820, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovsxdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3825, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovsxwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3823, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovzxbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3830, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovzxdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3835, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmovzxwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3833, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmulld, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3840, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pmullw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fd5, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pop, new InstructionInfo(0x0000008f, BadOp, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Popcnt, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fb8, flags: InstructionFlags.PrefixF3));
            Add(Instruction.Por, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000feb, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pshufb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3800, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pshufd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f70, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pslld, new InstructionInfo(BadOp, 0x06000f72, BadOp, BadOp, 0x00000ff2, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Pslldq, new InstructionInfo(BadOp, 0x07000f73, BadOp, BadOp, BadOp, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psllq, new InstructionInfo(BadOp, 0x06000f73, BadOp, BadOp, 0x00000ff3, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psllw, new InstructionInfo(BadOp, 0x06000f71, BadOp, BadOp, 0x00000ff1, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrad, new InstructionInfo(BadOp, 0x04000f72, BadOp, BadOp, 0x00000fe2, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psraw, new InstructionInfo(BadOp, 0x04000f71, BadOp, BadOp, 0x00000fe1, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrld, new InstructionInfo(BadOp, 0x02000f72, BadOp, BadOp, 0x00000fd2, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrlq, new InstructionInfo(BadOp, 0x02000f73, BadOp, BadOp, 0x00000fd3, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrldq, new InstructionInfo(BadOp, 0x03000f73, BadOp, BadOp, BadOp, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psrlw, new InstructionInfo(BadOp, 0x02000f71, BadOp, BadOp, 0x00000fd1, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ff8, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffa, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ffb, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Psubw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000ff9, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f68, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f6a, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhqdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f6d, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckhwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f69, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpcklbw, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f60, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpckldq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f62, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpcklqdq, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f6c, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Punpcklwd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f61, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Push, new InstructionInfo(BadOp, 0x0000006a, 0x00000068, BadOp, 0x060000ff, flags: InstructionFlags.None));
            Add(Instruction.Pxor, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fef, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Rcpps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f53, flags: InstructionFlags.Vex));
            Add(Instruction.Rcpss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f53, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Ror, new InstructionInfo(0x010000d3, 0x010000c1, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Roundpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a09, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Roundps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a08, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Roundsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a0b, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Roundss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a0a, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Rsqrtps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f52, flags: InstructionFlags.Vex));
            Add(Instruction.Rsqrtss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f52, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Sar, new InstructionInfo(0x070000d3, 0x070000c1, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Setcc, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f90, flags: InstructionFlags.Reg8Dest));
            Add(Instruction.Shl, new InstructionInfo(0x040000d3, 0x040000c1, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Shr, new InstructionInfo(0x050000d3, 0x050000c1, BadOp, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Shufpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc6, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Shufps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000fc6, flags: InstructionFlags.Vex));
            Add(Instruction.Sqrtpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Sqrtps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, flags: InstructionFlags.Vex));
            Add(Instruction.Sqrtsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Sqrtss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f51, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Sub, new InstructionInfo(0x00000029, 0x05000083, 0x05000081, BadOp, 0x0000002b, flags: InstructionFlags.None));
            Add(Instruction.Subpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Subps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, flags: InstructionFlags.Vex));
            Add(Instruction.Subsd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, flags: InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(Instruction.Subss, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f5c, flags: InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(Instruction.Test, new InstructionInfo(0x00000085, BadOp, 0x000000f7, BadOp, BadOp, flags: InstructionFlags.None));
            Add(Instruction.Unpckhpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f15, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Unpckhps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f15, flags: InstructionFlags.Vex));
            Add(Instruction.Unpcklpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f14, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Unpcklps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f14, flags: InstructionFlags.Vex));
            Add(Instruction.Vblendvpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a4b, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Vblendvps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a4a, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Vpblendvb, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x000f3a4c, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Xor, new InstructionInfo(0x00000031, 0x06000083, 0x06000081, BadOp, 0x00000033, flags: InstructionFlags.None));
            Add(Instruction.Xorpd, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f57, flags: InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(Instruction.Xorps, new InstructionInfo(BadOp, BadOp, BadOp, BadOp, 0x00000f57, flags: InstructionFlags.Vex));
        }

        private static void Add(Instruction inst, in InstructionInfo info) => Table[(int)inst] = info;
    }
}
