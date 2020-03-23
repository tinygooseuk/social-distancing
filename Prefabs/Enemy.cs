using Godot;
using System;

public class Enemy : KinematicBody2D
{
    // Override point
    protected virtual bool IsAffectedByGravity() => true;
    protected virtual Vector2 Move(Vector2 playerPosition) => Vector2.Zero;

    // Consts
    private static float GRAVITY = 9.8f;
    private static float FRICTION = 0.15f;

    // State
    protected Vector2 Velocity = Vector2.Zero;

    public override void _Process(float delta)
    {
        Vector2 move = Move(Main.Instance.Player1.GlobalPosition);
        Velocity += move;

        if (IsAffectedByGravity())
        {
            Velocity.y += GRAVITY;
            Velocity.x *= (1.0f - FRICTION);
        }      

        Velocity = MoveAndSlide(Velocity, upDirection: Vector2.Up);
    }
}
