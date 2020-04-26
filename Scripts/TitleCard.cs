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

    public override void _Ready()
    {
        this.FindSubnodes();     

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
        
    }

    public async Task AnimateIn()
    {
        Visible = true;

        AnimationPlayer.Play("AnimStart");
        await ToSignal(AnimationPlayer, "animation_finished");

        RoundLabel.Visible = false;
        Visible = false;

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