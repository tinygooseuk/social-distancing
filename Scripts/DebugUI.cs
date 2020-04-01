using Godot;
using System;

public class DebugUI : Control
{
    // Subnodes
    [Subnode("Vbox/FrameNum")] private Label FrameNumberLabel;
    [Subnode("Vbox/FPSCounter")] private Label FPSCounter;

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

        FrameNumberLabel.Text = $"F#: {Engine.GetFramesDrawn():D6}";
        
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
    } 
}
