using Godot;
using System;

public class Pixel : RigidBody2D
{
    private float Age = 0.0f;
    private const float LIFETIME = 2.5f;

    public override void _Process(float delta)
    {
        Age += delta;

        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1.0f - (Age / LIFETIME));

        if (Age > LIFETIME)
        {
            QueueFree();
        }
    }
}
