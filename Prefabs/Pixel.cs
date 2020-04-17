using Godot;
using System;

public class Pixel : RigidBody2D
{
    // Consts
    private const float LIFETIME = 1.5f;
    private const float LIFETIME_RANDOM = 0.25f;

    private const float SUCK_COLLECT_DISTANCE = 20f;
    private const float SUCK_CLOSE_DISTANCE = 50f;

    // Public state
    public float LifetimeMultiplier = 1f;
    public Vector2? CustomSuckTarget;
    public bool CanSuck = true;

    // State
    private float Age = 0f;
    private float Lifetime = 0f;
    
    private bool IsSucking = false;
    private bool IsSettled = false;

    // Subnodes
    public Sprite PixelSprite => GetNode<Sprite>("Sprite");
    public CollisionShape2D CollisionShape => GetNode<CollisionShape2D>("CollisionShape2D");

    public override void _Ready()
    {
        Lifetime = LifetimeMultiplier * LIFETIME + (float)GD.RandRange(-LIFETIME_RANDOM, +LIFETIME_RANDOM);

        if (!CanSuck)
        {
            Lifetime *= 2f;
        }
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        if (IsSettled) 
        {
            state.LinearVelocity = Vector2.Zero;
            state.AngularVelocity *= 0.99f;

            GlobalPosition = CustomSuckTarget.GetValueOrDefault();
            return;
        }

        if (IsSucking && CanSuck)
        {
            Vector2 target = Vector2.Zero;
            bool mustSettle = false;

            if (CustomSuckTarget.HasValue)
            {
                target = CustomSuckTarget.GetValueOrDefault();
                mustSettle = true;
            } 
            else 
            {
                Character nearestPlayer = Game.Instance.GetNearestPlayer(GlobalPosition);
                if (IsInstanceValid(nearestPlayer))
                {
                    target = nearestPlayer.GlobalPosition;
                }
            }

            float speed = state.LinearVelocity.Length();
            Vector2 towardsTarget = (target - GlobalPosition);

            float distance = towardsTarget.Length();
            if (distance < SUCK_COLLECT_DISTANCE)
            {
                if (mustSettle)
                {
                    GlobalPosition = target;
                    IsSettled = true;
                } 
                else 
                {
                    Asset<AudioStream> collectSound = R.Sounds.CollectPixel;
                    GetTree().PlaySound2D(collectSound, relativeTo: this);

                    if (!CustomSuckTarget.HasValue && Game.Instance.InputMethodManager.IsVibrationEnabled)
                    {
                        Character nearestPlayer = Game.Instance.GetNearestPlayer(GlobalPosition);
                        
                        // Vibrate
                        Input.StartJoyVibration(nearestPlayer.PlayerIndex, 0.8f, 0.2f, 0.2f);
                    }
                    
                    QueueFree();
                }
            } 
            else if (distance < SUCK_CLOSE_DISTANCE)
            {
                float newAlpha = Mathf.InverseLerp(SUCK_COLLECT_DISTANCE, SUCK_CLOSE_DISTANCE, distance);
                //Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, newAlpha);
                Scale = new Vector2(newAlpha, newAlpha);
            }

            Vector2 targetVelocity = towardsTarget.Normalized() * (speed + 10f);
            state.LinearVelocity = targetVelocity.Clamped(500f);
        }
    }

    public override void _Process(float delta)
    {
        Age += delta;
        
        // no suck? fade out!
        if (!CanSuck)
        {
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f - (Age / Lifetime));
        }

        if (Age > Lifetime)
        {
            if (CanSuck)
            {
                if (!IsSucking)
                {
                    CollisionMask = 0x00000000;
                    GravityScale = 0f;
                    
                    IsSucking = true;             
                }            
            }
            else 
            {
                QueueFree();
            }
        }
    }

    public async void SuckUpAndOut(float yChange = -16f, float delay = 0f)
    {
        Tween tweener = new Tween();
        AddChild(tweener);

        tweener.InterpolateProperty(this, "modulate:a", null, 0f, 0.4f, Tween.TransitionType.Expo, Tween.EaseType.Out, delay);
        tweener.InterpolateProperty(PixelSprite, "global_position:y", null, GlobalPosition.y + yChange, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out, delay);
        tweener.Start();

        // Wait for tween finish
        await ToSignal(tweener, "tween_all_completed");
        QueueFree();
    }
}
