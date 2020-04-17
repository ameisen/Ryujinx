﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Utilities
{
    [DebuggerDisplay("{ToString()}")]
    [StructLayout(LayoutKind.Sequential, Size = 16, Pack = 1)]
    public struct Buffer16
    {
        private unsafe fixed byte Bytes[16];

        public unsafe byte this[int i]
        {
            readonly get => Bytes[i];
            set => Bytes[i] = value;
        }

        // Prevent a defensive copy by changing the read-only in reference to a reference with Unsafe.AsRef()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(in Buffer16 value)
        {
            return SpanHelpers.AsByteSpan(ref Unsafe.AsRef(in value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(in Buffer16 value)
        {
            return SpanHelpers.AsReadOnlyByteSpan(ref Unsafe.AsRef(in value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T As<T>() where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() > (uint)Unsafe.SizeOf<Buffer16>())
            {
                throw new ArgumentException();
            }

            return ref MemoryMarshal.GetReference(AsSpan<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan<T>() where T : unmanaged
        {
            return SpanHelpers.AsSpan<Buffer16, T>(ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged
        {
            return SpanHelpers.AsReadOnlySpan<Buffer16, T>(ref Unsafe.AsRef(in this));
        }
    }
}
