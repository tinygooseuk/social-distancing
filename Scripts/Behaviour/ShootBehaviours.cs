using System;
using System.Reflection;
using Godot;

public enum ShootBehavioursEnum
{
    Default,
    OmniDirectional,
}

public static class ShootBehavioursFactory 
{
    public static IShootBehaviour Create(ShootBehavioursEnum behaviourModifier)
    {
        switch (behaviourModifier)
        {
            case ShootBehavioursEnum.Default:
                return new DefaultShootBehaviour();
                
            case ShootBehavioursEnum.OmniDirectional:
                return new OmniDirectionalShootBehaviour();
        }

        Type t = MethodBase.GetCurrentMethod().DeclaringType;
        throw new InvalidOperationException($"Missing case in {t.Name}");
    }

    public static IShootBehaviour CreateRandom()
    {
        int random = (int)GD.RandRange(0, Enum.GetValues(typeof(ShootBehavioursEnum)).Length);
        return Create((ShootBehavioursEnum)random);
    }
}
