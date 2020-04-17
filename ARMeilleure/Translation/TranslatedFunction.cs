using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ARMeilleure.Translation
{
    class TranslatedFunction
    {
        private const int MinCallsForRejit = 100;

        public readonly IntPtr Pointer;

        private volatile int _entryCount;

        private readonly GuestFunction _func;

        private readonly ulong _address;
        public readonly bool HighCq;
        private int _callCount;

        public TranslatedFunction(GuestFunction func, ulong address, bool highCq)
        {
            _func = func;
            Pointer = Marshal.GetFunctionPointerForDelegate(func);
            HighCq = highCq;
            _address = address;
        }

        public bool Discard() => Interlocked.CompareExchange(ref _entryCount, int.MinValue, 0) == 0;

        public ulong Execute(State.ExecutionContext context)
        {
            if (Interlocked.Increment(ref _entryCount) <= 0)
            {
                return _address;
            }

            try
            {
                return _func(context.NativeContextPtr);
            }
            finally
            {
                Interlocked.Decrement(ref _entryCount);
            }
        }

        public bool ShouldRejit => !HighCq && Interlocked.Increment(ref _callCount) == MinCallsForRejit;
    }
}