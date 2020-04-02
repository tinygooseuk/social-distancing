using System;
using Godot;

public class SlippyFrictionBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.Friction /= 2.0f;
    }
}

public class GrippyFrictionBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.Friction *= 2.0f;
    }
}