using AsmInstruction = RyuASM.X64.Instruction;

namespace ARMeilleure.CodeGen.X86
{
    struct IntrinsicInfo
    {
        public AsmInstruction Inst { get; }
        public IntrinsicType Type { get; }

        public IntrinsicInfo(AsmInstruction inst, IntrinsicType type)
        {
            Inst = inst;
            Type = type;
        }
    }
}