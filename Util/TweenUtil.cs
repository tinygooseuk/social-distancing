using System;
using Godot;

public static class TweenUtil
{
    public static async void FireAndForget(this Tween tween, Node addTo)
    {
        addTo.AddChild(tween);
        tween.Start();

        await addTo.ToSignal(tween, "tween_all_completed");
        addTo.RemoveChild(tween);
    }
}