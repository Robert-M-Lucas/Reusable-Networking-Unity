using System;
using System.Collections.Generic;
using System.Collections;

public static class ArrayExtentions<T>{
    public static void Merge(T[] BigArr, T[] SmallArr, int index){
        foreach (T item in SmallArr){
            BigArr[index] = item;
            index ++;
        }
    }

    public static T[] Slice(T[] Arr, int start, int end){
        T[] to_return = new T[end-start];
        var index = 0;
        for (int i = start; i < end; i ++){
            to_return[index] = Arr[i];
            index ++;
        }
        return to_return;
    }
}