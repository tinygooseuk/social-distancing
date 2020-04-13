using Godot;
using System;
using System.Threading.Tasks;

public class MainMenu : Control
{
    // Subnodes
    [Subnode("VBoxContainer/SinglePlayer")] private Button SinglePlayer;
    [Subnode("Transition")] private ColorRect Transition;
    [Subnode("Transition/TransitionTween")] private Tween TransitionTween;

    public override void _Ready()
    {
        this.FindSubnodes();
        
        Input.SetMouseMode(Input.MouseMode.Hidden);
        SinglePlayer.GrabFocus();
    }

    private async void Play(int numPlayers)
    {
        // Do the thing
        await RunTransition();

        Global.Reset();
        Global.NumberOfPlayers = numPlayers;

        Scene<Node> gameScene = R.Scenes.GetGameSceneForNumPlayers(numPlayers);
        GetTree().ChangeSceneTo(await gameScene.LoadAsync());
    }

    private async void QuitGame()
    {
        // Do the thing
        await RunTransition();

        GetTree().Quit();
    }

    private async Task RunTransition()
    {
        TransitionTween.InterpolateProperty(Transition.Material, "shader_param/progress", 0.0f, 1.0f, 1.0f); 
        TransitionTween.Start();

        await ToSignal(TransitionTween, "tween_all_completed");
    }    
}
