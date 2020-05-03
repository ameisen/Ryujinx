using RyuASM.Common;
using System.Runtime.CompilerServices;

namespace RyuASM.X64
{
    public interface ICapabilities
    {
        public bool VexEncoding { [MethodImpl(MethodFlags.FullInline)] get; }
    }
}
