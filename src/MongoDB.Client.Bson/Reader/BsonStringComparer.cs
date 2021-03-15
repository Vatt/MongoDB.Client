using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MongoDB.Client.Bson.Reader
{
    public static class BsonStringComparer
    {
        public static unsafe bool SequenceEqual1(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 1)
            {
                return false;
            }

            return value[0] == mask[0];
        }

        public static unsafe bool SequenceEqual2(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 2)
            {
                return false;
            }

            var first = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(value));
            var second = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(mask));
            return first == second;
        }

        public static unsafe bool SequenceEqual3(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 3)
            {
                return false;
            }
            ref var first = ref MemoryMarshal.GetReference(value);
            ref var second = ref MemoryMarshal.GetReference(mask);
            var diff = Unsafe.ReadUnaligned<ushort>(ref first) - Unsafe.ReadUnaligned<ushort>(ref second);
            diff |= Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, 1)) - Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref second, 1));
            return diff == 0;
        }

        public static unsafe bool SequenceEqual4(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 4)
            {
                return false;
            }

            var first = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(value));
            var second = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(mask));
            return first == second;
        }

        public static unsafe bool SequenceEqual5(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != mask.Length)
            {
                return false;
            }
            var offset = value.Length - 4;
            ref var first = ref MemoryMarshal.GetReference(value);
            ref var second = ref MemoryMarshal.GetReference(mask);
            var diff = Unsafe.ReadUnaligned<ushort>(ref first) - Unsafe.ReadUnaligned<ushort>(ref second);
            diff |= Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref second, offset));
            return diff == 0;
        }

        //public static unsafe bool SequenceEqual6(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 6)
        //    {
        //        return false;
        //    }
        //    ref var first = ref MemoryMarshal.GetReference(value);
        //    ref var second = ref MemoryMarshal.GetReference(mask);
        //    var firstInt = Unsafe.ReadUnaligned<uint>(ref first);
        //    var secondInt = Unsafe.ReadUnaligned<uint>(ref second);
        //    var firstShort = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, 4));
        //    var secondShort = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, 4));
        //    return firstInt == secondInt && firstShort == secondShort;
        //}

        //public static unsafe bool SequenceEqual7(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 7)
        //    {
        //        return false;
        //    }
        //    ref var first = ref MemoryMarshal.GetReference(value);
        //    ref var second = ref MemoryMarshal.GetReference(mask);
        //    var firstInt = Unsafe.ReadUnaligned<uint>(ref first);
        //    var secondInt = Unsafe.ReadUnaligned<uint>(ref second);
        //    var firstShort = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, 4));
        //    var secondShort = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, 4));
        //    return firstInt == secondInt && firstShort == secondShort && value[6] == mask[6];
        //}

        public static unsafe bool SequenceEqual8(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 8)
            {
                return false;
            }

            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            ulong differentBits = Unsafe.ReadUnaligned<ulong>(ref first) - Unsafe.ReadUnaligned<ulong>(ref second);
          //  differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
            return differentBits == 0;
        }

        public static unsafe bool SequenceEqual9(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != mask.Length)
            {
                return false;
            }

            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);
            int offset = value.Length - 8;

            ulong differentBits = Unsafe.ReadUnaligned<ulong>(ref first) - Unsafe.ReadUnaligned<ulong>(ref second);
            differentBits |= Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref second, offset));
            return differentBits == 0;
        }

        //public static unsafe bool SequenceEqual10(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    const int length = 10;
        //    if (value.Length != length)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    const int offset = 4;
        //    const int offset2 = 8;

        //    uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
        //    differentBits |= (uint)(Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref second, offset2)));

        //    return differentBits == 0;
        //}

        //public static unsafe bool SequenceEqual11(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 11)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    const int offset = 4;
        //    const int offset2 = 8;

        //    uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
        //    differentBits |= (uint)(Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref second, offset2)));

        //    return differentBits == 0 && value[10] == mask[10];
        //}

        //public static unsafe bool SequenceEqual12(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 12)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    const int offset = 4;
        //    const int offset2 = 8;

        //    uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset2));

        //    return differentBits == 0;
        //}

        //public static unsafe bool SequenceEqual13(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 13)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    const int offset = 4;
        //    const int offset2 = 8;

        //    uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset2));

        //    return differentBits == 0 && value[12] == mask[12];
        //}

        //public static unsafe bool SequenceEqual14(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 14)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    const int offset = 4;
        //    const int offset2 = 8;
        //    const int offset3 = 12;

        //    uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset2));
        //    differentBits |= (uint)(Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, offset3)) - Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref second, offset3)));

        //    return differentBits == 0;
        //}

        //public static unsafe bool SequenceEqual15(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    if (value.Length != 15)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    const int offset = 4;
        //    const int offset2 = 8;
        //    const int offset3 = 12;

        //    uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
        //    differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset2));
        //    differentBits |= (uint)(Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref first, offset3)) - Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref second, offset3)));

        //    return differentBits == 0 && value[14] == mask[14];
        //}

        public static unsafe bool SequenceEqual16(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            var length = value.Length;
            if (length != 16)
            {
                return false;
            }
            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            if (Sse2.IsSupported)
            {
                var vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref first), Unsafe.ReadUnaligned<Vector128<byte>>(ref second));

                return Sse2.MoveMask(vecResult) == 0xFFFF;
            }
            else
            {
                const int offset = 4;
                const int offset2 = 8;
                const int offset3 = 12;

                uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset2));
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset3)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset3));
                return differentBits == 0;
            }
        }

        public static unsafe bool SequenceEqual17(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            var length = value.Length;
            if (length != mask.Length)
            {
                return false;
            }


            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            if (Sse2.IsSupported)
            {
                int lengthToExamine = length - 16;
                var vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref first), Unsafe.ReadUnaligned<Vector128<byte>>(ref second));
                if (Sse2.MoveMask(vecResult) != 0xFFFF)
                {
                    return false;
                }

                vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref first, lengthToExamine)), Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref second, lengthToExamine)));
                return Sse2.MoveMask(vecResult) == 0xFFFF;
            }
            else
            {
                int offset = 0;
                int lengthToExamine = length - sizeof(int);
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        // Compare unsigned so not do a sign extend mov on 64 bit
                        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset)))
                        {
                            return false;
                        }
                        offset += sizeof(int);
                    } while (lengthToExamine > offset);
                }
                // Do final compare as sizeof(nuint) from end rather than start
                return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
            }
        }

        public static unsafe bool SequenceEqual32(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            var length = value.Length;
            if (length != 32)
            {
                return false;
            }
            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            if (Sse2.IsSupported)
            {
                if (Avx2.IsSupported)
                {
                    var vecResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref first), Unsafe.ReadUnaligned<Vector256<byte>>(ref second));
                    return Avx2.MoveMask(vecResult) == -1;
                }
                else
                {
                    var vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref first), Unsafe.ReadUnaligned<Vector128<byte>>(ref second));
                    if (Sse2.MoveMask(vecResult) != 0xFFFF)
                    {
                        return false;
                    }
                    vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref first, 16)), Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref second, 16)));
                    return Sse2.MoveMask(vecResult) == 0xFFFF;
                }

            }
            else
            {
                const int offset = 4;
                const int offset2 = 8;
                const int offset3 = 12;

                uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset));
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset2)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset2));
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset3)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset3));
                return differentBits == 0;
            }
        }

        public static unsafe bool SequenceEqual33(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            var length = value.Length;
            if (length != mask.Length)
            {
                return false;
            }


            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            if (Sse2.IsSupported)
            {
                if (Avx2.IsSupported)
                {
                    var avxResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref first), Unsafe.ReadUnaligned<Vector256<byte>>(ref second));
                    if (Avx2.MoveMask(avxResult) != -1)
                    {
                        return false;
                    }
                    var avxLengthToExamine = length - 32;
                    avxResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, avxLengthToExamine)), Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, avxLengthToExamine)));
                    if (Avx2.MoveMask(avxResult) == -1)
                    {
                        // C# compiler inverts this test, making the outer goto the conditional jmp.
                        return true;
                    }
                }

                int lengthToExamine = length - 16;
                var vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref first), Unsafe.ReadUnaligned<Vector128<byte>>(ref second));
                if (Sse2.MoveMask(vecResult) != 0xFFFF)
                {
                    return false;
                }

                vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref first, lengthToExamine)), Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref second, lengthToExamine)));
                return Sse2.MoveMask(vecResult) == 0xFFFF;
            }
            else
            {
                int offset = 0;
                int lengthToExamine = length - sizeof(int);
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        // Compare unsigned so not do a sign extend mov on 64 bit
                        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset)))
                        {
                            return false;
                        }
                        offset += sizeof(int);
                    } while (lengthToExamine > offset);
                }
                // Do final compare as sizeof(nuint) from end rather than start
                return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
            }
        }

        public static unsafe bool SequenceEqual64(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            var length = value.Length;
            if (length != mask.Length)
            {
                return false;
            }


            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            if (Sse2.IsSupported)
            {
                if (Avx2.IsSupported)
                {
                    var avxResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref first), Unsafe.ReadUnaligned<Vector256<byte>>(ref second));
                    if (Avx2.MoveMask(avxResult) != -1)
                    {
                        return false;
                    }
                    const int avxLengthToExamine = 32;
                    avxResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, avxLengthToExamine)), Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, avxLengthToExamine)));
                    if (Avx2.MoveMask(avxResult) == -1)
                    {
                        // C# compiler inverts this test, making the outer goto the conditional jmp.
                        return true;
                    }
                }

                int lengthToExamine = length - 16;
                var vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref first), Unsafe.ReadUnaligned<Vector128<byte>>(ref second));
                if (Sse2.MoveMask(vecResult) != 0xFFFF)
                {
                    return false;
                }

                vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref first, lengthToExamine)), Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref second, lengthToExamine)));
                return Sse2.MoveMask(vecResult) == 0xFFFF;
            }
            else
            {
                int offset = 0;
                int lengthToExamine = length - sizeof(int);
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        // Compare unsigned so not do a sign extend mov on 64 bit
                        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset)))
                        {
                            return false;
                        }
                        offset += sizeof(int);
                    } while (lengthToExamine > offset);
                }
                // Do final compare as sizeof(nuint) from end rather than start
                return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
            }
        }
        public static unsafe bool SequenceEqual65(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            return value.SequenceEqual(mask);
        }
        //public static unsafe bool SequenceEqual2(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    var length = value.Length;
        //    if (value.Length != mask.Length)
        //    {
        //        return false;
        //    }

        //    ref byte first = ref MemoryMarshal.GetReference(value);
        //    ref byte second = ref MemoryMarshal.GetReference(mask);
        //    if (Sse2.IsSupported)
        //    {
        //        if (Avx2.IsSupported && length >= Vector256<byte>.Count)
        //        {
        //            Vector256<byte> vecResult;
        //            int offset = 0;
        //            int lengthToExamine = length - Vector256<byte>.Count;

        //            // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
        //            Debug.Assert(lengthToExamine < length);
        //            if (lengthToExamine != 0)
        //            {
        //                do
        //                {

        //                    vecResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, offset)), Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref second, offset)));
        //                    if (Avx2.MoveMask(vecResult) != -1)
        //                    {
        //                        return false;
        //                    }
        //                    offset += Vector256<byte>.Count;
        //                } while (lengthToExamine > offset);
        //            }

        //            // Do final compare as Vector256<byte>.Count from end rather than start
        //            vecResult = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, lengthToExamine)), Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.Add(ref first, lengthToExamine)));
        //            if (Avx2.MoveMask(vecResult) == -1)
        //            {
        //                // C# compiler inverts this test, making the outer goto the conditional jmp.
        //                return true;
        //            }

        //            // This becomes a conditional jmp foward to not favor it.
        //            return false;
        //        }
        //        // Use Vector128.Size as Vector128<byte>.Count doesn't inline at R2R time
        //        // https://github.com/dotnet/runtime/issues/32714
        //        else if (length >= Vector128<byte>.Count)
        //        {
        //            var vector128Size = Vector128<byte>.Count;
        //            Vector128<byte> vecResult;
        //            int offset = 0;
        //            int lengthToExamine = length - vector128Size;
        //            // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
        //            Debug.Assert(lengthToExamine < length);
        //            if (lengthToExamine != 0)
        //            {
        //                do
        //                {
        //                    // We use instrincs directly as .Equals calls .AsByte() which doesn't inline at R2R time
        //                    // https://github.com/dotnet/runtime/issues/32714
        //                    vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref first, offset)), Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref second, offset)));
        //                    if (Sse2.MoveMask(vecResult) != 0xFFFF)
        //                    {
        //                        return false;
        //                    }
        //                    offset += vector128Size;
        //                } while (lengthToExamine > offset);
        //            }

        //            // Do final compare as Vector128<byte>.Count from end rather than start
        //            vecResult = Sse2.CompareEqual(Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref first, lengthToExamine)), Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref second, lengthToExamine)));
        //            if (Sse2.MoveMask(vecResult) == 0xFFFF)
        //            {
        //                // C# compiler inverts this test, making the outer goto the conditional jmp.
        //                return true;
        //            }

        //            // This becomes a conditional jmp foward to not favor it.
        //            return false;
        //        }
        //    }

        //    if (Sse2.IsSupported)
        //    {

        //        Debug.Assert(length <= sizeof(int) * 2);

        //        int offset = length - sizeof(int);
        //        int differentBits = Unsafe.ReadUnaligned<int>(ref first) - Unsafe.ReadUnaligned<int>(ref second);

        //        differentBits |= Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, offset)) - Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, offset));
        //        return differentBits == 0;
        //    }
        //    else
        //    {
        //        Debug.Assert(length >= sizeof(int));
        //        {
        //            int offset = 0;
        //            int lengthToExamine = length - sizeof(int);
        //            // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
        //            Debug.Assert(lengthToExamine < length);
        //            if (lengthToExamine > 0)
        //            {
        //                do
        //                {
        //                    // Compare unsigned so not do a sign extend mov on 64 bit
        //                    if (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, offset)))
        //                    {
        //                        return false;
        //                    }
        //                    offset += sizeof(int);
        //                } while (lengthToExamine > offset);
        //            }
        //            // Do final compare as sizeof(nuint) from end rather than start
        //            return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
        //        }
        //    }
        //}


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static bool CompareString256(ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    // Debug.Assert(value.Length > 16 && value.Length <= 32);
        //    if (Avx2.IsSupported)
        //    {
        //        ref byte valueStart = ref MemoryMarshal.GetReference(value);
        //        ref byte maskStart = ref MemoryMarshal.GetReference(mask);
        //        var valueVector = Unsafe.ReadUnaligned<Vector256<byte>>(ref valueStart);
        //        var maskVector = Unsafe.ReadUnaligned<Vector256<byte>>(ref maskStart);
        //        return Avx2.MoveMask(Avx2.CompareEqual(valueVector, maskVector)) == -1;
        //    }
        //    else
        //    {
        //        return value.SequenceEqual(mask);
        //    }
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static bool CompareString128(ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        //{
        //    Debug.Assert(value.Length > 8 && value.Length <= 16);
        //    if (Sse2.IsSupported)
        //    {
        //        ref byte valueStart = ref MemoryMarshal.GetReference(value);
        //        ref byte maskStart = ref MemoryMarshal.GetReference(mask);
        //        var valueVector = Unsafe.ReadUnaligned<Vector128<byte>>(ref valueStart);
        //        var maskVector = Unsafe.ReadUnaligned<Vector128<byte>>(ref maskStart);
        //        return Sse2.MoveMask(Sse2.CompareEqual(valueVector, maskVector)) == -1;
        //    }
        //    else
        //    {
        //        return value.SequenceEqual(mask);
        //    }
        //}
    }
}
