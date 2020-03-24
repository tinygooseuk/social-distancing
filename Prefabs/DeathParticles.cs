using Godot;
using System;

public class DeathParticles : Particles2D
{
    public async override void _Ready()
    {
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        CallDeferred("queue_free");
    }
}
