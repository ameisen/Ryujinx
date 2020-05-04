using RyuASM.Common;
using System;
using System.Runtime.CompilerServices;

using SelfConstants = RyuASM.X64.Constants.InstructionFlags;

namespace RyuASM.X64
{
#pragma warning disable RCS1191, RCS1154 // Declare enum value as combination of names. // Sort enum members.
    [Flags]
    internal enum InstructionFlags : uint
    {
        None =      0U,
        RegCoded =  1U << 0,
        Reg8Src =   1U << 1,
        Reg8Dest =  1U << 2,
        RexW =      1U << 3,
        Vex =       1U << 4,

        PrefixMask =    0b11 << SelfConstants.PrefixOffset,
        Prefix66 =      0b01 << SelfConstants.PrefixOffset,
        PrefixF3 =      0b10 << SelfConstants.PrefixOffset,
        PrefixF2 =      0b11 << SelfConstants.PrefixOffset
    }
#pragma warning restore RCS1191, RCS1154 // Declare enum value as combination of names. // Sort enum members.

    internal static class InstructionFlagsExt
    {
        [MethodImpl(MethodFlags.FullInline)]
        public static uint GetPrefix(this InstructionFlags flags) => (uint)(flags & InstructionFlags.PrefixMask) >> SelfConstants.PrefixOffset;
    }
}
