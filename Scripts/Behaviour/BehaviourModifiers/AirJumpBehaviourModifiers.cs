using System;
using Godot;

public class AirJumpMoreBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.NumAirJumps++;
    }
}

public class AirJumpLessBehaviourModifier : IBehaviourModifier
{
    public void Modify(Modifiables mods)
    {
        mods.NumAirJumps = Mathf.Max(1, mods.NumAirJumps - 1);
    }
}