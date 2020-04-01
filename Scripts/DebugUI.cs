using Godot;
using System;

public class DebugUI : Control
{
    // Subnodes
    [Subnode("Vbox/FrameNum")] private Label FrameNumberLabel;
    [Subnode("Vbox/FPSCounter")] private Label FPSCounter;
    [Subnode("Vbox/Level")] private Label LevelLabel;
    [Subnode("Vbox/Difficulty")] private Label DifficultyLabel;

    public override void _Ready()
    {
        this.FindSubnodes();

        Visible = OS.IsDebugBuild();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("toggle_debug") && OS.IsDebugBuild())
        {
            Visible = !Visible;
        }

        if (!Visible) return;

        // Frame number
        FrameNumberLabel.Text = $"F#: {Engine.GetFramesDrawn():D6}";
        
        // FPS
        float fps = Engine.GetFramesPerSecond();
        FPSCounter.Text = $"FPS: {fps}";
        if (fps > 58.0f)
        {
            FPSCounter.Modulate = Colors.Lime;
        }
        else if (fps > 45.0f)
        {
            FPSCounter.Modulate = Colors.Yellow;
        } 
        else 
        {
            FPSCounter.Modulate = Colors.Red;
        }

        // Level
        int lvl = Game.Instance.CurrentLevel;
        LevelLabel.Text = $"LVL: {lvl:D2}";

        // Difficulty
        string diffLevel = Game.GetDifficultyEnumValue(lvl).ToString().ToUpper();
        DifficultyLabel.Text = $"DIFF: {diffLevel} {Game.Instance.GetAIDifficultyScale(lvl):F1}";
    }
}
