using Godot;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

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
        // Find nearest player
        Character nearestPlayer = Game.Instance.GetNearestPlayer(GlobalPosition);
        
        if (IsInstanceValid(nearestPlayer))
        {
            Vector2 move = Move(nearestPlayer.GlobalPosition, Game.Instance.GetAIDifficultyScale(Game.Instance.CurrentLevel));
            Velocity += move;
        }

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
                break;
            }
        }
    }

    public void Die()
    {       
        IEnumerable<Pixel> pixels = EnemySprite.BurstIntoPixels(body: this);
        foreach (Pixel pixel in pixels)
        {
            pixel.ApplyCentralImpulse(Velocity);
        }

        // Spawn particles
        Scene<Particles2D> deathParticlesScene = R.Particles.DeathParticles;
        
        Particles2D deathParticles = deathParticlesScene.Instance();
        deathParticles.Position = Position;
        
        ParticlesMaterial processMaterial = (ParticlesMaterial)deathParticles.ProcessMaterial;
        GradientTexture gradientTexture = (GradientTexture)processMaterial.ColorRamp;  
        Gradient gradient = gradientTexture.Gradient;
        gradient.SetColor(0, GetColour());

        GetParent().AddChild(deathParticles);      

        // Free
        QueueFree();
    }
}
