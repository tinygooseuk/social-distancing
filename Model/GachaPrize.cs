using System;
using Godot;
using Godot.Collections;

public class GachaPrize : Resource
{
    // Colour/category
    [Export] public EnemyColour Colour;
    
    // Name and description
    [Export] public string Name;
    [Export] public string Description;

    // What's unlocked
    [Export] public BehaviourModifiersEnum UnlockedBehaviourModfier = BehaviourModifiersEnum.None;
    [Export] public ShootBehavioursEnum UnlockedShootBehaviour = ShootBehavioursEnum.None;

    // Texture to use
    [Export(PropertyHint.File, "*.png")] private string TexturePath;
    public Asset<Texture> Texture => TexturePath;
}