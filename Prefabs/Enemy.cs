using Godot;
using System;
using System.Threading.Tasks;
using System.Text;

public class Enemy : KinematicBody2D
{
    // Subnodes
    protected AnimatedSprite EnemySprite;

    // Override point
    protected virtual bool IsAffectedByGravity() => true;
    protected virtual Vector2 Move(Vector2 playerPosition, float difficultyScale) => Vector2.Zero;
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
        Vector2 move = Move(Game.Instance.Player1.GlobalPosition, Game.Instance.GetAIDifficultyScale(Game.Instance.CurrentLevel));
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

        Scene<Pixel> pixelScene = R.Prefabs.Pixel;
        pixelScene.Load();

        Transform2D transform = new Transform2D
        {
            origin = Vector2.Zero,
            x = GlobalTransform.x,
            y = GlobalTransform.y,
        };

        int numPixels = GetTree().GetNodesInGroup("pixels").Count;
        int skip = 1 + Mathf.RoundToInt(numPixels / 100);
        int counter = 0;

        enemyImage.Lock();
        {
            for (int y = 0; y < enemyTexture.GetHeight(); y++)
            {
                for (int x = 0; x < enemyTexture.GetWidth(); x++)
                {
                    if (counter++ % skip > 0)
                    {
                        continue;
                    }

                    Vector2 pixelOffset = enemyTexture.Region.Position;
                    Color pixelColour = enemyImage.GetPixel((int)pixelOffset.x + x, (int)pixelOffset.y + y);
                    bool isPixel = pixelColour.a > 0.5f;
                    
                    Vector2 offset = new Vector2((float)x * 2.0f, (float)y * 2.0f) - new Vector2(enemyTexture.GetWidth(), enemyTexture.GetHeight());

                    if (isPixel)
                    {
                        Pixel pixel = pixelScene.Instance();
                        pixel.Position = Position + transform.Xform(offset);
                        pixel.Modulate = pixelColour;
                        pixel.ApplyCentralImpulse(Velocity);
                        GetParent().AddChild(pixel);
                    }
                    
                }   
            }
        }
        enemyImage.Unlock();

        QueueFree();

        // Spawn particles
        Scene<Particles2D> deathParticlesScene = R.Prefabs.DeathParticles;
        
        Particles2D deathParticles = deathParticlesScene.Instance();
        deathParticles.Position = Position;
        
        ParticlesMaterial processMaterial = (ParticlesMaterial)deathParticles.ProcessMaterial;
        GradientTexture gradientTexture = (GradientTexture)processMaterial.ColorRamp;  
        Gradient gradient = gradientTexture.Gradient;
        gradient.SetColor(0, GetColour());

        GetParent().AddChild(deathParticles);      
    }
}
