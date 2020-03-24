using Godot;
using System;

public class Enemy_Blue : Enemy
{
    protected override Color GetColour() => Colors.Blue;

    public override void _Process(float delta)
    {
        base._Process(delta);

        RotationDegrees += Velocity.x / 10.0f;
    }

    protected override Vector2 Move(Vector2 playerPosition)
    {
        Vector2 move = (playerPosition - Position + new Vector2(0.1f, 0.1f)).Normalized() * 20.0f;
        move.y = 0.0f;

        return move;
    }
}
