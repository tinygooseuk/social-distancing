using Godot;
using System;

public class TouchControls : Control
{
    public override void _Ready()
    {
        Visible = PlatformUtil.IsMobile;
    }
}