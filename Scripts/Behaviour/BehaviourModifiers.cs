using System;
using System.Reflection;
using Godot;

public enum BehaviourModifiersEnum
{
    SlippyFriction,
    GrippyFriction,

    MoonGravity,

    HigherJumpHeight,
    LowerJumpHeight,

    MoveFaster,
    MoveSlower,

    BiggerScale,
    SmallerScale,

    ShootFaster,
    ShootSlower,
}

public static class BehaviourModifiersFactory 
{
    public static IBehaviourModifier Create(BehaviourModifiersEnum behaviourModifier)
    {
        switch (behaviourModifier)
        {
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

            case BehaviourModifiersEnum.MoveFaster:
                return new MoveFasterBehaviourModifier();
            case BehaviourModifiersEnum.MoveSlower:
                return new MoveSlowerBehaviourModifier();

            case BehaviourModifiersEnum.BiggerScale:
                return new BiggerScaleBehaviourModifier();
            case BehaviourModifiersEnum.SmallerScale:
                return new SmallerScaleBehaviourModifier();

            case BehaviourModifiersEnum.ShootFaster:
                return new ShootFasterBehaviourModifier();
            case BehaviourModifiersEnum.ShootSlower:
                return new ShootSlowerBehaviourModifier();
        }

        Type t = MethodBase.GetCurrentMethod().DeclaringType;
        throw new InvalidOperationException($"Missing case in {t.Name}");
    }

    public static IBehaviourModifier CreateRandom()
    {
        int random = (int)GD.RandRange(0, Enum.GetValues(typeof(BehaviourModifiersEnum)).Length);
        return Create((BehaviourModifiersEnum)random);
    }
}
