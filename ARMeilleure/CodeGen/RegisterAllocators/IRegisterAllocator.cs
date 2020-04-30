using ARMeilleure.Translation;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    interface IRegisterAllocator
    {
        AllocationResult RunPass(
            ControlFlowGraph cfg,
            StackAllocator stackAlloc,
            in RegisterMasks regMasks);
    }
}