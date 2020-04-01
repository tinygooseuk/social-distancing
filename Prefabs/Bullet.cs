using Godot;
using System;
using System.Threading.Tasks;

public class Bullet : KinematicBody2D
{
    public enum ColourEnum { Red, Blue, Yellow }

    // Subnodes
    [Subnode] private AnimatedSprite AnimatedSprite;
    [Subnode] private Tween IntroTween;
    
    // State
    public int FiredByPlayerIndex = 0;

    public float Direction = 0.0f;
    public ColourEnum Colour = ColourEnum.Red;

    private bool FirstFrame = true;
    
    public override void _Ready()
    {
        this.FindSubnodes();

        switch (Colour)
        {
            case ColourEnum.Red: AnimatedSprite.Modulate = new Color(1.0f, 0.0f, 0.0f); break;
            case ColourEnum.Yellow: AnimatedSprite.Modulate = new Color(1.0f, 1.0f, 0.0f); break;
            case ColourEnum.Blue: AnimatedSprite.Modulate = new Color(0.0f, 0.0f, 1.0f); break;
        }

        // Play shot sound
        Asset<AudioStream> Sound_Shoot = R.Sounds.Shoot;
        GetTree().PlaySound2D(Sound_Shoot, relativeTo: this);

        // Tween up the size
        IntroTween.InterpolateProperty(AnimatedSprite, "scale", null, new Vector2(2.0f, 2.0f), 0.1f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
        IntroTween.Start();

        // Show on next frame
        AnimatedSprite.CallDeferred("set_visible", true);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        KinematicCollision2D collision = MoveAndCollide(new Vector2(Direction, 0.0f) * delta * 400.0f);
        if (collision != null && IsInstanceValid(collision.Collider))
        {
            if (FirstFrame)
            {
                Character firedByPlayer = Game.Instance.GetPlayer(FiredByPlayerIndex);
                if (IsInstanceValid(firedByPlayer))
                {
                    firedByPlayer.MarkBulletFailed();
                }
            }

            if (Colour == ColourEnum.Red && collision.Collider is Enemy_Red er)
            {
                KillEnemy(er);

                return;
            }
            if (Colour == ColourEnum.Yellow && collision.Collider is Enemy_Yellow ey)
            {
                KillEnemy(ey);

                return;
            }
            if (Colour == ColourEnum.Blue && collision.Collider is Enemy_Blue eb)
            {
                KillEnemy(eb);

                return;
            }
            else 
            {
                QueueFree();
            }
        }

        FirstFrame = false;
    }

    private void KillEnemy(Enemy e)
    {
        e.Die();

        Game.Instance.KillScore += (int)(500.0f + (float)Game.Instance.CurrentLevel / 10.0f);
        
        // Play enemy death sound
        Asset<AudioStream> Sound_EnemyDeath = R.Sounds.EnemyDeath;
        GetTree().PlaySound2D(Sound_EnemyDeath, relativeTo: this);

        // Shake correct camera
        Character c = Game.Instance.GetPlayer(FiredByPlayerIndex);
        if (IsInstanceValid(c))
        {
            c.ShakeCamera(new Vector2(Direction * 10.0f, (float)GD.RandRange(-8.0f, +8.0f)));
        }
    }
}
