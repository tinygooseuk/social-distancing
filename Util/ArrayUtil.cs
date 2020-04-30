using System;
using System.Collections.Generic;
using Godot;

public static class ArrayUtil
{
    public static T[] Shuffle<T>(this T[] inArray)
    {
        int n = inArray.Length;
        for (int i = 0; i < (n - 1); i++)
        {
            // Use Next on random instance with an argument.
            // ... The argument is an exclusive bound.
            //     So we will not go past the end of the array.
            int r = i + (int)(GD.Randi() % (n - i));
            T t = inArray[r];
            inArray[r] = inArray[i];
            inArray[i] = t;
        }

        return inArray;
    }

    public static T[] GenerateArray<T>(int num)
    {
        return Generate<T>(num, i => default(T)).ToArray();
    }
    
    public static List<T> Generate<T>(int num)
    {
        return Generate<T>(num, i => default(T));
    }

    public static T[] GenerateArray<T>(int num, Func<int, T> generator)
    {
        return Generate<T>(num, generator).ToArray();
    }

    public static List<T> Generate<T>(int num, Func<int, T> generator)
    {
        var list = new List<T>(num);

        for (int i = 0; i < num; i++)
        {
            list.Add(generator.Invoke(i));
        }
        
        return list;
    }
    
    
}