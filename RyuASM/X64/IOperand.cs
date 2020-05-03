using RyuASM.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RyuASM.X64
{
    public interface IOperand
    {
        public bool IsRegister { [MethodImpl(MethodFlags.FullInline)] get; }
        public bool IsConstant { [MethodImpl(MethodFlags.FullInline)] get; }
        public bool IsMemory { [MethodImpl(MethodFlags.FullInline)] get; }

        public OperandType Type { [MethodImpl(MethodFlags.FullInline)] get; }

        public ulong Value { [MethodImpl(MethodFlags.FullInline)] get; }

        public uint RegisterIndex { [MethodImpl(MethodFlags.FullInline)] get; }

        internal Register Register => (Register)RegisterIndex;

        public string KindName { [return: NotNull] [MethodImpl(MethodFlags.NoInline)] get; }

        public sealed byte AsByte => (byte)Value;
        public sealed short AsInt16 => (short)Value;
        public sealed int AsInt32 => (int)Value;
        public sealed long AsInt64 => (long)Value;
        public sealed float AsFloat => BitConverter.Int32BitsToSingle((int)Value);
        public sealed double AsDouble => BitConverter.Int64BitsToDouble((long)Value);
    }
}
