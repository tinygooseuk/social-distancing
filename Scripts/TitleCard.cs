using Godot;
using System;
using System.Threading.Tasks;

public class TitleCard : ColorRect
{
    // Subnodes
    [Subnode] Label RoundLabel;
    [Subnode] AnimationPlayer AnimationPlayer;

    [Subnode] TextureRect FastForwardIcon;

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

    public override void _Ready()
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

        _ = AnimateIn();
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

            FastForwardIcon.Visible = Mathf.PosMod(CurrentTime, 2f) > 1f;
        }
        else
        {
            FastForwardIcon.Visible = false;
        }
    }

    public async Task AnimateIn()
    {
        Visible = true;

        AnimationPlayer.Play("AnimStart");
        await ToSignal(AnimationPlayer, "animation_finished");

        RoundLabel.Visible = false;

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