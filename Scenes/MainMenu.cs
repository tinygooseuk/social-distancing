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

    private async void Play(int numPlayers)
    {
        Global.Reset();
        Global.NumberOfPlayers = numPlayers;

        Scene<Node> gameScene = R.Scenes.GetGameSceneForNumPlayers(numPlayers);
        GetTree().ChangeSceneTo(await gameScene.LoadAsync());
    }

    private void QuitGame()
    {
        GetTree().Quit();
    }
}
