using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;

using AsmInstruction = RyuASM.X64.Instruction;

namespace ARMeilleure.CodeGen.X86
{
    static class IntrinsicTable
    {
        private static IntrinsicInfo[] _intrinTable;

        static IntrinsicTable()
        {
            _intrinTable = new IntrinsicInfo[EnumUtils.GetCount(typeof(Intrinsic))];

            Add(Intrinsic.X86Addpd,      new IntrinsicInfo(AsmInstruction.Addpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Addps,      new IntrinsicInfo(AsmInstruction.Addps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Addsd,      new IntrinsicInfo(AsmInstruction.Addsd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Addss,      new IntrinsicInfo(AsmInstruction.Addss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Aesdec,     new IntrinsicInfo(AsmInstruction.Aesdec,     IntrinsicType.Binary));
            Add(Intrinsic.X86Aesdeclast, new IntrinsicInfo(AsmInstruction.Aesdeclast, IntrinsicType.Binary));
            Add(Intrinsic.X86Aesenc,     new IntrinsicInfo(AsmInstruction.Aesenc,     IntrinsicType.Binary));
            Add(Intrinsic.X86Aesenclast, new IntrinsicInfo(AsmInstruction.Aesenclast, IntrinsicType.Binary));
            Add(Intrinsic.X86Aesimc,     new IntrinsicInfo(AsmInstruction.Aesimc,     IntrinsicType.Unary));
            Add(Intrinsic.X86Andnpd,     new IntrinsicInfo(AsmInstruction.Andnpd,     IntrinsicType.Binary));
            Add(Intrinsic.X86Andnps,     new IntrinsicInfo(AsmInstruction.Andnps,     IntrinsicType.Binary));
            Add(Intrinsic.X86Andpd,      new IntrinsicInfo(AsmInstruction.Andpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Andps,      new IntrinsicInfo(AsmInstruction.Andps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Blendvpd,   new IntrinsicInfo(AsmInstruction.Blendvpd,   IntrinsicType.Ternary));
            Add(Intrinsic.X86Blendvps,   new IntrinsicInfo(AsmInstruction.Blendvps,   IntrinsicType.Ternary));
            Add(Intrinsic.X86Cmppd,      new IntrinsicInfo(AsmInstruction.Cmppd,      IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Cmpps,      new IntrinsicInfo(AsmInstruction.Cmpps,      IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Cmpsd,      new IntrinsicInfo(AsmInstruction.Cmpsd,      IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Cmpss,      new IntrinsicInfo(AsmInstruction.Cmpss,      IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Comisdeq,   new IntrinsicInfo(AsmInstruction.Comisd,     IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisdge,   new IntrinsicInfo(AsmInstruction.Comisd,     IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisdlt,   new IntrinsicInfo(AsmInstruction.Comisd,     IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisseq,   new IntrinsicInfo(AsmInstruction.Comiss,     IntrinsicType.Comis_));
            Add(Intrinsic.X86Comissge,   new IntrinsicInfo(AsmInstruction.Comiss,     IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisslt,   new IntrinsicInfo(AsmInstruction.Comiss,     IntrinsicType.Comis_));
            Add(Intrinsic.X86Cvtdq2pd,   new IntrinsicInfo(AsmInstruction.Cvtdq2pd,   IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtdq2ps,   new IntrinsicInfo(AsmInstruction.Cvtdq2ps,   IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtpd2dq,   new IntrinsicInfo(AsmInstruction.Cvtpd2dq,   IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtpd2ps,   new IntrinsicInfo(AsmInstruction.Cvtpd2ps,   IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtps2dq,   new IntrinsicInfo(AsmInstruction.Cvtps2dq,   IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtps2pd,   new IntrinsicInfo(AsmInstruction.Cvtps2pd,   IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtsd2si,   new IntrinsicInfo(AsmInstruction.Cvtsd2si,   IntrinsicType.UnaryToGpr));
            Add(Intrinsic.X86Cvtsd2ss,   new IntrinsicInfo(AsmInstruction.Cvtsd2ss,   IntrinsicType.Binary));
            Add(Intrinsic.X86Cvtsi2sd,   new IntrinsicInfo(AsmInstruction.Cvtsi2sd,   IntrinsicType.BinaryGpr));
            Add(Intrinsic.X86Cvtsi2si,   new IntrinsicInfo(AsmInstruction.Movd,       IntrinsicType.UnaryToGpr));
            Add(Intrinsic.X86Cvtsi2ss,   new IntrinsicInfo(AsmInstruction.Cvtsi2ss,   IntrinsicType.BinaryGpr));
            Add(Intrinsic.X86Cvtss2sd,   new IntrinsicInfo(AsmInstruction.Cvtss2sd,   IntrinsicType.Binary));
            Add(Intrinsic.X86Cvtss2si,   new IntrinsicInfo(AsmInstruction.Cvtss2si,   IntrinsicType.UnaryToGpr));
            Add(Intrinsic.X86Divpd,      new IntrinsicInfo(AsmInstruction.Divpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Divps,      new IntrinsicInfo(AsmInstruction.Divps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Divsd,      new IntrinsicInfo(AsmInstruction.Divsd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Divss,      new IntrinsicInfo(AsmInstruction.Divss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Haddpd,     new IntrinsicInfo(AsmInstruction.Haddpd,     IntrinsicType.Binary));
            Add(Intrinsic.X86Haddps,     new IntrinsicInfo(AsmInstruction.Haddps,     IntrinsicType.Binary));
            Add(Intrinsic.X86Insertps,   new IntrinsicInfo(AsmInstruction.Insertps,   IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Maxpd,      new IntrinsicInfo(AsmInstruction.Maxpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Maxps,      new IntrinsicInfo(AsmInstruction.Maxps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Maxsd,      new IntrinsicInfo(AsmInstruction.Maxsd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Maxss,      new IntrinsicInfo(AsmInstruction.Maxss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Minpd,      new IntrinsicInfo(AsmInstruction.Minpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Minps,      new IntrinsicInfo(AsmInstruction.Minps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Minsd,      new IntrinsicInfo(AsmInstruction.Minsd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Minss,      new IntrinsicInfo(AsmInstruction.Minss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Movhlps,    new IntrinsicInfo(AsmInstruction.Movhlps,    IntrinsicType.Binary));
            Add(Intrinsic.X86Movlhps,    new IntrinsicInfo(AsmInstruction.Movlhps,    IntrinsicType.Binary));
            Add(Intrinsic.X86Movss,      new IntrinsicInfo(AsmInstruction.Movss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Mulpd,      new IntrinsicInfo(AsmInstruction.Mulpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Mulps,      new IntrinsicInfo(AsmInstruction.Mulps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Mulsd,      new IntrinsicInfo(AsmInstruction.Mulsd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Mulss,      new IntrinsicInfo(AsmInstruction.Mulss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Paddb,      new IntrinsicInfo(AsmInstruction.Paddb,      IntrinsicType.Binary));
            Add(Intrinsic.X86Paddd,      new IntrinsicInfo(AsmInstruction.Paddd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Paddq,      new IntrinsicInfo(AsmInstruction.Paddq,      IntrinsicType.Binary));
            Add(Intrinsic.X86Paddw,      new IntrinsicInfo(AsmInstruction.Paddw,      IntrinsicType.Binary));
            Add(Intrinsic.X86Pand,       new IntrinsicInfo(AsmInstruction.Pand,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pandn,      new IntrinsicInfo(AsmInstruction.Pandn,      IntrinsicType.Binary));
            Add(Intrinsic.X86Pavgb,      new IntrinsicInfo(AsmInstruction.Pavgb,      IntrinsicType.Binary));
            Add(Intrinsic.X86Pavgw,      new IntrinsicInfo(AsmInstruction.Pavgw,      IntrinsicType.Binary));
            Add(Intrinsic.X86Pblendvb,   new IntrinsicInfo(AsmInstruction.Pblendvb,   IntrinsicType.Ternary));
            Add(Intrinsic.X86Pcmpeqb,    new IntrinsicInfo(AsmInstruction.Pcmpeqb,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpeqd,    new IntrinsicInfo(AsmInstruction.Pcmpeqd,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpeqq,    new IntrinsicInfo(AsmInstruction.Pcmpeqq,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpeqw,    new IntrinsicInfo(AsmInstruction.Pcmpeqw,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtb,    new IntrinsicInfo(AsmInstruction.Pcmpgtb,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtd,    new IntrinsicInfo(AsmInstruction.Pcmpgtd,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtq,    new IntrinsicInfo(AsmInstruction.Pcmpgtq,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtw,    new IntrinsicInfo(AsmInstruction.Pcmpgtw,    IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxsb,     new IntrinsicInfo(AsmInstruction.Pmaxsb,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxsd,     new IntrinsicInfo(AsmInstruction.Pmaxsd,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxsw,     new IntrinsicInfo(AsmInstruction.Pmaxsw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxub,     new IntrinsicInfo(AsmInstruction.Pmaxub,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxud,     new IntrinsicInfo(AsmInstruction.Pmaxud,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxuw,     new IntrinsicInfo(AsmInstruction.Pmaxuw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pminsb,     new IntrinsicInfo(AsmInstruction.Pminsb,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pminsd,     new IntrinsicInfo(AsmInstruction.Pminsd,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pminsw,     new IntrinsicInfo(AsmInstruction.Pminsw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pminub,     new IntrinsicInfo(AsmInstruction.Pminub,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pminud,     new IntrinsicInfo(AsmInstruction.Pminud,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pminuw,     new IntrinsicInfo(AsmInstruction.Pminuw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmovsxbw,   new IntrinsicInfo(AsmInstruction.Pmovsxbw,   IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovsxdq,   new IntrinsicInfo(AsmInstruction.Pmovsxdq,   IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovsxwd,   new IntrinsicInfo(AsmInstruction.Pmovsxwd,   IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovzxbw,   new IntrinsicInfo(AsmInstruction.Pmovzxbw,   IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovzxdq,   new IntrinsicInfo(AsmInstruction.Pmovzxdq,   IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovzxwd,   new IntrinsicInfo(AsmInstruction.Pmovzxwd,   IntrinsicType.Unary));
            Add(Intrinsic.X86Pmulld,     new IntrinsicInfo(AsmInstruction.Pmulld,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pmullw,     new IntrinsicInfo(AsmInstruction.Pmullw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Popcnt,     new IntrinsicInfo(AsmInstruction.Popcnt,     IntrinsicType.PopCount));
            Add(Intrinsic.X86Por,        new IntrinsicInfo(AsmInstruction.Por,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pshufb,     new IntrinsicInfo(AsmInstruction.Pshufb,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pslld,      new IntrinsicInfo(AsmInstruction.Pslld,      IntrinsicType.Binary));
            Add(Intrinsic.X86Pslldq,     new IntrinsicInfo(AsmInstruction.Pslldq,     IntrinsicType.Binary));
            Add(Intrinsic.X86Psllq,      new IntrinsicInfo(AsmInstruction.Psllq,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psllw,      new IntrinsicInfo(AsmInstruction.Psllw,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psrad,      new IntrinsicInfo(AsmInstruction.Psrad,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psraw,      new IntrinsicInfo(AsmInstruction.Psraw,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psrld,      new IntrinsicInfo(AsmInstruction.Psrld,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psrlq,      new IntrinsicInfo(AsmInstruction.Psrlq,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psrldq,     new IntrinsicInfo(AsmInstruction.Psrldq,     IntrinsicType.Binary));
            Add(Intrinsic.X86Psrlw,      new IntrinsicInfo(AsmInstruction.Psrlw,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psubb,      new IntrinsicInfo(AsmInstruction.Psubb,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psubd,      new IntrinsicInfo(AsmInstruction.Psubd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psubq,      new IntrinsicInfo(AsmInstruction.Psubq,      IntrinsicType.Binary));
            Add(Intrinsic.X86Psubw,      new IntrinsicInfo(AsmInstruction.Psubw,      IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhbw,  new IntrinsicInfo(AsmInstruction.Punpckhbw,  IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhdq,  new IntrinsicInfo(AsmInstruction.Punpckhdq,  IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhqdq, new IntrinsicInfo(AsmInstruction.Punpckhqdq, IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhwd,  new IntrinsicInfo(AsmInstruction.Punpckhwd,  IntrinsicType.Binary));
            Add(Intrinsic.X86Punpcklbw,  new IntrinsicInfo(AsmInstruction.Punpcklbw,  IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckldq,  new IntrinsicInfo(AsmInstruction.Punpckldq,  IntrinsicType.Binary));
            Add(Intrinsic.X86Punpcklqdq, new IntrinsicInfo(AsmInstruction.Punpcklqdq, IntrinsicType.Binary));
            Add(Intrinsic.X86Punpcklwd,  new IntrinsicInfo(AsmInstruction.Punpcklwd,  IntrinsicType.Binary));
            Add(Intrinsic.X86Pxor,       new IntrinsicInfo(AsmInstruction.Pxor,       IntrinsicType.Binary));
            Add(Intrinsic.X86Rcpps,      new IntrinsicInfo(AsmInstruction.Rcpps,      IntrinsicType.Unary));
            Add(Intrinsic.X86Rcpss,      new IntrinsicInfo(AsmInstruction.Rcpss,      IntrinsicType.Unary));
            Add(Intrinsic.X86Roundpd,    new IntrinsicInfo(AsmInstruction.Roundpd,    IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Roundps,    new IntrinsicInfo(AsmInstruction.Roundps,    IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Roundsd,    new IntrinsicInfo(AsmInstruction.Roundsd,    IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Roundss,    new IntrinsicInfo(AsmInstruction.Roundss,    IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Rsqrtps,    new IntrinsicInfo(AsmInstruction.Rsqrtps,    IntrinsicType.Unary));
            Add(Intrinsic.X86Rsqrtss,    new IntrinsicInfo(AsmInstruction.Rsqrtss,    IntrinsicType.Unary));
            Add(Intrinsic.X86Shufpd,     new IntrinsicInfo(AsmInstruction.Shufpd,     IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Shufps,     new IntrinsicInfo(AsmInstruction.Shufps,     IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Sqrtpd,     new IntrinsicInfo(AsmInstruction.Sqrtpd,     IntrinsicType.Unary));
            Add(Intrinsic.X86Sqrtps,     new IntrinsicInfo(AsmInstruction.Sqrtps,     IntrinsicType.Unary));
            Add(Intrinsic.X86Sqrtsd,     new IntrinsicInfo(AsmInstruction.Sqrtsd,     IntrinsicType.Unary));
            Add(Intrinsic.X86Sqrtss,     new IntrinsicInfo(AsmInstruction.Sqrtss,     IntrinsicType.Unary));
            Add(Intrinsic.X86Subpd,      new IntrinsicInfo(AsmInstruction.Subpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Subps,      new IntrinsicInfo(AsmInstruction.Subps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Subsd,      new IntrinsicInfo(AsmInstruction.Subsd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Subss,      new IntrinsicInfo(AsmInstruction.Subss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Unpckhpd,   new IntrinsicInfo(AsmInstruction.Unpckhpd,   IntrinsicType.Binary));
            Add(Intrinsic.X86Unpckhps,   new IntrinsicInfo(AsmInstruction.Unpckhps,   IntrinsicType.Binary));
            Add(Intrinsic.X86Unpcklpd,   new IntrinsicInfo(AsmInstruction.Unpcklpd,   IntrinsicType.Binary));
            Add(Intrinsic.X86Unpcklps,   new IntrinsicInfo(AsmInstruction.Unpcklps,   IntrinsicType.Binary));
            Add(Intrinsic.X86Xorpd,      new IntrinsicInfo(AsmInstruction.Xorpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Xorps,      new IntrinsicInfo(AsmInstruction.Xorps,      IntrinsicType.Binary));
        }

        private static void Add(Intrinsic intrin, IntrinsicInfo info)
        {
            _intrinTable[(int)intrin] = info;
        }

        public static IntrinsicInfo GetInfo(Intrinsic intrin)
        {
            return _intrinTable[(int)intrin];
        }
    }
}