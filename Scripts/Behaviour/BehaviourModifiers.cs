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
}

public static class BehaviourModifiersFactory 
{
    public static IBehaviourModifier Create(BehaviourModifiersEnum behaviourModifier)
    {
        switch (behaviourModifier)
        {
            case BehaviourModifiersEnum.None:
                return null;

            case BehaviourModifiersEnum.SlippyFriction:
                return new SlippyFrictionBehaviourModifier();
            case BehaviourModifiersEnum.GrippyFriction:
                return new GrippyFrictionBehaviourModifier();

            case BehaviourModifiersEnum.MoonGravity:
                return new MoonGravityBehaviourModifier();

            case BehaviourModifiersEnum.HigherJumpHeight:
                return new HigherJumpHeightBehaviourModifier();
            case BehaviourModifiersEnum.LowerJumpHeight:
                return new LowerJumpHeightBehaviourModifier();

            case BehaviourModifiersEnum.BiggerScale:
                return new BiggerScaleBehaviourModifier();
            case BehaviourModifiersEnum.SmallerScale:
                return new SmallerScaleBehaviourModifier();

            case BehaviourModifiersEnum.ShootFaster:
                return new ShootFasterBehaviourModifier();
            case BehaviourModifiersEnum.ShootSlower:
                return new ShootSlowerBehaviourModifier();
            
            case BehaviourModifiersEnum.AirJumpMore:
                return new AirJumpMoreBehaviourModifier();
            
            case BehaviourModifiersEnum.AirJumpLess:
                return new AirJumpLessBehaviourModifier();
            
            case BehaviourModifiersEnum.Reset:
                return new ResetBehaviourModifier();
        }

        Type t = MethodBase.GetCurrentMethod().DeclaringType;
        throw new InvalidOperationException($"Missing case in {t?.Name}");
    }

    public static IBehaviourModifier CreateRandom()
    {
        int random = (int)GD.RandRange(0, EnumUtil.GetCount<BehaviourModifiersEnum>());
        return Create((BehaviourModifiersEnum)random);
    }
}
