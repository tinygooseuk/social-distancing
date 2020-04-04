using Godot;
using System;

public class Spring : Area2D
{
    // Exports
    [Export] private float Power = 600.0f;

    // Subnodes
    [Subnode] private Sprite Sprite;
    [Subnode] private AudioStreamPlayer2D BoingSound;
    [Subnode] private Tween SpringTween;

    public override void _Ready()
    {
        this.FindSubnodes();
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.SetVelocityY(-Power);
            Boing();
        }

        if (body is Character character)
        {
            character.ShakeCamera(new Vector2(0.0f, -Power / 40.0f));
            character.SetVelocityY(-Power);   

            Boing();
        }
    }    

    private void Boing()
    {
        // Play sound
        BoingSound.Play();

        // Play tween
        Vector2 originalPosition = Sprite.Position;
        Sprite.Position = new Vector2(Sprite.Position.x, Sprite.Position.y - 6.0f);

        SpringTween.InterpolateProperty(Sprite, "position:y", null, originalPosition.y, 1.0f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        SpringTween.Start();
    }
}
