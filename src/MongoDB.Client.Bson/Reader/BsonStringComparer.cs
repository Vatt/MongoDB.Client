using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MongoDB.Client.Bson.Reader
{
    public static class BsonStringComparer
    {
        public static bool SequenceEqual1(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 1)
            {
                return false;
            }

            return value[0] == mask[0];
        }

        public static bool SequenceEqual2(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 2)
            {
                return false;
            }

            var first = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(value));
            var second = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(mask));
            return first == second;
        }

        public static bool SequenceEqual3(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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

        public static bool SequenceEqual4(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 4)
            {
                return false;
            }

            var first = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(value));
            var second = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(mask));
            return first == second;
        }

        public static bool SequenceEqual5(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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

        public static bool SequenceEqual8(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            if (value.Length != 8)
            {
                return false;
            }

            ref byte first = ref MemoryMarshal.GetReference(value);
            ref byte second = ref MemoryMarshal.GetReference(mask);

            ulong differentBits = Unsafe.ReadUnaligned<ulong>(ref first) - Unsafe.ReadUnaligned<ulong>(ref second);
            return differentBits == 0;
        }

        public static bool SequenceEqual9(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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

        public static bool SequenceEqual16(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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

        public static bool SequenceEqual17(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset)))
                        {
                            return false;
                        }
                        offset += sizeof(int);
                    } while (lengthToExamine > offset);
                }
                return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
            }
        }

        public static bool SequenceEqual32(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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

        public static bool SequenceEqual33(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset)))
                        {
                            return false;
                        }
                        offset += sizeof(int);
                    } while (lengthToExamine > offset);
                }
                return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
            }
        }

        public static bool SequenceEqual64(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
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
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref first, offset)) != Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref second, offset)))
                        {
                            return false;
                        }
                        offset += sizeof(int);
                    } while (lengthToExamine > offset);
                }
                return (Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref first, lengthToExamine)) == Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref second, lengthToExamine)));
            }
        }


        public static bool SequenceEqual65(this ReadOnlySpan<byte> value, ReadOnlySpan<byte> mask)
        {
            return value.SequenceEqual(mask);
        }
    }
}
