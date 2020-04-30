using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ARMeilleure.Translation
{
    class TranslatedFunction
    {
        private const int MinCallsForRejit = 100;

        private GuestFunction _func;
        private IntPtr _funcPtr;

        private bool _rejit;
        private int  _callCount;

        public bool HighCq => !_rejit;

        public TranslatedFunction(GuestFunction func, bool rejit)
        {
            _func  = func;
            _rejit = rejit;
            _funcPtr = Marshal.GetFunctionPointerForDelegate(_func);
        }

        public ulong Execute(State.ExecutionContext context)
        {
            return _func(context.NativeContextPtr);
        }

        public bool ShouldRejit()
        {
            return _rejit && Interlocked.Increment(ref _callCount) == MinCallsForRejit;
        }

        public IntPtr GetPointer()
        {
            if (_funcPtr == IntPtr.Zero)
            {
                _funcPtr = Marshal.GetFunctionPointerForDelegate(_func);
            }

            return _funcPtr;
        }
    }
}