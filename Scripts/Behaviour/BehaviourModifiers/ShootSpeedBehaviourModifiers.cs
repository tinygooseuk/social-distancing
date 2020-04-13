using System;
using Godot;

public class ShootFasterBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.ShootDebounce /= 4f;
    }
}

public class ShootSlowerBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.ShootDebounce *= 2f;
    }
}