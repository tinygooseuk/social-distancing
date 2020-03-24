using Godot;
using System;
using System.Threading.Tasks;

public class Bullet : KinematicBody2D
{
    public enum ColourEnum { Red, Blue, Yellow }

    // Subnodes
    [Subnode] private AnimatedSprite AnimatedSprite;
    [Subnode("Sounds/EnemyDeath")] private AudioStreamPlayer Sound_EnemyDeath;
    
    // State
    public float Direction = 0.0f;
    public ColourEnum Colour = ColourEnum.Red;
    
    public override void _Ready()
    {
        this.FindSubnodes();

        switch (Colour)
        {
            case ColourEnum.Red: AnimatedSprite.Modulate = new Color(1.0f, 0.0f, 0.0f); break;
            case ColourEnum.Yellow: AnimatedSprite.Modulate = new Color(1.0f, 1.0f, 0.0f); break;
            case ColourEnum.Blue: AnimatedSprite.Modulate = new Color(0.0f, 0.0f, 1.0f); break;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override async void _PhysicsProcess(float delta)
    {
        KinematicCollision2D collision = MoveAndCollide(new Vector2(Direction, 0.0f) * delta * 400.0f);
        if (collision != null && IsInstanceValid(collision.Collider))
        {
            if (Colour == ColourEnum.Red && collision.Collider is Enemy_Red er)
            {
                await KillEnemy(er);
                QueueFree();

                return;
            }
            if (Colour == ColourEnum.Yellow && collision.Collider is Enemy_Yellow ey)
            {
                await KillEnemy(ey);
                QueueFree();

                return;
            }
            if (Colour == ColourEnum.Blue && collision.Collider is Enemy_Blue eb)
            {
                await KillEnemy(eb);
                QueueFree();

                return;
            }
            else 
            {
                QueueFree();
            }
        }
    }

    private async Task KillEnemy(Enemy e)
    {
        e.QueueFree();
        Main.Instance.Player1.OnEnemyDied();

        Sound_EnemyDeath.Play();

        await ToSignal(Sound_EnemyDeath, "finished");
        Visible = false;
    }
}
