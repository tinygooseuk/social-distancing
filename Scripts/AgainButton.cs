using Godot;
using System;

public class AgainButton : Button
{
    private void OnPressed()
    {
        Game.Instance.RestartGame();
    }
}
