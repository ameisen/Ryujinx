using System;

namespace RyuASM.X64
{
#pragma warning disable RCS1191 // Declare enum value as combination of names.
    [Flags]
    internal enum InstructionFlags : uint
    {
        None = 0U,
        RegCoded = 1U << 0,
        Reg8Src = 1U << 1,
        Reg8Dest = 1U << 2,
        RexW = 1U << 3,
        Vex = 1U << 4,

        PrefixBit = 16,
        PrefixMask = 3U << (int)PrefixBit,
        Prefix66 = 1U << (int)PrefixBit,
        PrefixF3 = 2U << (int)PrefixBit,
        PrefixF2 = 3U << (int)PrefixBit
    }
#pragma warning restore RCS1191 // Declare enum value as combination of names.
}
