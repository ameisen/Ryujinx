using ARMeilleure.Common;
using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    // Based on:
    // "Linear Scan Register Allocation for the Java(tm) HotSpot Client Compiler".
    // http://www.christianwimmer.at/Publications/Wimmer04a/Wimmer04a.pdf
    sealed class LinearScanAllocator : IRegisterAllocator
    {
        private const int InstructionGap     = 2;
        private const int InstructionGapMask = InstructionGap - 1;

        private const int RegistersCount = 16;

        private HashSet<int> _blockEdges;

        private LiveRange[] _blockRanges;

        private BitMap[] _blockLiveIn;

        private List<LiveInterval> _intervals;

        private LiveInterval[] _parentIntervals;

        private List<(IntrusiveList<Node>, Node)> _operationNodes;

        private int _operationsCount;

        private struct AllocationContext : IDisposable
        {
            public readonly RegisterMasks Masks { get; }

            public readonly StackAllocator StackAlloc { get; }

            public readonly BitMap Active   { get; }
            public readonly BitMap Inactive { get; }

            public int IntUsedRegisters { readonly get; set; }
            public int VecUsedRegisters { readonly get; set; }

            [MethodImpl(MethodOptions.FastInline)]
            public AllocationContext(StackAllocator stackAlloc, in RegisterMasks masks, int intervalsCount)
            {
                StackAlloc = stackAlloc;
                Masks      = masks;

                Active   = BitMapPool.Allocate(intervalsCount);
                Inactive = BitMapPool.Allocate(intervalsCount);

                IntUsedRegisters = 0;
                VecUsedRegisters = 0;
            }

            [MethodImpl(MethodOptions.FastInline)]
            public readonly void MoveActiveToInactive(int bit)
            {
                Move(Active, Inactive, bit);
            }

            [MethodImpl(MethodOptions.FastInline)]
            public readonly void MoveInactiveToActive(int bit)
            {
                Move(Inactive, Active, bit);
            }

            [MethodImpl(MethodOptions.FastInline)]
            private static void Move(BitMap source, BitMap dest, int bit)
            {
                source.Clear(bit);

                dest.Set(bit);
            }

            [MethodImpl(MethodOptions.FastInline)]
            public readonly void Dispose()
            {
                BitMapPool.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public AllocationResult RunPass(
            ControlFlowGraph cfg,
            StackAllocator stackAlloc,
            in RegisterMasks regMasks)
        {
            NumberLocals(cfg);

            var context = new AllocationContext(stackAlloc, regMasks, _intervals.Count);

            try
            {
                BuildIntervals(cfg, context);

                for (int index = 0; index < _intervals.Count; index++)
                {
                    LiveInterval current = _intervals[index];

                    if (current.IsEmpty)
                    {
                        continue;
                    }

                    if (current.IsFixed)
                    {
                        context.Active.Set(index);

                        if (current.Register.Type == RegisterType.Integer)
                        {
                            context.IntUsedRegisters |= 1 << current.Register.Index;
                        }
                        else /* if (interval.Register.Type == RegisterType.Vector) */
                        {
                            context.VecUsedRegisters |= 1 << current.Register.Index;
                        }

                        continue;
                    }

                    AllocateInterval(ref context, current, index);
                }

                for (int index = RegistersCount * 2; index < _intervals.Count; index++)
                {
                    if (!_intervals[index].IsSpilled)
                    {
                        ReplaceLocalWithRegister(_intervals[index]);
                    }
                }

                InsertSplitCopies();
                InsertSplitCopiesAtEdges(cfg);

                return new AllocationResult(
                    context.IntUsedRegisters,
                    context.VecUsedRegisters,
                    context.StackAlloc.TotalSize
                );
            }
            finally
            {
                context.Dispose();
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void AllocateInterval(ref AllocationContext context, LiveInterval current, int cIndex)
        {
            // Check active intervals that already ended.
            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.GetEnd() < current.GetStart())
                {
                    context.Active.Clear(iIndex);
                }
                else if (!interval.Overlaps(current.GetStart()))
                {
                    context.MoveActiveToInactive(iIndex);
                }
            }

            // Check inactive intervals that already ended or were reactivated.
            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.GetEnd() < current.GetStart())
                {
                    context.Inactive.Clear(iIndex);
                }
                else if (interval.Overlaps(current.GetStart()))
                {
                    context.MoveInactiveToActive(iIndex);
                }
            }

            if (!TryAllocateRegWithoutSpill(ref context, current, cIndex))
            {
                AllocateRegWithSpill(context, current, cIndex);
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private bool TryAllocateRegWithoutSpill(ref AllocationContext context, LiveInterval current, int cIndex)
        {
            RegisterType regType = current.Local.Type.ToRegisterType();

            int availableRegisters = context.Masks.GetAvailableRegisters(regType);

            Span<int> freePositions = stackalloc int[RegistersCount];

            for (int index = 0; index < RegistersCount; index++)
            {
                if ((availableRegisters & (1 << index)) != 0)
                {
                    freePositions[index] = int.MaxValue;
                }
            }

            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.Register.Type == regType)
                {
                    freePositions[interval.Register.Index] = 0;
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.Register.Type == regType)
                {
                    int overlapPosition = interval.GetOverlapPosition(current);

                    if (overlapPosition != LiveInterval.NotFound && freePositions[interval.Register.Index] > overlapPosition)
                    {
                        freePositions[interval.Register.Index] = overlapPosition;
                    }
                }
            }

            int selectedReg = GetHighestValueIndex(freePositions);

            int selectedNextUse = freePositions[selectedReg];

            // Intervals starts and ends at odd positions, unless they span an entire
            // block, in this case they will have ranges at a even position.
            // When a interval is loaded from the stack to a register, we can only
            // do the split at a odd position, because otherwise the split interval
            // that is inserted on the list to be processed may clobber a register
            // used by the instruction at the same position as the split.
            // The problem only happens when a interval ends exactly at this instruction,
            // because otherwise they would interfere, and the register wouldn't be selected.
            // When the interval is aligned and the above happens, there's no problem as
            // the instruction that is actually with the last use is the one
            // before that position.
            selectedNextUse &= ~InstructionGapMask;

            if (selectedNextUse <= current.GetStart())
            {
                return false;
            }
            else if (selectedNextUse < current.GetEnd())
            {
                Debug.Assert(selectedNextUse > current.GetStart(), "Trying to split interval at the start.");

                LiveInterval splitChild = current.Split(selectedNextUse);

                if (splitChild.UsesCount != 0)
                {
                    Debug.Assert(splitChild.GetStart() > current.GetStart(), "Split interval has an invalid start position.");

                    InsertInterval(splitChild);
                }
                else
                {
                    Spill(context, splitChild);
                }
            }

            current.Register = new Register(selectedReg, regType);

            if (regType == RegisterType.Integer)
            {
                context.IntUsedRegisters |= 1 << selectedReg;
            }
            else /* if (regType == RegisterType.Vector) */
            {
                context.VecUsedRegisters |= 1 << selectedReg;
            }

            context.Active.Set(cIndex);

            return true;
        }

        [MethodImpl(MethodOptions.FastInline)]
        private unsafe void AllocateRegWithSpill(in AllocationContext context, LiveInterval current, int cIndex)
        {
            RegisterType regType = current.Local.Type.ToRegisterType();

            int availableRegisters = context.Masks.GetAvailableRegisters(regType);

            var usePositions     = stackalloc int[RegistersCount];
            var blockedPositions = stackalloc int[RegistersCount];

            for (int index = 0; index < RegistersCount; index++)
            {
                if ((availableRegisters & (1 << index)) != 0)
                {
                    usePositions[index] = int.MaxValue;

                    blockedPositions[index] = int.MaxValue;
                }
            }

            void SetUsePosition(int index, int position)
            {
                usePositions[index] = Math.Min(usePositions[index], position);
            }

            void SetBlockedPosition(int index, int position)
            {
                blockedPositions[index] = Math.Min(blockedPositions[index], position);

                SetUsePosition(index, position);
            }

            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register.Type == regType)
                {
                    int nextUse = interval.NextUseAfter(current.GetStart());

                    if (nextUse != -1)
                    {
                        SetUsePosition(interval.Register.Index, nextUse);
                    }
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register.Type == regType && interval.Overlaps(current))
                {
                    int nextUse = interval.NextUseAfter(current.GetStart());

                    if (nextUse != -1)
                    {
                        SetUsePosition(interval.Register.Index, nextUse);
                    }
                }
            }

            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.IsFixed && interval.Register.Type == regType)
                {
                    SetBlockedPosition(interval.Register.Index, 0);
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.IsFixed && interval.Register.Type == regType)
                {
                    int overlapPosition = interval.GetOverlapPosition(current);

                    if (overlapPosition != LiveInterval.NotFound)
                    {
                        SetBlockedPosition(interval.Register.Index, overlapPosition);
                    }
                }
            }

            int selectedReg = GetHighestValueIndex(new ReadOnlySpan<int>(usePositions, RegistersCount));

            int currentFirstUse = current.FirstUse();

            Debug.Assert(currentFirstUse >= 0, "Current interval has no uses.");

            if (usePositions[selectedReg] < currentFirstUse)
            {
                // All intervals on inactive and active are being used before current,
                // so spill the current interval.
                Debug.Assert(currentFirstUse > current.GetStart(), "Trying to spill a interval currently being used.");

                LiveInterval splitChild = current.Split(currentFirstUse);

                Debug.Assert(splitChild.GetStart() > current.GetStart(), "Split interval has an invalid start position.");

                InsertInterval(splitChild);

                Spill(context, current);
            }
            else if (blockedPositions[selectedReg] > current.GetEnd())
            {
                // Spill made the register available for the entire current lifetime,
                // so we only need to split the intervals using the selected register.
                current.Register = new Register(selectedReg, regType);

                SplitAndSpillOverlappingIntervals(context, current);

                context.Active.Set(cIndex);
            }
            else
            {
                // There are conflicts even after spill due to the use of fixed registers
                // that can't be spilled, so we need to also split current at the point of
                // the first fixed register use.
                current.Register = new Register(selectedReg, regType);

                int splitPosition = blockedPositions[selectedReg] & ~InstructionGapMask;

                Debug.Assert(splitPosition > current.GetStart(), "Trying to split a interval at a invalid position.");

                LiveInterval splitChild = current.Split(splitPosition);

                if (splitChild.UsesCount != 0)
                {
                    Debug.Assert(splitChild.GetStart() > current.GetStart(), "Split interval has an invalid start position.");

                    InsertInterval(splitChild);
                }
                else
                {
                    Spill(context, splitChild);
                }

                SplitAndSpillOverlappingIntervals(context, current);

                context.Active.Set(cIndex);
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static int GetHighestValueIndex(in ReadOnlySpan<int> array)
        {
            int highest = array[0];

            if (highest == int.MaxValue)
            {
                return 0;
            }

            int selected = 0;

            for (int index = 1; index < array.Length; index++)
            {
                int current = array[index];

                if (highest < current)
                {
                    highest  = current;
                    selected = index;

                    if (current == int.MaxValue)
                    {
                        break;
                    }
                }
            }

            return selected;
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void SplitAndSpillOverlappingIntervals(in AllocationContext context, LiveInterval current)
        {
            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register == current.Register)
                {
                    SplitAndSpillOverlappingInterval(context, current, interval);

                    context.Active.Clear(iIndex);
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register == current.Register && interval.Overlaps(current))
                {
                    SplitAndSpillOverlappingInterval(context, current, interval);

                    context.Inactive.Clear(iIndex);
                }
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void SplitAndSpillOverlappingInterval(
            in AllocationContext context,
            LiveInterval      current,
            LiveInterval      interval)
        {
            // If there's a next use after the start of the current interval,
            // we need to split the spilled interval twice, and re-insert it
            // on the "pending" list to ensure that it will get a new register
            // on that use position.
            int nextUse = interval.NextUseAfter(current.GetStart());

            LiveInterval splitChild;

            if (interval.GetStart() < current.GetStart())
            {
                splitChild = interval.Split(current.GetStart());
            }
            else
            {
                splitChild = interval;
            }

            if (nextUse != -1)
            {
                Debug.Assert(nextUse > current.GetStart(), "Trying to spill a interval currently being used.");

                if (nextUse > splitChild.GetStart())
                {
                    LiveInterval right = splitChild.Split(nextUse);

                    Spill(context, splitChild);

                    splitChild = right;
                }

                InsertInterval(splitChild);
            }
            else
            {
                Spill(context, splitChild);
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void InsertInterval(LiveInterval interval)
        {
            Debug.Assert(interval.UsesCount != 0, "Trying to insert a interval without uses.");
            Debug.Assert(!interval.IsEmpty,       "Trying to insert a empty interval.");
            Debug.Assert(!interval.IsSpilled,     "Trying to insert a spilled interval.");

            int startIndex = RegistersCount * 2;

            int insertIndex = _intervals.BinarySearch(startIndex, _intervals.Count - startIndex, interval, null);

            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }

            _intervals.Insert(insertIndex, interval);
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void Spill(in AllocationContext context, LiveInterval interval)
        {
            Debug.Assert(!interval.IsFixed,       "Trying to spill a fixed interval.");
            Debug.Assert(interval.UsesCount == 0, "Trying to spill a interval with uses.");

            // We first check if any of the siblings were spilled, if so we can reuse
            // the stack offset. Otherwise, we allocate a new space on the stack.
            // This prevents stack-to-stack copies being necessary for a split interval.
            if (!interval.TrySpillWithSiblingOffset())
            {
                interval.Spill(context.StackAlloc.Allocate(interval.Local.Type));
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void InsertSplitCopies()
        {
            var copyResolvers = new Dictionary<int, CopyResolver>();

            CopyResolver GetCopyResolver(int position)
            {
                if (copyResolvers.TryGetValue(position, out var copyResolver))
                {
                    return copyResolver;
                }
                copyResolver = new CopyResolver();
                copyResolvers.Add(position, copyResolver);
                return copyResolver;
            }

            foreach (LiveInterval interval in _intervals)
            {
                if (!interval.IsSplit)
                {
                    continue;
                }

                LiveInterval previous = interval;

                foreach (LiveInterval splitChild in interval.SplitChilds())
                {
                    int splitPosition = splitChild.GetStart();

                    if (!_blockEdges.Contains(splitPosition) && previous.GetEnd() == splitPosition)
                    {
                        GetCopyResolver(splitPosition).AddSplit(previous, splitChild);
                    }

                    previous = splitChild;
                }
            }

            foreach (KeyValuePair<int, CopyResolver> kv in copyResolvers)
            {
                CopyResolver copyResolver = kv.Value;

                if (!copyResolver.HasCopy)
                {
                    continue;
                }

                int splitPosition = kv.Key;

                (IntrusiveList<Node> nodes, Node node) = GetOperationNode(splitPosition);

                var sequence = copyResolver.Sequence();

                nodes.AddBefore(node, sequence[0]);

                node = sequence[0];

                for (int index = 1; index < sequence.Count; index++)
                {
                    nodes.AddAfter(node, sequence[index]);

                    node = sequence[index];
                }
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void InsertSplitCopiesAtEdges(ControlFlowGraph cfg)
        {
            int blocksCount = cfg.Blocks.Count;

            bool IsSplitEdgeBlock(BasicBlock block)
            {
                return block.Index >= blocksCount;
            }

            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                if (IsSplitEdgeBlock(block))
                {
                    continue;
                }

                bool hasSingleOrNoSuccessor = block.Next == null || block.Branch == null;

                for (int i = 0; i < 2; i++)
                {
                    // This used to use an enumerable, but it ended up generating a lot of garbage, so now it is a loop.
                    BasicBlock successor = (i == 0) ? block.Next : block.Branch;
                    if (successor == null)
                    {
                        continue;
                    }

                    int succIndex = successor.Index;

                    // If the current node is a split node, then the actual successor node
                    // (the successor before the split) should be right after it.
                    if (IsSplitEdgeBlock(successor))
                    {
                        succIndex = FirstSuccessor(successor).Index;
                    }

                    var copyResolver = new CopyResolver();

                    foreach (int iIndex in _blockLiveIn[succIndex])
                    {
                        LiveInterval interval = _parentIntervals[iIndex];

                        if (!interval.IsSplit)
                        {
                            continue;
                        }

                        int lEnd   = _blockRanges[block.Index].End - 1;
                        int rStart = _blockRanges[succIndex].Start;

                        LiveInterval left  = interval.GetSplitChild(lEnd);
                        LiveInterval right = interval.GetSplitChild(rStart);

                        if (left != null && right != null && left != right)
                        {
                            copyResolver.AddSplit(left, right);
                        }
                    }

                    if (!copyResolver.HasCopy)
                    {
                        continue;
                    }

                    var sequence = copyResolver.Sequence();

                    if (hasSingleOrNoSuccessor)
                    {
                        foreach (Operation operation in sequence)
                        {
                            block.Append(operation);
                        }
                    }
                    else if (successor.Predecessors.Count == 1)
                    {
                        successor.Operations.AddFirst(sequence[0]);

                        Node prependNode = sequence[0];

                        for (int index = 1; index < sequence.Count; index++)
                        {
                            Operation operation = sequence[index];

                            successor.Operations.AddAfter(prependNode, operation);

                            prependNode = operation;
                        }
                    }
                    else
                    {
                        // Split the critical edge.
                        BasicBlock splitBlock = cfg.SplitEdge(block, successor);

                        foreach (Operation operation in sequence)
                        {
                            splitBlock.Append(operation);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static void UpdateMemoryOperand(Operand operand, Operand register, Operand local)
        {
            var memOp = (MemoryOperand)operand;

            if (memOp.BaseAddress == local)
            {
                memOp.BaseAddress = register;
            }

            if (memOp.Index == local)
            {
                memOp.Index = register;
            }
        }

        [Conditional("DEBUG")]
        private static void AssertNotMemoryOperand(Operand operand) => Debug.Assert(operand.Kind != OperandKind.Memory);

        [MethodImpl(MethodOptions.FastInline)]
        private void ReplaceLocalWithRegister(LiveInterval current)
        {
            Operand register = GetRegister(current);

            var usePositions = current.UsePositions();
            for (int i = usePositions.Count; i-- > 0;)
            {
                int usePosition = -usePositions[i];
                (_, Node operation) = GetOperationNode(usePosition);

                for (int index = 0; index < operation.SourcesCount; index++)
                {
                    Operand operand = operation.GetSource(index);

                    if (operand == current.Local)
                    {
                        AssertNotMemoryOperand(operand);
                        operation.SetSource(index, register);
                    }
                    else if (operand.Kind == OperandKind.Memory)
                    {
                        UpdateMemoryOperand(operand, register, current.Local);
                    }
                }

                for (int index = 0; index < operation.DestinationsCount; index++)
                {
                    Operand operand = operation.GetDestination(index);

                    if (operand == current.Local)
                    {
                        AssertNotMemoryOperand(operand);
                        operation.SetDestination(index, register);
                    }
                    else if (operand.Kind == OperandKind.Memory)
                    {
                        UpdateMemoryOperand(operand, register, current.Local);
                    }
                }
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static Operand GetRegister(LiveInterval interval)
        {
            Debug.Assert(!interval.IsSpilled, "Spilled intervals are not allowed.");

            return OperandHelper.Register(
                interval.Register.Index,
                interval.Register.Type,
                interval.Local.Type);
        }

        [MethodImpl(MethodOptions.FastInline)]
        private (IntrusiveList<Node>, Node) GetOperationNode(int position)
        {
            return _operationNodes[position / InstructionGap];
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void NumberLocals(ControlFlowGraph cfg)
        {
            _operationNodes = new List<(IntrusiveList<Node>, Node)>();

            _intervals = new List<LiveInterval>();

            for (int index = 0; index < RegistersCount; index++)
            {
                _intervals.Add(new LiveInterval(new Register(index, RegisterType.Integer)));
                _intervals.Add(new LiveInterval(new Register(index, RegisterType.Vector)));
            }

            HashSet<Operand> visited = new HashSet<Operand>();

            _operationsCount = 0;

            for (int index = cfg.PostOrderBlocks.Length - 1; index >= 0; index--)
            {
                BasicBlock block = cfg.PostOrderBlocks[index];

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    _operationNodes.Add((block.Operations, node));

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);
                        if (dest.Kind == OperandKind.LocalVariable && visited.Add(dest))
                        {
                            dest.NumberLocal(_intervals.Count);

                            _intervals.Add(new LiveInterval(dest));
                        }
                    }
                }

                _operationsCount += block.Operations.Count * InstructionGap;

                if (block.Operations.Count == 0)
                {
                    // Pretend we have a dummy instruction on the empty block.
                    _operationNodes.Add((null, null));

                    _operationsCount += InstructionGap;
                }
            }

            _parentIntervals = _intervals.ToArray();
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void BuildIntervals(ControlFlowGraph cfg, in AllocationContext context)
        {
            _blockRanges = new LiveRange[cfg.Blocks.Count];

            int mapSize = _intervals.Count;

            BitMap[] blkLiveGen  = new BitMap[cfg.Blocks.Count];
            BitMap[] blkLiveKill = new BitMap[cfg.Blocks.Count];

            // Compute local live sets.
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                BitMap liveGen  = BitMapPool.Allocate(mapSize);
                BitMap liveKill = BitMapPool.Allocate(mapSize);

                void HandleSource(Operand source)
                {
                    int id = GetOperandId(source);

                    if (!liveKill.IsSet(id))
                    {
                        liveGen.Set(id);
                    }
                }

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    Sources(node, HandleSource);

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);
                        if (dest.Kind == OperandKind.Memory)
                        {
                            // Memory operands act more like sources than destinations.
                            HandleMemorySource(dest, HandleSource);
                            continue;
                        }
                        liveKill.Set(GetOperandId(dest));
                    }
                }

                blkLiveGen [block.Index] = liveGen;
                blkLiveKill[block.Index] = liveKill;
            }

            // Compute global live sets.
            var blkLiveIn  = new BitMap[cfg.Blocks.Count];
            var blkLiveOut = new BitMap[cfg.Blocks.Count];

            for (int index = 0; index < cfg.Blocks.Count; index++)
            {
                blkLiveIn [index] = BitMapPool.Allocate(mapSize);
                blkLiveOut[index] = BitMapPool.Allocate(mapSize);
            }

            bool modified;

            do
            {
                modified = false;

                foreach (var block in cfg.PostOrderBlocks)
                {
                    BitMap liveOut = blkLiveOut[block.Index];

                    if ((block.Next != null && liveOut.Set(blkLiveIn[block.Next.Index])) || 
                        (block.Branch != null && liveOut.Set(blkLiveIn[block.Branch.Index])))
                    {
                        modified = true;
                    }

                    BitMap liveIn = blkLiveIn[block.Index];

                    liveIn.Set  (liveOut);
                    liveIn.Clear(blkLiveKill[block.Index]);
                    liveIn.Set  (blkLiveGen [block.Index]);
                }
            }
            while (modified);

            _blockLiveIn = blkLiveIn;

            _blockEdges = new HashSet<int>();

            // Compute lifetime intervals.
            int operationPos = _operationsCount;

            foreach (var block in cfg.PostOrderBlocks)
            {
                // We handle empty blocks by pretending they have a dummy instruction,
                // because otherwise the block would have the same start and end position,
                // and this is not valid.
                int instCount = Math.Max(block.Operations.Count, 1);

                int blockStart = operationPos - instCount * InstructionGap;
                int blockEnd   = operationPos;

                _blockRanges[block.Index] = new LiveRange(blockStart, blockEnd);

                _blockEdges.Add(blockStart);

                BitMap liveOut = blkLiveOut[block.Index];

                foreach (int id in liveOut)
                {
                    _intervals[id].AddRange(blockStart, blockEnd);
                }

                if (block.Operations.Count == 0)
                {
                    operationPos -= InstructionGap;

                    continue;
                }

                foreach (Node node in BottomOperations(block))
                {
                    operationPos -= InstructionGap;

                    void HandleSource(Operand source)
                    {
                        LiveInterval interval = _intervals[GetOperandId(source)];

                        interval.AddRange(blockStart, operationPos + 1);
                        interval.AddUsePosition(operationPos);
                    }

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);

                        // Memory destinations are really 'sources', in a way.
                        // They act more as an address source for the output.
                        if (dest.Kind == OperandKind.Memory)
                        {
                            HandleMemorySource(dest, HandleSource);
                            continue;
                        }

                        LiveInterval interval = _intervals[GetOperandId(dest)];

                        interval.SetStart(operationPos + 1);
                        interval.AddUsePosition(operationPos + 1);
                    }

                    Sources(node, HandleSource);

                    if (node is Operation operation && operation.Instruction == Instruction.Call)
                    {
                        AddIntervalCallerSavedReg(context.Masks.IntCallerSavedRegisters, operationPos, RegisterType.Integer);
                        AddIntervalCallerSavedReg(context.Masks.VecCallerSavedRegisters, operationPos, RegisterType.Vector);
                    }
                }
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void AddIntervalCallerSavedReg(int mask, int operationPos, RegisterType regType)
        {
            while (mask != 0)
            {
                int regIndex = BitOperations.TrailingZeroCount(mask);

                var callerSavedReg = new Register(regIndex, regType);

                LiveInterval interval = _intervals[GetRegisterId(callerSavedReg)];

                interval.AddRange(operationPos + 1, operationPos + InstructionGap);

                mask &= ~(1 << regIndex);
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static int GetOperandId(Operand operand) => operand.Kind switch
        {
            OperandKind.LocalVariable => operand.AsInt32(),
            OperandKind.Register =>      GetRegisterId(operand.GetRegister()),
            _ => throw new ArgumentException($"Invalid operand kind \"{operand.Kind}\"."),
        };

        [MethodImpl(MethodOptions.FastInline)]
        private static int GetRegisterId(Register register)
        {
            // Equivalent to (b ? 1 : 0)
            bool isVector = register.Type == RegisterType.Vector;
            int isVectorBit = Unsafe.As<bool, byte>(ref isVector);

            return (register.Index << 1) | isVectorBit;
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static BasicBlock FirstSuccessor(BasicBlock block)
        {
            return block.Next ?? block.Branch;
        }

        private ref struct BottomNodeEnumerator
        {
            private readonly Node Start;
            public Node Current { readonly get; private set; }

            public BottomNodeEnumerator(Node node)
            {
                Start = node;
                Current = null;
            }

            [MethodImpl(MethodOptions.FastInline)]
            public bool MoveNext()
            {
                var next = (Current != null) ? Current.ListPrevious : Start;
                if (next == null || next is PhiNode)
                {
                    return false;
                }
                Current = next;
                return true;
            }

            [MethodImpl(MethodOptions.FastInline)]
            public readonly BottomNodeEnumerator GetEnumerator() => this;
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static BottomNodeEnumerator BottomOperations(BasicBlock block)
        {
            return new BottomNodeEnumerator(block.Operations.Last);
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static void HandleMemorySource(Operand source, Action<Operand> action)
        {
            var memOp = (MemoryOperand)source;

            if (memOp.BaseAddress != null)
            {
                // If this gets hit, we have to handle dependant accesses
                AssertNotMemoryOperand(memOp.BaseAddress);
                action(memOp.BaseAddress);
            }

            if (memOp.Index != null)
            {
                // If this gets hit, we have to handle dependant accesses
                AssertNotMemoryOperand(memOp.Index);
                action(memOp.Index);
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        private static void Sources(Node node, Action<Operand> action)
        {
            for (int index = 0; index < node.SourcesCount; index++)
            {
                var source = node.GetSource(index);

                switch (source.Kind)
                {
                    case OperandKind.LocalVariable:
                    case OperandKind.Register:
                        action(source);
                        break;

                    case OperandKind.Memory:
                        HandleMemorySource(source, action);
                        break;
                }
            }
        }
    }
}