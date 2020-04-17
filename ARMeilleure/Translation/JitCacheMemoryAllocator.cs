using ARMeilleure.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ARMeilleure.Translation
{
    sealed class JitCacheMemoryAllocator
    {
        private const int FreeDelay = 5 * 60; // in seconds

        private sealed class Block
        {
            [Range(0, int.MaxValue)] internal int Start;
            [Range(0, int.MaxValue)] internal int End;
            [Range(0, int.MaxValue)] internal int Extent => End - Start;
            internal bool Allocated;

            internal Block(int start, int extent, bool allocated)
            {
                Start = start;
                End = start + extent;
                Allocated = allocated;
            }
        }

        private readonly struct FreeNode
        {
            internal readonly DateTime FreeTime;
            [NotNull] 
            internal readonly LinkedListNode<Block> Node;

            internal FreeNode([NotNull] LinkedListNode<Block> node)
            {
                FreeTime = DateTime.Now.AddSeconds(FreeDelay);
                Node = node;
            }
        }

        [NotNull] private readonly LinkedList<Block> MemoryRanges;
        [NotNull] private readonly Queue<FreeNode> PendingFreeList;

        public JitCacheMemoryAllocator(int size, int startPosition)
        {
            MemoryRanges = new LinkedList<Block>();
            PendingFreeList = new Queue<FreeNode>();

            MemoryRanges.AddFirst(new Block(
                start: startPosition,
                extent: size,
                allocated: false
            ));
        }

        [return: MaybeNull]
        private LinkedListNode<Block> FindNode([Range(0, int.MaxValue)] int offset)
        {
            for (var node = MemoryRanges.First; node != null; node = node.Next)
            {
                if (node.Value.Start == offset)
                {
                    return node;
                }
            }

            return null;
        }

        private void FlushPendingFreeList()
        {
            var currentTime = DateTime.Now;

            while (PendingFreeList.TryPeek(out var freeNode))
            {
                if (freeNode.FreeTime > currentTime)
                {
                    break;
                }

                ReleaseNode(freeNode.Node);
                PendingFreeList.Dequeue();
            }
        }

        private void RemoveNode([NotNull] LinkedListNode<Block> node)
        {
            MemoryRanges.Remove(node);
        }

        private void ReleaseNode([NotNull] LinkedListNode<Block> node)
        {
            node.Value.Allocated = false;

            // Check to see if the block can be merged with its neighbor on the 'right'
            var nextNeighbor = node.Next;
            if (!(nextNeighbor?.Value.Allocated ?? true))
            {
                // In that case, we will remove the next node, and merge it with this one.
                node.Value.End = nextNeighbor.Value.End;
                RemoveNode(node.Next);
            }
            // Check the block on the 'left'
            var prevNeighbor = node.Previous;
            if (!(prevNeighbor?.Value.Allocated ?? true))
            {
                // In that case, we will remove the previous node, and merge it with this one.
                node.Value.Start = prevNeighbor.Value.Start;
                RemoveNode(node.Previous);
            }
        }

        public int Allocate([Range(0, int.MaxValue)] int size)
        {
            // Flush the pending free list
            FlushPendingFreeList();

            // Find the first node that fits it.
            // This is *not* an ideal algorithm, and this is a linear-time search.
            for (var node = MemoryRanges.First; node != null; node = node.Next)
            {
                var block = node.Value;

                // If the node is allocated, we can just skip it.
                if (block.Allocated)
                {
                    continue;
                }

                if (block.Extent >= size)
                {
                    // If it fits, we will allocate out of this node.
                    if (block.Extent > size)
                    {
                        // If it is larger, we should create a 'free' node out of it for the remainder.
                        var freeBlock = new Block(
                            start: block.Start + size,
                            extent: block.Extent - size,
                            allocated: false
                        );
                        MemoryRanges.AddAfter(node, freeBlock);
                        block.End -= size;
                    }

                    block.Allocated = true;
                    return block.Start;
                }
            }

            throw new OutOfMemoryException();
        }

        public void Free([Range(0, int.MaxValue)] int offset)
        {
            return;

            var node = FindNode(offset);

            if (node == null)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (!node.Value.Allocated)
            {
                throw new InvalidOperationException($"Offset {offset} is not marked as allocated");
            }

            PendingFreeList.Enqueue(new FreeNode(node));
        }
    }
}