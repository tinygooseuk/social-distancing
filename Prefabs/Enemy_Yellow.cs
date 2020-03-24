using Godot;
using System;

public class Enemy_Yellow : Enemy
{
    protected override Color GetColour() => Colors.Yellow;

    // State
    private float GroundedTime = 0.0f;
    

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (IsOnFloor())
        {
            GroundedTime += delta;
        }
        else
        {
            GroundedTime = 0.0f;
        }
    }

    protected override Vector2 Move(Vector2 playerPosition)
    {
        if (GroundedTime < 1.5f)
        {
            return Vector2.Zero;
        }

        Vector2 move = (playerPosition - Position + new Vector2(0.1f, 0.1f)).Normalized() * 800.0f;
        move.y = -500.0f;

        GroundedTime = 0.0f;

        return move;
    }
}
