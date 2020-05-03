using System;

namespace ARMeilleure.IntermediateRepresentation
{
    enum OperandType
    {
        None,
        I32,
        I64,
        FP32,
        FP64,
        V128
    }

    static class OperandTypeExtensions
    {
        public static RyuASM.X64.OperandType ToAssemblerType(this OperandType type) => type switch
        {
            OperandType.None => RyuASM.X64.OperandType.None,
            OperandType.I32 => RyuASM.X64.OperandType.Integer32,
            OperandType.I64 => RyuASM.X64.OperandType.Integer64,
            OperandType.FP32 => RyuASM.X64.OperandType.Float32,
            OperandType.FP64 => RyuASM.X64.OperandType.Float64,
            OperandType.V128 => RyuASM.X64.OperandType.Vector128,
            _ => throw new NotImplementedException($"Operand Type {type} unimplemented")
        };

        public static bool IsInteger(this OperandType type)
        {
            return type == OperandType.I32 ||
                   type == OperandType.I64;
        }

        public static RegisterType ToRegisterType(this OperandType type)
        {
            switch (type)
            {
                case OperandType.FP32: return RegisterType.Vector;
                case OperandType.FP64: return RegisterType.Vector;
                case OperandType.I32:  return RegisterType.Integer;
                case OperandType.I64:  return RegisterType.Integer;
                case OperandType.V128: return RegisterType.Vector;
            }

            throw new InvalidOperationException($"Invalid operand type \"{type}\".");
        }

        public static int GetSizeInBytes(this OperandType type)
        {
            switch (type)
            {
                case OperandType.FP32: return 4;
                case OperandType.FP64: return 8;
                case OperandType.I32:  return 4;
                case OperandType.I64:  return 8;
                case OperandType.V128: return 16;
            }

            throw new InvalidOperationException($"Invalid operand type \"{type}\".");
        }
    }
}