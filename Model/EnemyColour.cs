using System;
using Godot;

public enum EnemyColour
{
    Blue, 
    Yellow,
    Red,
    Green
}

public static class EnemyColourUtils
{
    public static Color ToColor(this EnemyColour colour)
    {
        switch (colour)
        {
            case EnemyColour.Red: return Colors.Red;
            case EnemyColour.Yellow: return Colors.Yellow;
            case EnemyColour.Blue: return Colors.Blue;
            case EnemyColour.Green: return Colors.Green;
            default:
                return Colors.Red;
        }
    }
}