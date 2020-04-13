using Godot;
using System;

public class Enemy_Yellow : Enemy
{
    protected override EnemyColour GetColour() => EnemyColour.Yellow;

    // Subnodes
    [Subnode] private AudioStreamPlayer2D JumpSound;

    // Consts
    private const float TIME_TO_JUMP = 1.5f;

    // State
    private float GroundedTime = 0f;
    
    public override void _Ready()
    {
        base._Ready();
        this.FindSubnodes();

        // Don't all jump at once!
        GroundedTime = (float)GD.RandRange(0f, 0.5f);
    }

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
            GroundedTime = 0f;
        }

        // Change scale based on time to jump
        float jumpProximity = GroundedTime / TIME_TO_JUMP;
        if (jumpProximity < 0.5f)
        {
            jumpProximity = 0f;
        }
        else 
        {
            jumpProximity = (jumpProximity - 0.5f) * 2f;
        }
        
        EnemySprite.Scale = new Vector2
        {   
            x = 2f * Mathf.Lerp(1f, 1.2f, jumpProximity),
            y = 2f * Mathf.Lerp(1f, 0.7f, jumpProximity)
        };
        EnemySprite.Position = new Vector2(0f, Mathf.Lerp(0f, +5f, jumpProximity));
    }

    protected override Vector2 Move(Vector2 playerPosition, float difficultyScale)
    {
        // Move towards player a little
        Vector2 move = new Vector2
        {
            x = Mathf.Sign(playerPosition.x - GlobalPosition.x) * 10f * difficultyScale,
            y = 0f
        };

        if (IsOnFloor())
        {            
            if (GroundedTime > TIME_TO_JUMP)
            {
                // BIG JUMP
                move.x *= 100f;
                move.y = -400f;
                GroundedTime = 0f;

                JumpSound.PitchScale = (float)GD.RandRange(0.8f, 1.2f);
                JumpSound.Play();
            }
            else
            {
                return Vector2.Zero;
            }
        }

        return move;
    }
}
