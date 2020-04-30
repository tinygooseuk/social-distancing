using Godot;
using System;

public class InputMethodManager : Node
{
    public enum InputMethodEnum
    {
        Keyboard,
        Controller,
    };
    public InputMethodEnum InputMethod { get; private set; } = InputMethodEnum.Keyboard;
    public bool IsVibrationEnabled => InputMethod == InputMethodEnum.Controller;
    
    public override void _Ready()
    {
        var t = new Timer 
        {
            WaitTime = 0.25f,
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
