using RyuASM.Common;
using System.Runtime.CompilerServices;

namespace RyuASM.X64
{
    public enum OperandType
    {
        None,

        Integer16,
        Integer32,
        Integer64,

        Float32,
        Float64,

        Vector128
    }

    static class OperandTypeExtensions
    {
        private static class Integral
        {
            internal const OperandType Start = OperandType.Integer16;
            internal const OperandType End = OperandType.Integer64;

            [MethodImpl(MethodFlags.FullInline)]
            internal static bool Is(OperandType type) => (type >= Start) & (type <= End);
        }

        private static class Floating
        {
            internal const OperandType Start = OperandType.Float32;
            internal const OperandType End = OperandType.Float64;

            [MethodImpl(MethodFlags.FullInline)]
            internal static bool Is(OperandType type) => (type >= Start) & (type <= End);
        }

        private static class Vector
        {
            internal const OperandType Start = OperandType.Vector128;
            internal const OperandType End = OperandType.Vector128;

            [MethodImpl(MethodFlags.FullInline)]
            internal static bool Is(OperandType type) => (type >= Start) & (type <= End);
        }

        [MethodImpl(MethodFlags.FullInline)]
        public static bool IsIntegral(this OperandType type) => Integral.Is(type);
        [MethodImpl(MethodFlags.FullInline)]
        public static bool IsFloating(this OperandType type) => Floating.Is(type);
        [MethodImpl(MethodFlags.FullInline)]
        public static bool IsVector(this OperandType type) => Vector.Is(type);
    }
}
