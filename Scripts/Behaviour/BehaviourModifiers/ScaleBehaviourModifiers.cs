using System;
using Godot;

public class BiggerScaleBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {        
        mods.MoveSpeed /= 1.2f;
        mods.CharacterScale *= 1.2f;
    }
}

public class SmallerScaleBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.MoveSpeed *= 1.2f;
        mods.CharacterScale /= 1.2f;
    }
}