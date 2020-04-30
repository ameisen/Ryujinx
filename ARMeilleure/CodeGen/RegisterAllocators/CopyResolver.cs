using ARMeilleure.IntermediateRepresentation;
using System;
using System.Collections.Generic;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    sealed class CopyResolver
    {
        private struct ParallelCopy
        {
            private readonly struct Copy
            {
                public readonly Register Dest   { get; }
                public readonly Register Source { get; }

                public readonly OperandType Type { get; }

                public Copy(Register dest, Register source, OperandType type)
                {
                    Dest   = dest;
                    Source = source;
                    Type   = type;
                }
            }

            private readonly List<Copy> _copies;

            public int Count => _copies.Count;

            public ParallelCopy(int capacity = 0)
            {
                _copies = new List<Copy>(capacity: capacity);
            }

            public void AddCopy(Register dest, Register source, OperandType type)
            {
                _copies.Add(new Copy(dest, source, type));
            }

            public void Sequence(List<Operation> sequence)
            {
                var locations = new Dictionary<Register, Register>(capacity: _copies.Count);
                var sources   = new Dictionary<Register, Register>(capacity: _copies.Count);

                var types = new Dictionary<Register, OperandType>(capacity: _copies.Count);

                var pendingQueue = new Queue<Register>(capacity: _copies.Count);
                var readyQueue   = new Queue<Register>(capacity: _copies.Count);

                foreach (Copy copy in _copies)
                {
                    locations[copy.Source] = copy.Source;
                    sources[copy.Dest]     = copy.Source;
                    types[copy.Dest]       = copy.Type;

                    pendingQueue.Enqueue(copy.Dest);
                }

                foreach (Copy copy in _copies)
                {
                    // If the destination is not used anywhere, we can assign it immediately.
                    if (!locations.ContainsKey(copy.Dest))
                    {
                        readyQueue.Enqueue(copy.Dest);
                    }
                }

                while (pendingQueue.TryDequeue(out Register current))
                {
                    Register copyDest;
                    Register origSource;
                    Register copySource;

                    while (readyQueue.TryDequeue(out copyDest))
                    {
                        origSource = sources[copyDest];
                        copySource = locations[origSource];

                        OperandType type = types[copyDest];

                        EmitCopy(sequence, GetRegister(copyDest, type), GetRegister(copySource, type));

                        locations[origSource] = copyDest;

                        if (origSource == copySource && sources.ContainsKey(origSource))
                        {
                            readyQueue.Enqueue(origSource);
                        }
                    }

                    copyDest   = current;
                    origSource = sources[copyDest];
                    copySource = locations[origSource];

                    if (copyDest != copySource)
                    {
                        OperandType type = types[copyDest];

                        bool isInteger = type.IsInteger();
                        type = isInteger ? OperandType.I64 : OperandType.V128;

                        EmitSwap(isInteger, sequence, GetRegister(copyDest, type), GetRegister(copySource, type));

                        locations[origSource] = copyDest;

                        Register swapOther = copySource;

                        if (copyDest != locations[sources[copySource]])
                        {
                            // Find the other swap destination register.
                            // To do that, we search all the pending registers, and pick
                            // the one where the copy source register is equal to the
                            // current destination register being processed (copyDest).
                            foreach (Register pending in pendingQueue)
                            {
                                // Is this a copy of pending <- copyDest?
                                if (copyDest == locations[sources[pending]])
                                {
                                    swapOther = pending;

                                    break;
                                }
                            }
                        }

                        // The value that was previously at "copyDest" now lives on
                        // "copySource" thanks to the swap, now we need to update the
                        // location for the next copy that is supposed to copy the value
                        // that used to live on "copyDest".
                        locations[sources[swapOther]] = copySource;
                    }
                }
            }

            private static void EmitSwap(bool isInteger, List<Operation> sequence, Operand x, Operand y)
            {
                if (!isInteger)
                {
                    EmitXorSwap(sequence, x, y);
                }
                else
                {
                    EmitExchange(sequence, x, y);
                }
            }

            private static void EmitCopy(List<Operation> sequence, Operand x, Operand y)
            {
                sequence.Add(Operation(Instruction.Copy, x, y));
            }

            private static void EmitExchange(List<Operation> sequence, Operand x, Operand y)
            {
                sequence.Add(Operation(Instruction.Exchange, x, y));
            }

            private static void EmitXorSwap(List<Operation> sequence, Operand x, Operand y)
            {
                sequence.Add(Operation(Instruction.BitwiseExclusiveOr, x, x, y));
                sequence.Add(Operation(Instruction.BitwiseExclusiveOr, y, y, x));
                sequence.Add(Operation(Instruction.BitwiseExclusiveOr, x, x, y));
            }
        }

        private readonly Queue<Operation> _fillQueue  = new Queue<Operation>();
        private readonly Queue<Operation> _spillQueue = new Queue<Operation>();

        private readonly ParallelCopy _parallelCopy;

        public bool HasCopy { get; private set; }

        public CopyResolver(int capacity = 0)
        {
            _fillQueue  = new Queue<Operation>();
            _spillQueue = new Queue<Operation>();

            _parallelCopy = new ParallelCopy(capacity: capacity);
        }

        public void AddSplit(LiveInterval left, LiveInterval right)
        {
            if (left.Local != right.Local)
            {
                throw new ArgumentException("Intervals of different variables are not allowed.");
            }

            OperandType type = left.Local.Type;

            if (left.IsSpilled && !right.IsSpilled)
            {
                // Move from the stack to a register.
                AddSplitFill(left, right, type);
            }
            else if (!left.IsSpilled && right.IsSpilled)
            {
                // Move from a register to the stack.
                AddSplitSpill(left, right, type);
            }
            else if (!left.IsSpilled && !right.IsSpilled && left.Register != right.Register)
            {
                // Move from one register to another.
                AddSplitCopy(left, right, type);
            }
            else if (left.SpillOffset != right.SpillOffset)
            {
                // This would be the stack-to-stack move case, but this is not supported.
                throw new ArgumentException("Both intervals were spilled.");
            }
        }

        private void AddSplitFill(LiveInterval left, LiveInterval right, OperandType type)
        {
            Operand register = GetRegister(right.Register, type);

            Operand offset = Const(left.SpillOffset);

            _fillQueue.Enqueue(Operation(Instruction.Fill, register, offset));

            HasCopy = true;
        }

        private void AddSplitSpill(LiveInterval left, LiveInterval right, OperandType type)
        {
            Operand offset = Const(right.SpillOffset);

            Operand register = GetRegister(left.Register, type);

            _spillQueue.Enqueue(Operation(Instruction.Spill, null, offset, register));

            HasCopy = true;
        }

        private void AddSplitCopy(LiveInterval left, LiveInterval right, OperandType type)
        {
            _parallelCopy.AddCopy(right.Register, left.Register, type);

            HasCopy = true;
        }

        public List<Operation> Sequence()
        {
            var sequence = new List<Operation>();

            while (_spillQueue.TryDequeue(out Operation spillOp))
            {
                sequence.Add(spillOp);
            }

            _parallelCopy.Sequence(sequence);

            while (_fillQueue.TryDequeue(out Operation fillOp))
            {
                sequence.Add(fillOp);
            }

            return sequence;
        }

        private static Operand GetRegister(Register reg, OperandType type)
        {
            return Register(reg.Index, reg.Type, type);
        }
    }
}