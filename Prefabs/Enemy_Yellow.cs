using Godot;
using System;

public class Enemy_Yellow : Enemy
{
    protected override Color GetColour() => Colors.Yellow;

    // Consts
    private const float TIME_TO_JUMP = 1.5f;

    // State
    private float GroundedTime = 0.0f;
    

    public override void _Process(float delta)
    {
        base._Process(delta);

        // Count up grounded time
        if (IsOnFloor())
        {
            GroundedTime += delta;
        }
        else
        {
            GroundedTime = 0.0f;
        }

        // Change scale based on time to jump
        float jumpProximity = GroundedTime / TIME_TO_JUMP;
        if (jumpProximity < 0.5f)
        {
            jumpProximity = 0.0f;
        }
        else 
        {
            jumpProximity = (jumpProximity - 0.5f) * 2.0f;
        }
        
        EnemySprite.Scale = new Vector2
        {   
            x = 2.0f * Mathf.Lerp(1.0f, 1.2f, jumpProximity),
            y = 2.0f * Mathf.Lerp(1.0f, 0.7f, jumpProximity)
        };
        EnemySprite.Position = new Vector2(0.0f, Mathf.Lerp(0.0f, +5.0f, jumpProximity));
    }

    protected override Vector2 Move(Vector2 playerPosition)
    {
        // Move towards player a little
        Vector2 move = new Vector2
        {
            x = Mathf.Sign(playerPosition.x - GlobalPosition.x) * 10.0f,
            y = 0.0f
        };

        if (IsOnFloor())
        {            
            if (GroundedTime > TIME_TO_JUMP)
            {
                // BIG JUMP
                move.x *= 100.0f;
                move.y = -400.0f;
                GroundedTime = 0.0f;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        return move;
    }
}
