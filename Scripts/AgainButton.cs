using Godot;
using System;

public class AgainButton : Button
{
    private void OnPressed()
    {
        Main.Instance.RestartGame();
    }
}
