using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ARMeilleure.Common
{
    sealed class ThreadStaticPool<T> where T : class, new()
    {
        private const int InitialPoolSize = 256;
        private const int MaxPoolId = 2; // Presently, HighCQ and LowCQ are the only pool indices

        [MethodImpl(MethodOptions.FastInline)]
        private static int ResizeUp(int currentSize)
        {
            return (int)((ulong)currentSize * 3UL / 2UL + 1UL);
        }

        [ThreadStatic]
        private static ThreadStaticPool<T> _instance;
        public static ThreadStaticPool<T> Instance => _instance ?? PreparePool(0);

        private static List<ConcurrentStack<ThreadStaticPool<T>>> _pools = new List<ConcurrentStack<ThreadStaticPool<T>>>(capacity: MaxPoolId);

        private static ConcurrentStack<ThreadStaticPool<T>> GetPools(int groupId)
        {
            lock (_pools)
            {
                _pools.EnlargeTo(groupId + 1);
                var pool = _pools[groupId] ??= new ConcurrentStack<ThreadStaticPool<T>>();
                return pool;
            }
        }

        [return: NotNull]
        public static ThreadStaticPool<T> PreparePool(int groupId)
        {
            // Prepare the pool for this thread, ideally using an existing one from the specified group.
            if (_instance == null)
            {
                var pools = GetPools(groupId);
                if (!pools.TryPop(out var newPool))
                {
                    newPool = new ThreadStaticPool<T>(InitialPoolSize);
                }
                return _instance = newPool;
            }
            return _instance;
        }

        public static void ReturnPool(int groupId)
        {
            // Reset and return the pool for this thread to the specified group.
            var pools = GetPools(groupId);
            _instance.Clear();
            pools.Push(_instance);
            _instance = null;
        }

        private volatile T[] _pool;
        private int _poolUsed = -1;
        private volatile int _poolSize;

        public ThreadStaticPool(int initialSize)
        {
            var pool = _pool = new T[initialSize];

            for (int i = 0; i < initialSize; i++)
            {
                pool[i] = new T();
            }

            _poolSize = initialSize;
        }

        [MethodImpl(MethodOptions.FastInline)]
        private bool NeedResize(int index) => index >= _poolSize;

        [MethodImpl(MethodOptions.FastInline)]
        public T Allocate()
        {
            int index = Interlocked.Increment(ref _poolUsed);
            CheckIncreaseSize(index);
            return _pool[index];
        }

        [MethodImpl(MethodOptions.FastInline)]
        private void CheckIncreaseSize(int index)
        {
            if (!NeedResize(index))
            {
                return;
            }

            lock (_pool)
            {
                if (!NeedResize(index))
                {
                    return;
                }

                var newPoolSize = ResizeUp(_poolSize);

                var pool = _pool;
                Array.Resize(ref pool, newPoolSize);

                for (int i = _poolSize; i < newPoolSize; i++)
                {
                    pool[i] = new T();
                }

                _poolSize = newPoolSize;
                _pool = pool;
            }
        }

        public void Clear() => _poolUsed = -1;
    }
}
