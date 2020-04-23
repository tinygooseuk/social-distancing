using Godot;
using System;

public class SelfRemovingParticles : Particles2D
{
    public override async void _Ready()
    {        
        await ToSignal(GetTree().CreateTimer(Lifetime * 0.9f), "timeout");
        CallDeferred("queue_free");
    }
}
