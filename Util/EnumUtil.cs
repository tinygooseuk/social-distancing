using System;
using System.Reflection;

public static class EnumUtil
{
    public static int GetCount<T>() 
    {
        return Enum.GetValues(typeof(T)).Length;
    }
}
