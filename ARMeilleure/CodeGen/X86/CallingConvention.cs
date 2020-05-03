using System;
using System.Runtime.InteropServices;

using AsmRegister = RyuASM.X64.Register;

namespace ARMeilleure.CodeGen.X86
{
    static class CallingConvention
    {
        private const int RegistersMask = 0xffff;

        public static int GetIntAvailableRegisters()
        {
            return RegistersMask & ~(1 << (int)AsmRegister.Sp);
        }

        public static int GetVecAvailableRegisters()
        {
            return RegistersMask;
        }

        public static int GetIntCallerSavedRegisters()
        {
            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                return (1 << (int)AsmRegister.A) |
                       (1 << (int)AsmRegister.C) |
                       (1 << (int)AsmRegister.D) |
                       (1 << (int)AsmRegister.R8)  |
                       (1 << (int)AsmRegister.R9)  |
                       (1 << (int)AsmRegister.R10) |
                       (1 << (int)AsmRegister.R11);
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                return (1 << (int)AsmRegister.A) |
                       (1 << (int)AsmRegister.C) |
                       (1 << (int)AsmRegister.D) |
                       (1 << (int)AsmRegister.Si) |
                       (1 << (int)AsmRegister.Di) |
                       (1 << (int)AsmRegister.R8)  |
                       (1 << (int)AsmRegister.R9)  |
                       (1 << (int)AsmRegister.R10) |
                       (1 << (int)AsmRegister.R11);
            }
        }

        public static int GetVecCallerSavedRegisters()
        {
            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                return (1 << (int)AsmRegister.Xmm0) |
                       (1 << (int)AsmRegister.Xmm1) |
                       (1 << (int)AsmRegister.Xmm2) |
                       (1 << (int)AsmRegister.Xmm3) |
                       (1 << (int)AsmRegister.Xmm4) |
                       (1 << (int)AsmRegister.Xmm5);
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                return RegistersMask;
            }
        }

        public static int GetIntCalleeSavedRegisters()
        {
            return GetIntCallerSavedRegisters() ^ RegistersMask;
        }

        public static int GetVecCalleeSavedRegisters()
        {
            return GetVecCallerSavedRegisters() ^ RegistersMask;
        }

        public static int GetArgumentsOnRegsCount()
        {
            return 4;
        }

        public static int GetIntArgumentsOnRegsCount()
        {
            return 6;
        }

        public static int GetVecArgumentsOnRegsCount()
        {
            return 8;
        }

        public static AsmRegister GetIntArgumentRegister(int index)
        {
            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                switch (index)
                {
                    case 0: return AsmRegister.C;
                    case 1: return AsmRegister.D;
                    case 2: return AsmRegister.R8;
                    case 3: return AsmRegister.R9;
                }
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                switch (index)
                {
                    case 0: return AsmRegister.Di;
                    case 1: return AsmRegister.Si;
                    case 2: return AsmRegister.D;
                    case 3: return AsmRegister.C;
                    case 4: return AsmRegister.R8;
                    case 5: return AsmRegister.R9;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static AsmRegister GetVecArgumentRegister(int index)
        {
            int count;

            if (GetCurrentCallConv() == CallConvName.Windows)
            {
                count = 4;
            }
            else /* if (GetCurrentCallConv() == CallConvName.SystemV) */
            {
                count = 8;
            }

            if ((uint)index < count)
            {
                return (AsmRegister)((int)AsmRegister.Xmm0 + index);
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static AsmRegister GetIntReturnRegister()
        {
            return AsmRegister.A;
        }

        public static AsmRegister GetIntReturnRegisterHigh()
        {
            return AsmRegister.D;
        }

        public static AsmRegister GetVecReturnRegister()
        {
            return AsmRegister.Xmm0;
        }

        public static CallConvName GetCurrentCallConv()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? CallConvName.Windows
                : CallConvName.SystemV;
        }
    }
}