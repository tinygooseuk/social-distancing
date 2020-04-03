using Godot;
using System;

public class InputMethodManager : Node
{
    public enum InputMethodEnum
    {
        Unknown,
        Keyboard,
        Controller,
    };
    public InputMethodEnum InputMethod { get; private set; } = InputMethodEnum.Unknown;

    public override void _Ready()
    {
        Timer t = new Timer 
        {
            WaitTime = 0.5f,
            Autostart = true,
            OneShot = false,
        };
        AddChild(t);
        t.Connect("timeout", this, nameof(UpdateInputMethod));        
    }

    private void UpdateInputMethod()
    {
        if (Input.IsActionPressed("inputmethod_keyboard"))
        {
            InputMethod = InputMethodEnum.Keyboard;
        } 
        else if (Input.IsActionPressed("inputmethod_controller"))
        {
            InputMethod = InputMethodEnum.Controller;
        }
    }
}
