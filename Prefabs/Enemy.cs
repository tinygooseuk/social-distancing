using Godot;
using System;
using System.Threading.Tasks;
using System.Text;

public class Enemy : KinematicBody2D
{
    // Subnodes
    [Subnode("Sprite")] private AnimatedSprite EnemySprite;

    // Override point
    protected virtual bool IsAffectedByGravity() => true;
    protected virtual Vector2 Move(Vector2 playerPosition) => Vector2.Zero;
    protected virtual Color GetColour() => Colors.White;

    // Consts
    private static float GRAVITY = 9.8f;
    private static float FRICTION = 0.15f;

    // State
    protected Vector2 Velocity = Vector2.Zero;

    public override void _Ready()
    {
        EnemySprite = GetNode<AnimatedSprite>("Sprite"); // Why subnode not work here?!
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 move = Move(Main.Instance.Player1.GlobalPosition);
        Velocity += move;

        if (IsAffectedByGravity())
        {
            Velocity.y += GRAVITY;
            Velocity.x *= (1.0f - FRICTION);
        }      

        Velocity = MoveAndSlide(Velocity, upDirection: Vector2.Up);

        for (int slideIndex = 0; slideIndex < GetSlideCount(); slideIndex++)
        {
            KinematicCollision2D collision = GetSlideCollision(slideIndex);
            if (collision != null && IsInstanceValid(collision.Collider) && collision.Collider is Character character)
            {
                character.Die();
            }
        }
    }

    public void Die()
    {       
        AtlasTexture enemyTexture = (AtlasTexture)EnemySprite.Frames.GetFrame("Idle", 0);
        Image enemyImage = enemyTexture.Atlas.GetData();

        PackedScene pixelScene = GD.Load<PackedScene>("res://Prefabs/Pixel.tscn");

        enemyImage.Lock();
        {
            for (int y = 0; y < enemyTexture.GetHeight(); y++)
            {
                for (int x = 0; x < enemyTexture.GetWidth(); x++)
                {
                    Vector2 offset = enemyTexture.Region.Position;

                    bool isPixel = enemyImage.GetPixel((int)offset.x + x, (int)offset.y + y).a > 0.5f;
                    
                    if (isPixel)
                    {
                        Pixel pixel = (Pixel)pixelScene.Instance();
                        pixel.Position = Position + new Vector2((float)x * 2.0f, (float)y * 2.0f) - new Vector2(enemyTexture.GetWidth(), enemyTexture.GetHeight());
                        pixel.Modulate = GetColour();
                        pixel.ApplyCentralImpulse(Velocity);
                        GetParent().AddChild(pixel);
                    }
                    
                }   
            }
        }
        enemyImage.Unlock();

        QueueFree();

        // Spawn particles
        PackedScene deathParticlesScene = GD.Load<PackedScene>("res://Prefabs/DeathParticles.tscn");
        
        Particles2D deathParticles = (Particles2D)deathParticlesScene.Instance();
        deathParticles.Position = Position;
        
        ParticlesMaterial processMaterial = (ParticlesMaterial)deathParticles.ProcessMaterial;
        GradientTexture gradientTexture = (GradientTexture)processMaterial.ColorRamp;  
        Gradient gradient = gradientTexture.Gradient;
        gradient.SetColor(0, GetColour());

        GetParent().AddChild(deathParticles);      
    }
}
