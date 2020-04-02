using System;
using Godot;

public class MoonGravityBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.Gravity /= 2.0f;
    }
}