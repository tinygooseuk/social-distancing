using System;
using Godot;

public class HigherJumpHeightBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.JumpImpulse *= 1.2f;
    }
}

public class LowerJumpHeightBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.JumpImpulse = Mathf.Max(280f, mods.JumpImpulse / 1.2f);
    }
}