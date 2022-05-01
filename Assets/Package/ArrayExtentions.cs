using System;
using System.Collections.Generic;
using System.Collections;

public static class ArrayExtentions
{
    public static byte[] Merge(byte[] BigArr, byte[] SmallArr, int index = 0)
    {
        foreach (byte item in SmallArr)
        {
            BigArr[index] = item;
            index++;
        }

        return BigArr;
    }

    public static byte[] Slice(byte[] Arr, int start, int end)
    {
        byte[] to_return = new byte[end - start];
        var index = 0;
        for (int i = start; i < end; i++)
        {
            to_return[index] = Arr[i];
            index++;
        }
        return to_return;
    }

    public static Tuple<byte[], int> ClearEmpty(byte[] Arr)
    {
        int index = 0;
        for (int i = 0; i < Arr.Length; i++)
        {
            if (Arr[i] != 0)
            {
                index = i;
                break;
            }
        }
        return new Tuple<byte[], int>(
            Merge(new byte[Arr.Length], Slice(Arr, index, Arr.Length)),
            index
        );
    }
}
