using Godot;
using System;

public class Enemy_Red : Enemy
{
    protected override bool IsAffectedByGravity() => false;
    protected override Color GetColour() => Colors.Red;

    protected override Vector2 Move(Vector2 playerPosition, float difficultyScale)
    {
        return (playerPosition - GlobalPosition + new Vector2(0.1f, 0.1f)).Normalized() * 4.0f * difficultyScale;
    }
}
