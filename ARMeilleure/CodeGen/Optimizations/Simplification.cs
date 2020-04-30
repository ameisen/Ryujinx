using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class Simplification
    {
        public static void RunPass(Operation operation)
        {
            switch (operation.Instruction)
            {
                case Instruction.Add:
                    TryEliminateBinaryOpComutative(operation, 0);
                    break;

                case Instruction.BitwiseAnd:
                    TryEliminateBitwiseAnd(operation);
                    break;

                case Instruction.BitwiseOr:
                    TryEliminateBitwiseOr(operation);
                    break;

                case Instruction.BitwiseExclusiveOr:
                    TryEliminateBitwiseExclusiveOr(operation);
                    break;

                case Instruction.ConditionalSelect:
                    TryEliminateConditionalSelect(operation);
                    break;

                case Instruction.Divide:
                    TryEliminateBinaryOpY(operation, 1);
                    break;

                case Instruction.Multiply:
                    TryEliminateBinaryOpComutative(operation, 1);
                    break;

                case Instruction.ShiftLeft:
                case Instruction.ShiftRightSI:
                case Instruction.ShiftRightUI:
                case Instruction.Subtract:
                    TryEliminateBinaryOpY(operation, 0);
                    break;
            }
        }

        private static bool LoadDependantPass([NotNull] Operation sourceOperation, [NotNull] Operation dependantOperation)
        {
            return false;

            static bool IsLoad(Operation operation) => operation.Instruction switch
            {
                Instruction.Load => true,
                _ => false,
            };

            //switch (dependantOperation.Instruction)
            //{
            //    case Instruction.BitwiseOr:
            //        break;
            //    default:
            //        return false;
            //}

            var dependantSources = dependantOperation.Sources;

            if (
                (!IsLoad(sourceOperation)) ||
                (sourceOperation.Destinations.Count != 1) || // TODO : I'm not sure how to handle this situation right now.
                (sourceOperation.Sources.Count != 1) || // TODO : Or this one.
                (dependantSources.Count == 0) || // If the dependant operation has no sources, it obviously cannot be coalesced.
                (dependantOperation.Destinations.Count != 1) ||
                IsLoad(dependantOperation) // TODO : we should handle load operations from dependencies, since x86 MOV can do that.
            )
            {
                return false;
            }

            var source = sourceOperation.Sources[0];
            var destination = sourceOperation.Destinations[0];
            var finalDestination = dependantOperation.Destinations[0];

            if (finalDestination.IsMemory)
            {
                return false;
            }

            // See if any of the dependant operand's sources match the source operand's destination.
            if (!dependantSources.TryFindIndex(out var matchIndex, (operand) => operand == destination))
            {
                return false;
            }

            // TODO : I'm not sure what to do in this case.
            if (dependantSources[matchIndex].Kind != source.Kind)
            {
                return false;
            }

            // Make sure none of the other source operands are memory operands.
            if (dependantSources.Any((operand, i) => (i != matchIndex) && operand.IsMemory))
            {
                return false;
            }

            // TODO : Not sure if this is right, but x86 doesn't allow multiple sources, so make
            // sure that all other sources are the final destination.
            //if (!dependantSources.All((operand, i) => (i == matchIndex) || (operand == finalDestination)))
            //{
            //    return false;
            //}


            // If we found a match, let's replace that operand.
            var sources = dependantSources.ToArray();
            sources[matchIndex] = MemoryOp(destination.Type, source);
            dependantOperation.SetSources(sources);

            return true;
        }

        private static bool StoreDependantPass([NotNull] Operation sourceOperation, [NotNull] Operation dependantOperation)
        {
            return false;

            static bool IsStore(Operation operation) => operation.Instruction switch
            {
                Instruction.Store => true,
                //Instruction.Store16 => true,
                //Instruction.Store8 => true,
                _ => false,
            };
            switch (sourceOperation.Instruction)
            {
                case Instruction.Subtract:
                    break;
                default:
                    return false;
            }

            var dependantSources = dependantOperation.Sources;

            if (
                (!IsStore(dependantOperation)) ||
                (sourceOperation.Destinations.Count != 1) || // TODO : I'm not sure how to handle this situation right now.
                (dependantSources.Count != 2) || // If the dependant operation has no sources, it obviously cannot be coalesced.
                (dependantOperation.Destinations.Count != 0) ||
                IsStore(sourceOperation)
            )
            {
                return false;
            }

            var addressOperand = dependantSources[0];
            var valueOperand = dependantSources[1];

            var destination = sourceOperation.Destinations[0];

            // We cannot propogate the address operand forward
            if (destination == addressOperand)
            {
                return false;
            }

            // If it isn't the value operand, these two operations are not dependant on one another.
            if (destination != valueOperand)
            {
                return false;
            }

            // Only the source or the destination can be a memory operand, not both.
            if (sourceOperation.Sources.Any((operand) => operand.IsMemory))
            {
                return false;
            }

            if (addressOperand.Kind != OperandKind.LocalVariable)
            {
                return false;
            }

            if (destination.Uses.Count > 1)
            {
                return false;
            }

            // If we found a match, let's replace that operand.
            var newDestination = MemoryOp(destination.Type, addressOperand);
            newDestination.SuperSpecial = true;
            dependantOperation.Reset();
            sourceOperation.SetDestination(newDestination);

            return true;
        }

        public static bool RunPass([NotNull] Operation sourceOperation, [NotNull] Operation dependantOperation)
        {
            return LoadDependantPass(sourceOperation, dependantOperation);
        }

        public static bool RunPostPass([NotNull] Operation sourceOperation, [NotNull] Operation dependantOperation)
        {
            return StoreDependantPass(sourceOperation, dependantOperation);
        }

        private static void TryEliminateBitwiseAnd(Operation operation)
        {
            // Try to recognize and optimize those 3 patterns (in order):
            // x & 0xFFFFFFFF == x,          0xFFFFFFFF & y == y,
            // x & 0x00000000 == 0x00000000, 0x00000000 & y == 0x00000000
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(x, AllOnes(x.Type)))
            {
                operation.TurnIntoCopy(y);
            }
            else if (IsConstEqual(y, AllOnes(y.Type)))
            {
                operation.TurnIntoCopy(x);
            }
            else if (IsConstEqual(x, 0) || IsConstEqual(y, 0))
            {
                operation.TurnIntoCopy(Const(0));
            }
        }

        private static void TryEliminateBitwiseOr(Operation operation)
        {
            // Try to recognize and optimize those 3 patterns (in order):
            // x | 0x00000000 == x,          0x00000000 | y == y,
            // x | 0xFFFFFFFF == 0xFFFFFFFF, 0xFFFFFFFF | y == 0xFFFFFFFF
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(x, 0))
            {
                operation.TurnIntoCopy(y);
            }
            else if (IsConstEqual(y, 0))
            {
                operation.TurnIntoCopy(x);
            }
            else if (IsConstEqual(x, AllOnes(x.Type)) || IsConstEqual(y, AllOnes(y.Type)))
            {
                operation.TurnIntoCopy(Const(AllOnes(x.Type)));
            }
        }

        private static void TryEliminateBitwiseExclusiveOr(Operation operation)
        {
            // Try to recognize and optimize those 2 patterns (in order):
            // x ^ y == 0x00000000 when x == y
            // 0x00000000 ^ y == y, x ^ 0x00000000 == x
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (x == y && x.Type.IsInteger())
            {
                operation.TurnIntoCopy(Const(x.Type, 0));
            }
            else
            {
                TryEliminateBinaryOpComutative(operation, 0);
            }
        }

        private static void TryEliminateBinaryOpY(Operation operation, ulong comparand)
        {
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(y, comparand))
            {
                operation.TurnIntoCopy(x);
            }
        }

        private static void TryEliminateBinaryOpComutative(Operation operation, ulong comparand)
        {
            Operand x = operation.GetSource(0);
            Operand y = operation.GetSource(1);

            if (IsConstEqual(x, comparand))
            {
                operation.TurnIntoCopy(y);
            }
            else if (IsConstEqual(y, comparand))
            {
                operation.TurnIntoCopy(x);
            }
        }

        private static void TryEliminateConditionalSelect(Operation operation)
        {
            Operand cond = operation.GetSource(0);

            if (cond.Kind != OperandKind.Constant)
            {
                return;
            }

            // The condition is constant, we can turn it into a copy, and select
            // the source based on the condition value.
            int srcIndex = (cond.Value != 0) ? 1 : 2;

            Operand source = operation.GetSource(srcIndex);

            operation.TurnIntoCopy(source);
        }

        private static bool IsConstEqual(Operand operand, ulong comparand)
        {
            if (operand.Kind != OperandKind.Constant || !operand.Type.IsInteger())
            {
                return false;
            }

            return operand.Value == comparand;
        }

        private static ulong AllOnes(OperandType type)
        {
            return type switch
            {
                OperandType.I32 => uint.MaxValue,
                OperandType.I64 => ulong.MaxValue,
                _ => throw new ArgumentException("Invalid operand type \"" + type + "\"."),
            };
        }
    }
}