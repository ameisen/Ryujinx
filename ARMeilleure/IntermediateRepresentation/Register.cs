using ARMeilleure.CodeGen.X86;
using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.IntermediateRepresentation
{
    [StructLayout(LayoutKind.Explicit)]
    readonly struct Register : IEquatable<Register>
    {
        // It turns out that the JIT isn't able to turn the Equals comparison of two
        // consecutive integers into a single 64-bit comparison without being
        // explicitly told that they are consecutive by using a packed field.
        [FieldOffset(0)]
        private readonly long Packed;

        [FieldOffset(0)]
        public readonly int Index;

        [FieldOffset(0)]
        public readonly X86Register Reg;
        
        [FieldOffset(sizeof(int))]
        public readonly RegisterType Type;

        public Register(int index, RegisterType type) : this()
        {
            Index = index;
            Type  = type;
        }

        public override readonly int GetHashCode()
        {
            return (ushort)Index | ((int)Type << 16);
        }

        public static bool operator ==(in Register x, X86Register y)
        {
            return x.Reg == y;
        }

        public static bool operator !=(in Register x, X86Register y)
        {
            return x.Reg != y;
        }

        public static bool operator ==(in Register x, in Register y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(in Register x, in Register y)
        {
            return !x.Equals(y);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is Register reg && Equals(reg);
        }

        public readonly bool Equals(Register other)
        {
            return other.Packed == Packed;
        }
    }
}