using Godot;
using System;

public class DebugUI : Control
{
    // Subnodes
    [Subnode("Vbox/FrameNum")] private Label FrameNumberLabel;
    [Subnode("Vbox/FPSCounter")] private Label FPSCounter;
    [Subnode("Vbox/Level")] private Label LevelLabel;
    [Subnode("Vbox/Difficulty")] private Label DifficultyLabel;
    [Subnode("Vbox/Input")] private Label InputMethodLabel;

    public override void _Ready()
    {
        this.FindSubnodes();

        Visible = false;
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
        if (fps > 58f)
        {
            FPSCounter.Modulate = Colors.Lime;
        }
        else if (fps > 45f)
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

        // Input
        switch (Game.Instance.InputMethodManager.InputMethod)
        {
            case InputMethodManager.InputMethodEnum.Controller: 
                InputMethodLabel.Text = "CNTRL: CTRLR"; 
                break;
            case InputMethodManager.InputMethodEnum.Keyboard: 
                InputMethodLabel.Text = "CNTRL: KYBRD"; 
                break;
            default:
                InputMethodLabel.Text = "CNTRL: ?"; 
                break;
        }
    }
}
