using Godot;
using System;

public class Pixel : RigidBody2D
{
    private float Age = 0.0f;
    private const float LIFETIME = 1.5f;

    private const float SUCK_COLLECT_DISTANCE = 20.0f;
    private const float SUCK_CLOSE_DISTANCE = 50.0f;

    private bool IsSucking = false;

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        if (IsSucking)
        {
            Character nearestPlayer = Game.Instance.GetNearestPlayer(GlobalPosition);
            if (IsInstanceValid(nearestPlayer))
            {
                float speed = state.LinearVelocity.Length();
                Vector2 towardsPlayer = (nearestPlayer.GlobalPosition - GlobalPosition);

                float distance = towardsPlayer.Length();
                if (distance < SUCK_COLLECT_DISTANCE)
                {
                    Asset<AudioStream> collectSound = R.Sounds.CollectPixel;
                    GetTree().PlaySound2D(collectSound, relativeTo: this);

                    QueueFree();
                } 
                else if (distance < SUCK_CLOSE_DISTANCE)
                {
                    float newAlpha = Mathf.InverseLerp(SUCK_COLLECT_DISTANCE, SUCK_CLOSE_DISTANCE, distance);
                    //Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, newAlpha);
                    Scale = new Vector2(newAlpha, newAlpha);
                }

                Vector2 targetVelocity = towardsPlayer.Normalized() * (speed + 10.0f);
                state.LinearVelocity = targetVelocity;
            }
        }
    }

    public override void _Process(float delta)
    {
        Age += delta;

        if (Age > LIFETIME)
        {
            if (!IsSucking)
            {
                CollisionMask = 0x00000000;
                GravityScale = 0.0f;
             
                IsSucking = true;             
            }            
        }
    }
}
