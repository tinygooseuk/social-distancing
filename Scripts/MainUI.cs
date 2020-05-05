using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MainUI : Control
{
    // Subnodes
    [Subnode("Score")] public Label ScoreLabel;
    [Subnode] public Button AgainButton;
    [Subnode] public TextureRect FastForwardIcon;
    [Subnode] private HBoxContainer BuffsBox;

    public override void _Ready()
    {
        this.FindSubnodes();

    }

    public async Task FillBuffsBox(int playerIndex)
    {
        Scene<GachaTile> tileScene = R.Prefabs.UI.GACHA_TILE_SMALL;
        tileScene.Load();

        List<GachaPrize> prizesWon = Global.PrizesWon[playerIndex];
        foreach (GachaPrize prize in prizesWon)
        {
            GachaTile newTile = tileScene.Instance();
            newTile.GachaPrize = prize;

            BuffsBox.AddChild(newTile);

            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }
    }
    public async void HideAndEmptyBuffsBox()
    {
        Tween boxTween = new Tween();
        boxTween.InterpolateProperty(BuffsBox, "modulate:a", 1f, 0f, 0.3f, Tween.TransitionType.Quart, Tween.EaseType.InOut);

        BuffsBox.AddChild(boxTween);
        boxTween.Start();
        await ToSignal(boxTween, "tween_all_completed");

        foreach (Node child in BuffsBox.GetChildren())
        {
            child.QueueFree();
        }
        Modulate = Colors.White;
    }
}