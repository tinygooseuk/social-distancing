using Godot;
using System;
using System.Threading.Tasks;

public class TitleCard : ColorRect
{
    // Subnodes
    [Subnode] Label RoundLabel;
    [Subnode] AnimationPlayer AnimationPlayer;

    // Shader params
    private float TransitionValue 
    {
        get => (float)((ShaderMaterial)Material).GetShaderParam("progress");
        set => ((ShaderMaterial)Material).SetShaderParam("progress", value);
    }

    private float TargetSeparation = 0f;
    private float FastForwardSeparation
    {
        get => (float)((ShaderMaterial)Material).GetShaderParam("ffwd_separation");
        set => ((ShaderMaterial)Material).SetShaderParam("ffwd_separation", value);
    }

    // Public state
    public bool ShowFastForwardFX = false;

    // Private state
    private float CurrentTime = 0f;

    public override async void _Ready()
    {
        this.FindSubnodes();     

        FastForwardSeparation = 0f;

        if (Global.RoundNumber == 0)
        {
            RoundLabel.Text = "Get Ready!";
        }
        else
        {
            RoundLabel.Text = $"Round {Global.RoundNumber}";
        }

        await ToSignal(GetTree(), "idle_frame");
        await Game.Instance.MainUI.FillBuffsBox(0); // player index 0?
        _ = AnimateIn();
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        Game.Instance.MainUI.HideAndEmptyBuffsBox();
    }

    public override void _Process(float delta)
    {
        if (!ShowFastForwardFX) return;

        bool isFastForwarding = TargetSeparation > 0.8f;

        // Add VHS effect
        TargetSeparation = Mathf.MoveToward(TargetSeparation, Input.GetActionStrength("fast_forward") * 5f, delta * 10f);       
        FastForwardSeparation = TargetSeparation + (isFastForwarding? (float)GD.RandRange(-1.2f, +1.2f) : 0f);        

        if (isFastForwarding)
        {
            // Show/hide ffwd icon
            CurrentTime += delta;

            Game.Instance.MainUI.FastForwardIcon.Visible = Mathf.PosMod(CurrentTime, 2f) > 1f;
        }
        else
        {
            Game.Instance.MainUI.FastForwardIcon.Visible = false;
        }
    }

    public async Task AnimateIn()
    {
        Visible = true;

        AnimationPlayer.Play("AnimStart");
        await ToSignal(AnimationPlayer, "animation_finished");

        RoundLabel.Visible = false;
        Game.Instance.MainUI.ScoreLabel.Visible = true;

        if (PlatformUtil.IsMobile)
        {
            Game.Instance.TouchControls_Main.SetTouchControlsVisible(true);   
        }
    }

    public async Task AnimateOut()
    {
        Visible = true;

        AnimationPlayer.Play("AnimEnd");
        await ToSignal(AnimationPlayer, "animation_finished");
    
        Visible = false;
    }
}