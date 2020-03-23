using Godot;
using System;

public class Enemy_Red : Enemy
{
    protected override bool IsAffectedByGravity() => false;

    protected override Vector2 Move(Vector2 playerPosition)
    {
        return (playerPosition - GlobalPosition + new Vector2(0.1f, 0.1f)).Normalized() * 2.0f;
    }
}
