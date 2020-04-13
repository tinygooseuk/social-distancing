using System;
using Godot;

public class SlippyFrictionBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.Friction /= 1.2f;
    }
}

public class GrippyFrictionBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.Friction *= 1.2f;
    }
}