using System;
using Godot;

public class ResetBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.Reset();
    }
}