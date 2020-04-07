using Godot;
using System;

public class AgainButton : Button
{
    private void OnPressed()
    {
        Global.Reset();
        Game.Instance.ReloadGameScene();
    }
}
