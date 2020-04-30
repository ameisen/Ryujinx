using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class Optimizer
    {
        private static bool IsUsedOperation(Node node, out Operation operation, out bool isUnused)
        {
            if (node is null)
            {
                isUnused = true;
                operation = null;
                return false;
            }

            isUnused = IsUnused(node);
            if (node is Operation op)
            {
                operation = op;
                return true;
            }

            operation = null;
            return false;
        }

        public static void RunPass(ControlFlowGraph cfg)
        {
            bool modified;

            do
            {
                modified = false;

                bool subModified;
                do
                {
                    subModified = false;
                    for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
                    {
                        Node node = block.Operations.First;

                        while (node != null)
                        {
                            Node nextNode = node.ListNext;

                            if (!IsUsedOperation(node, out var operation, out bool isUnused))
                            {
                                if (isUnused)
                                {
                                    RemoveNode(block, node);

                                    subModified = modified = true;
                                }

                                node = nextNode;

                                continue;
                            }

                            ConstantFolding.RunPass(operation);

                            Simplification.RunPass(operation);


                            if (DestIsLocalVar(operation) && IsPropagableCopy(operation))
                            {
                                PropagateCopy(operation);

                                RemoveNode(block, node);

                                subModified = modified = true;
                            }

                            node = nextNode;
                        }
                    }
                }
                while (subModified);

                do
                {
                    subModified = false;
                    for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
                    {
                        Node node = block.Operations.First;

                        while (node != null)
                        {
                            Node nextNode = node.ListNext;

                            if (!(node is Operation operation))
                            {
                                node = nextNode;
                                continue;
                            }

                            // Now run the simplification pass on both this operation and the next operation, to see if that can be coalesced.
                            if (IsUsedOperation(nextNode, out var nextOperation, out var _))
                            {
                                if (Simplification.RunPass(operation, nextOperation))
                                {
                                    // If true is returned, the current node was eliminated.
                                    node.SetDestinations(Array.Empty<Operand>());
                                    node.SetSources(Array.Empty<Operand>());
                                    RemoveNode(block, node);
                                    node = nextNode;
                                    subModified = modified = true;
                                    continue;
                                }
                                if (Simplification.RunPostPass(operation, nextOperation))
                                {
                                    // If true is returned, the next node was eliminated
                                    var nextNextNode = nextNode.ListNext;
                                    nextNode.SetDestinations(Array.Empty<Operand>());
                                    nextNode.SetSources(Array.Empty<Operand>());
                                    RemoveNode(block, nextNode);
                                    node = nextNextNode;
                                    subModified = modified = true;
                                    continue;
                                }
                            }

                            node = nextNode;
                        }
                    }
                }
                while (subModified);
            }
            while (modified);
        }

        public static void RemoveUnusedNodes(ControlFlowGraph cfg)
        {
            bool modified;

            do
            {
                modified = false;

                for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
                {
                    Node node = block.Operations.First;

                    while (node != null)
                    {
                        Node nextNode = node.ListNext;

                        if (IsUnused(node))
                        {
                            RemoveNode(block, node);

                            modified = true;
                        }

                        node = nextNode;
                    }
                }
            }
            while (modified);
        }

        private static void PropagateCopy(Operation copyOp)
        {
            // Propagate copy source operand to all uses of the destination operand.
            Operand dest   = copyOp.Destination;
            Operand source = copyOp.GetSource(0);

            Node[] uses = dest.Uses.ToArray();

            foreach (Node use in uses)
            {
                for (int index = 0; index < use.SourcesCount; index++)
                {
                    if (use.GetSource(index) == dest)
                    {
                        use.SetSource(index, source);
                    }
                }
            }
        }

        private static void RemoveNode(BasicBlock block, Node node)
        {
            // Remove a node from the nodes list, and also remove itself
            // from all the use lists on the operands that this node uses.
            block.Operations.Remove(node);

            for (int index = 0; index < node.SourcesCount; index++)
            {
                node.SetSource(index, null);
            }

            Debug.Assert(node.Destination == null || node.Destination.Uses.Count == 0);

            node.Destination = null;
        }

        private static bool IsUnused(Node node)
        {
            return DestIsLocalVar(node) && node.Destination.Uses.Count == 0 && !HasSideEffects(node);
        }

        private static bool DestIsLocalVar(Node node)
        {
            return node.Destination != null && node.Destination.Kind == OperandKind.LocalVariable;
        }

        private static bool HasSideEffects(Node node)
        {
            return (node is Operation operation) && (operation.Instruction == Instruction.Call
                || operation.Instruction == Instruction.Tailcall
                || operation.Instruction == Instruction.CompareAndSwap);
        }

        private static bool IsPropagableCopy(Operation operation)
        {
            if (operation.Instruction != Instruction.Copy)
            {
                return false;
            }

            return operation.Destination.Type == operation.GetSource(0).Type;
        }
    }
}