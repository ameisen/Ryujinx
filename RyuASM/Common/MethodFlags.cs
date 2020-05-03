using System.Runtime.CompilerServices;

namespace RyuASM.Common
{
    static class MethodFlags
    {
        public const MethodImplOptions NoInline = MethodImplOptions.NoInlining;
        public const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
        public const MethodImplOptions FullOptimize = MethodImplOptions.AggressiveOptimization;
        public const MethodImplOptions FullInline = Inline | FullOptimize; 
    }
}
