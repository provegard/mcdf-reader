using System;
using System.Collections.Generic;

namespace McdfReader
{
    internal static class SectorUtil
    {
        internal static uint[] ReadSectorNumbers(ref ReadOnlySpan<byte> span)
        {
            var count = span.Length / 4;
            var arr = new uint[count];
            for (var i = 0; i < arr.Length; i++)
            {
                var offset = 4 * i;
                var sectorNumber = BitConverter.ToUInt32(span[offset..(offset + 4)]);
                arr[i] = sectorNumber;
            }
            return arr;
        }
    }
}
