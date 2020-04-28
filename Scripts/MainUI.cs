using Godot;
using System;

public class MainUI : Control
{
    // Subnodes
    [Subnode("Score")] public Label ScoreLabel;
    [Subnode] public Button AgainButton;
    [Subnode] public TextureRect FastForwardIcon;

    public override void _Ready()
    {
        this.FindSubnodes();

    }
}