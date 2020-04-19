using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.Exceptions
{
    public class InvalidStructLayoutException<T> : Exception where T : struct
    {
        static readonly Type _structType = typeof(T);

        public InvalidStructLayoutException(string message) : base(message) {}
        
        public InvalidStructLayoutException(int expectedSize) : 
            base($"Type {_structType.Name} has the wrong size. Expected: {expectedSize} bytes, Got: {Unsafe.SizeOf<T>()} bytes") {}
    }
}