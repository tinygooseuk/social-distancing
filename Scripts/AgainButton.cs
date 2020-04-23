using Godot;
using System;

public class AgainButton : Button
{
    private async void OnPressed()
    {
        // Wait for animation to end
        await Game.Instance.TitleCard.AnimateOut();

        Global.Reset();
        Game.Instance.ReloadGameScene();
    }
}
