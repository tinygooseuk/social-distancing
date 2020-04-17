using Godot;
using System;

public class GachaTile : Control
{
    // Subnodes
    [Subnode] private TextureRect Background;
    [Subnode] private TextureRect Icon;
    
    [Subnode] private AnimationPlayer AnimationPlayer;

    // State
    private GachaPrize _GachaPrize;
    public GachaPrize GachaPrize
    {
        get => _GachaPrize;
        set { _GachaPrize = value; UpdateGachaPrize(); }
    }

    public override void _Ready()
    {
        this.FindSubnodes();    
    }

    private void UpdateGachaPrize()
    {
        this.FindSubnodes();

        Icon.Texture = GachaPrize.Texture.Load();
    }

    public void PlayWonAnimation()
    {
        AnimationPlayer.Play("Won");
    }
}
