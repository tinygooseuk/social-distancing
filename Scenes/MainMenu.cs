using Godot;
using System;

public class MainMenu : Control
{
    // Subnodes
    [Subnode("VBoxContainer/SinglePlayer")] private Button SinglePlayer;

    public override void _Ready()
    {
        this.FindSubnodes();

        SinglePlayer.GrabFocus();
    }

    private async void StartSinglePlayer()
    {
        Global.NumberOfPlayers = 1;

        Scene<Node> singlePlayerScene = R.Scenes.SinglePlayer;
        GetTree().ChangeSceneTo(await singlePlayerScene.LoadAsync());
    }

    private async void StartTwoPlayer()
    {
        Global.NumberOfPlayers = 2;

        Scene<Node> twoPlayerScene = R.Scenes.TwoPlayer;
        GetTree().ChangeSceneTo(await twoPlayerScene.LoadAsync());
    }

    private void QuitGame()
    {
        GetTree().Quit();
    }
}
