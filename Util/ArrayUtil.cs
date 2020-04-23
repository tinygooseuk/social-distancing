using System;
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
}