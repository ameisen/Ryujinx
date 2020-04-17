using ARMeilleure.CodeGen;
using ARMeilleure.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    static class JitCache
    {
        private const int PageSize = 4 * 1024;
        private const int PageMask = PageSize - 1;

        private const int CodeAlignment = 4; // Bytes

        private const int CacheSize = 2047 * 1024 * 1024;

        private static readonly ReservedRegion _jitRegion;

        private static IntPtr _basePointer => _jitRegion.Pointer;

        private static readonly JitCacheMemoryAllocator _allocator;

        private static readonly Dictionary<int, JitCacheEntry> _cacheEntries;

        private static int _protectedOffset;

        private static readonly object _lock;

        private static IntPtr GetPointer(int offset) => _basePointer + offset;
        private static int GetOffset(IntPtr address) => checked((int)((ulong)address - (ulong)_basePointer));

        static JitCache()
        {
            _jitRegion = new ReservedRegion(CacheSize);

            int startOffset = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _jitRegion.ExpandIfNeeded(PageSize);
                JitUnwindWindows.InstallFunctionTableHandler(_basePointer, CacheSize);

                // The first page is used for the table based SEH structs.
                startOffset = PageSize;
            }

            _allocator = new JitCacheMemoryAllocator(CacheSize - startOffset, startOffset);

            _cacheEntries = new Dictionary<int, JitCacheEntry>();

            _protectedOffset = 0;

            _lock = new object();
        }

        public static IntPtr Map(CompiledFunction func)
        {
            byte[] code = func.Code;

            lock (_lock)
            {
                int funcOffset = Allocate(code.Length);

                IntPtr funcPtr = _basePointer + funcOffset;

                Marshal.Copy(code, 0, funcPtr, code.Length);

                ReprotectTo(funcOffset + code.Length);

                Add(new JitCacheEntry(funcOffset, code.Length, func.UnwindInfo));

                MemoryManagement.FlushInstructionCache(funcPtr, (ulong)code.Length);

                return funcPtr;
            }
        }

        private static void ReprotectTo(int offset)
        {
            // Map pages that are already full as RX.
            // Map pages that are not full yet as RWX.
            // On unix, the address must be page aligned.
            if (offset <= _protectedOffset)
            {
                return;
            }

            int pageStart = _protectedOffset & ~PageMask;
            int pageEnd   = (offset + PageMask) & ~PageMask;

            int fullPagesSize = pageEnd - pageStart;

            IntPtr funcPtr = GetPointer(pageStart);

            MemoryManagement.Reprotect(funcPtr, (ulong)fullPagesSize, MemoryProtection.ReadWriteExecute);

            _protectedOffset = pageEnd;
        }

        private static int Allocate(int codeSize)
        {
            codeSize = checked(codeSize + (CodeAlignment - 1)) & ~(CodeAlignment - 1);

            int allocOffset = _allocator.Allocate(codeSize);

            ulong endOffset = (ulong)allocOffset + (ulong)codeSize;

            _jitRegion.ExpandIfNeeded(endOffset);

            return allocOffset;
        }

        public static void Free(IntPtr address)
        {
            int offset = GetOffset(address);
            lock (_lock)
            {
                if (_cacheEntries.Remove(offset, out var entry))
                {
                    _allocator.Free(entry.Offset);
                }
                else
                {
                    throw new InvalidOperationException($"Address {address} does not exist within the Cache Entries");
                }
            }
        }

        private static void Add(JitCacheEntry entry)
        {
            _cacheEntries.Add(entry.Offset, entry);
        }

        public static bool TryFind(int offset, out JitCacheEntry entry)
        {
            lock (_lock)
            {
                if (_cacheEntries.TryGetValue(offset, out entry))
                {
                    return true;
                }
            }

            entry = default;

            return false;
        }
    }
}