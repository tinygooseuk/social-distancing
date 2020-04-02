using System;
using Godot;

public class MoveFasterBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.MoveSpeed *= 2.0f;
    }
}

public class MoveSlowerBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.MoveSpeed /= 2.0f;
    }
}