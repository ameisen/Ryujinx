using RyuASM.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RyuASM.X64
{
    public interface IMemoryOperand : IOperand
    {
        bool IOperand.IsMemory => true;

        public IOperand BaseAddress { [return: NotNull] [MethodImpl(MethodFlags.FullInline)] get; }

        public int Displacement { [MethodImpl(MethodFlags.FullInline)] get; }

        public IOperand Index { [return: MaybeNull] [MethodImpl(MethodFlags.FullInline)] get; }

        public int Shift { [MethodImpl(MethodFlags.FullInline)] get; }
    }
}
