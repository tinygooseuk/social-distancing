using System;
using Godot;

public class BiggerScaleBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.CharacterScale *= 1.1f;
    }
}

public class SmallerScaleBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.CharacterScale /= 1.5f;
    }
}