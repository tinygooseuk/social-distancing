using System;
using System.Reflection;
using Godot;

public enum BehaviourModifiersEnum
{
    None,

    SlippyFriction,
    GrippyFriction,

    MoonGravity,

    HigherJumpHeight,
    LowerJumpHeight,

    BiggerScale,
    SmallerScale,

    ShootFaster,
    ShootSlower,
    
    AirJumpMore,
    AirJumpLess,
    
    Reset,

    ShootHomingBullets
}

public static class BehaviourModifiersFactory 
{
    public static IBehaviourModifier Create(BehaviourModifiersEnum behaviourModifier)
    {
        return behaviourModifier switch
        {
            BehaviourModifiersEnum.None => null,

            BehaviourModifiersEnum.SlippyFriction => new SlippyFrictionBehaviourModifier(),
            BehaviourModifiersEnum.GrippyFriction => new GrippyFrictionBehaviourModifier(),

            BehaviourModifiersEnum.MoonGravity => new MoonGravityBehaviourModifier(),

            BehaviourModifiersEnum.HigherJumpHeight => new HigherJumpHeightBehaviourModifier(),
            BehaviourModifiersEnum.LowerJumpHeight => new LowerJumpHeightBehaviourModifier(),

            BehaviourModifiersEnum.BiggerScale => new BiggerScaleBehaviourModifier(),
            BehaviourModifiersEnum.SmallerScale => new SmallerScaleBehaviourModifier(),

            BehaviourModifiersEnum.ShootFaster => new ShootFasterBehaviourModifier(),
            BehaviourModifiersEnum.ShootSlower => new ShootSlowerBehaviourModifier(),
            
            BehaviourModifiersEnum.AirJumpMore => new AirJumpMoreBehaviourModifier(),
            
            BehaviourModifiersEnum.AirJumpLess => new AirJumpLessBehaviourModifier(),
            
            BehaviourModifiersEnum.Reset => new ResetBehaviourModifier(),

            BehaviourModifiersEnum.ShootHomingBullets => new HomingShotsBehaviourModifier(),

            _ => null
        };
    }

    public static IBehaviourModifier CreateRandom()
    {
        int random = (int)GD.RandRange(0, EnumUtil.GetCount<BehaviourModifiersEnum>());
        return Create((BehaviourModifiersEnum)random);
    }
}
