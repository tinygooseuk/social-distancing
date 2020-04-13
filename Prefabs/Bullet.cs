using Godot;
using System;
using System.Threading.Tasks;

public class Bullet : KinematicBody2D
{
    // Subnodes
    [Subnode] private AnimatedSprite AnimatedSprite;
    [Subnode] private Tween IntroTween;
    
    // State
    public int FiredByPlayerIndex = 0;

    public Vector2 Direction = Vector2.Zero;
    public float Speed = 400f;

    public EnemyColour Colour = EnemyColour.Red;
    
    public bool DisableRetry = false;
    private bool FirstFrame = true;
    
    
    public override void _Ready()
    {
        this.FindSubnodes();

        // Set colour
        AnimatedSprite.Modulate = Colour.ToColor();

        // Tween up the size
        IntroTween.InterpolateProperty(AnimatedSprite, "scale", null, new Vector2(2f, 2f), 0.1f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
        IntroTween.Start();

        // Don't collide with current char
        SetCollisionMaskBit(FiredByPlayerIndex, false);

        // Show on next frame
        AnimatedSprite.CallDeferred("set_visible", true);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        KinematicCollision2D collision = MoveAndCollide(Direction * delta * Speed);
        if (collision != null && IsInstanceValid(collision.Collider))
        {
            if (FirstFrame && !DisableRetry)
            {
                Character firedByPlayer = Game.Instance.GetPlayer(FiredByPlayerIndex);
                if (IsInstanceValid(firedByPlayer))
                {
                    firedByPlayer.MarkBulletFailed();
                }
            }

            if (Colour == EnemyColour.Red && collision.Collider is Enemy_Red er)
            {
                KillEnemy(er);

                return;
            }
            if (Colour == EnemyColour.Yellow && collision.Collider is Enemy_Yellow ey)
            {
                KillEnemy(ey);

                return;
            }
            if (Colour == EnemyColour.Blue && collision.Collider is Enemy_Blue eb)
            {
                KillEnemy(eb);

                return;
            }
            else 
            {
                QueueFree();
                return;
            }
        }

        if (FirstFrame)
        {
            // Play shot sound        
            Asset<AudioStream> Sound_Shoot = R.Sounds.Shoot;
            GetTree().PlaySound2D(Sound_Shoot, relativeTo: this);
        }

        FirstFrame = false;
    }

    private void KillEnemy(Enemy e)
    {
        e.Die();

        Game.Instance.KillScore += (int)(500f + (float)Game.Instance.CurrentLevel / 10f);
        
        // Play enemy death sound
        Asset<AudioStream> Sound_EnemyDeath = R.Sounds.EnemyDeath;
        GetTree().PlaySound2D(Sound_EnemyDeath, relativeTo: this);

        // Shake correct camera
        Character c = Game.Instance.GetPlayer(FiredByPlayerIndex);
        if (IsInstanceValid(c))
        {
            Vector2 randomShake = new Vector2((float)GD.RandRange(-8f, +8f), (float)GD.RandRange(-8f, +8f));
            c.ShakeCamera(Direction * 8f + randomShake);
        }
    }
}
