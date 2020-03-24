using Godot;
using System;
using System.Threading.Tasks;

public class Enemy : KinematicBody2D
{
    // Override point
    protected virtual bool IsAffectedByGravity() => true;
    protected virtual Vector2 Move(Vector2 playerPosition) => Vector2.Zero;
    protected virtual Color GetColour() => Colors.White;

    // Consts
    private static float GRAVITY = 9.8f;
    private static float FRICTION = 0.15f;

    // State
    protected Vector2 Velocity = Vector2.Zero;

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
