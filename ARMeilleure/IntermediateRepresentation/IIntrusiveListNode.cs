using ARMeilleure.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ARMeilleure.IntermediateRepresentation
{
    interface IIntrusiveListNode<T> where T : class, IIntrusiveListNode<T>
    {
        [MaybeNull]
        T ListPrevious { get; set; }
        [MaybeNull]
        T ListNext { get; set; }
    }

    // These have to be in extension methods, as for some reason they aren't seen as default interface methods.
    static class IntrusiveListNodeExt
    {
        // This is not called GetEnumerable as you could put IEnumerables into an IntrusiveList.
        // We need to explicitly state that we want to traverse a linked list.
        [MethodImpl(MethodOptions.FastInline)]
        public static IEnumerable<T> Traverse<T>(this T self, bool skipSelf = false) where T : class, IIntrusiveListNode<T>
        {
            var node = skipSelf ? self.ListNext : self;
            while (node != null)
            {
                yield return node;

                node = node.ListNext;
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        public static IEnumerable<T> Traverse<T>(this T self) where T : class, IIntrusiveListNode<T>
        {
            var node = self;
            do
            {
                yield return node;

                node = node.ListNext;
            } while (node != null);
        }

        [MethodImpl(MethodOptions.FastInline)]
        public static IEnumerable<T> TraverseReverse<T>(this T self, bool skipSelf = false) where T : class, IIntrusiveListNode<T>
        {
            var node = skipSelf ? self.ListPrevious : self;
            while (node != null)
            {
                yield return node;

                node = node.ListPrevious;
            }
        }

        [MethodImpl(MethodOptions.FastInline)]
        public static IEnumerable<T> TraverseReverse<T>(this T self) where T : class, IIntrusiveListNode<T>
        {
            var node = self;
            do
            {
                yield return node;

                node = node.ListPrevious;
            } while (node != null);
        }
    }
}
