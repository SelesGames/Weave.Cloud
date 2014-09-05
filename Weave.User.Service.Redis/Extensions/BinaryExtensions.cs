using System.Collections.Generic;

namespace System.IO
{
    public static class BinaryExtensions
    {
        public static Guid ReadGuid(this BinaryReader br)
        {
            return new Guid(br.ReadBytes(16));
        }

        public static DateTime ReadDateTime(this BinaryReader br)
        {
            return DateTime.FromBinary(br.ReadInt64());
        }

        public static IEnumerable<bool> GetBits(this byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return ReadBit(b, i);
            }
        }

        static bool ReadBit(byte b, int bitIndex)
        {
            return (b & (1 << bitIndex)) != 0;
        }

        public static void Write(this BinaryWriter bw, Guid guid)
        {
            bw.Write(guid.ToByteArray());
        }

        public static void Write(this BinaryWriter bw, DateTime dateTime)
        {
            bw.Write(dateTime.ToBinary());
        }

        public static byte ToByte(this IEnumerable<bool> bools)
        {
            return (byte)ToInt32(bools);
        }

        public static int ToInt32(this IEnumerable<bool> bools)
        {
            int x = 0;
            int index = 0;

            foreach (var val in bools)
            {
                int mask = 1 << index;

                if (val)
                    x |= mask;

                index++;
            }

            return x;
        }
    }
}