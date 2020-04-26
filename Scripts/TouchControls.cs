using Godot;

public class TouchControls : Control
{
    // Singleton
    public static TouchControls Instance { get; private set; } = null;
    
    // Consts
    private const float CONTROLS_ALPHA = 0.25f;
    
    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public async void SetTouchControlsVisible(bool newVisible, bool animated = true)
    {
        if (animated)
        {
            float sourceAlpha = newVisible == true ? 0f : CONTROLS_ALPHA;
            float destAlpha = newVisible == true ? CONTROLS_ALPHA : 0f;
            
            // Set visible, set alpha to source
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, sourceAlpha);
            Visible = true;

            // Fade in/out to new alpha => wait till done
            
            Tween fadeTween = new Tween();
            fadeTween.InterpolateProperty(this, "modulate:a", sourceAlpha, destAlpha, 0.3f, Tween.TransitionType.Cubic,
                Tween.EaseType.InOut);
            AddChild(fadeTween);

            fadeTween.Start();
            await ToSignal(fadeTween, "tween_all_completed");
            fadeTween.QueueFree();
        }

        // Finally set visible to on/off
        Visible = newVisible;
    }
}