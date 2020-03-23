using Godot;
using System;

public class Character : KinematicBody2D
{
    // Subnodes    
    [Subnode] private AnimatedSprite Sprite;
    [Subnode] private CollisionShape2D Collider;
    [Subnode] private AnimationPlayer AnimationPlayer;

    [Subnode("Sounds/Jump")] private AudioStreamPlayer Sound_Jump;
    [Subnode("Sounds/PlayerDeath")] private AudioStreamPlayer Sound_Death;
    
    // Consts
    private static float GRAVITY = 9.8f;
    private static float MOVE_SPEED = 40.0f;
    private static float JUMP_IMPULSE = 300.0f;
    private static float FRICTION = 0.15f;

    // State
    private float LastGrounded = 100.0f;
    private float LastBullet = 100.0f;
    private bool IsRight = true;
    private Vector2 Velocity = Vector2.Zero;
    private bool IsDead = false;

    private int BaseScore = 0;
    private int KillScore = 0;
    public int Score => BaseScore + KillScore;

    private bool IsGrounded => LastGrounded < 0.2f;

    public override void _Ready()
    {
        this.FindSubnodes();
    }

    public override void _Process(float delta)
    {
        // Input
        ProcessInput(delta);

        // "Physics"
        ApplyPhysics();
        CheckEnemies();
        UpdateGrounded(delta);

        // Graphics
        UpdateSprite();
        
        // Update score
        float score = -(Position.y - 207.0f);
        BaseScore = (int)Mathf.Max(score, BaseScore);
        Main.Instance.Score.Text = $"Score: {Score}";
    }

    public void OnEnemyDied()
    {
        int roughLevel = BaseScore / 100; // FIXME: bad.
        KillScore += (int)(500.0f + (float)roughLevel / 10.0f);
    }

    private void ProcessInput(float delta)
    {
        if (IsDead)
        {
            return;
        }

        float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        Velocity.x += move * MOVE_SPEED;

        if (!Mathf.IsEqualApprox(Velocity.x, 0.0f))
        {
            IsRight = Mathf.Sign(Velocity.x) == 1;
        }

        bool jump = Input.IsActionJustPressed("jump") && IsGrounded;
        if (jump) 
        {
            AnimationPlayer.Play("Jump");
            Sound_Jump.Play();
            Velocity.y = -JUMP_IMPULSE;
        }

        LastBullet += delta;
        if (Input.IsActionJustPressed("hit_red") && LastBullet > 0.5f)
        {
            FireBullet(Bullet.ColourEnum.Red);
        }
        if (Input.IsActionJustPressed("hit_yellow") && LastBullet > 0.5f)
        {
            FireBullet(Bullet.ColourEnum.Yellow);
        }
        if (Input.IsActionJustPressed("hit_blue") && LastBullet > 0.5f)
        {
            FireBullet(Bullet.ColourEnum.Blue);
        }
    }

    private void FireBullet(Bullet.ColourEnum colour)
    {
        PackedScene bulletScene = (PackedScene)GD.Load("res://Prefabs/Bullet.tscn");
        Bullet bullet = (Bullet)bulletScene.Instance();
        bullet.Colour = colour;
        bullet.Direction = IsRight ? 1.0f : -1.0f;
        bullet.Position = Position + new Vector2(bullet.Direction * 30.0f, -8.0f);
        GetParent().AddChild(bullet);        
    }

    private void ApplyPhysics()
    {
        if (IsDead)
        {
            Velocity.y += GRAVITY;
            Velocity.x = 0.0f;

            Velocity = MoveAndSlide(Velocity, upDirection: Vector2.Up);
            return;
        }

        Velocity.y += GRAVITY;
        Velocity.x *= (1.0f - FRICTION);

        Velocity = MoveAndSlide(Velocity, upDirection: Vector2.Up);
    }

    private void CheckEnemies()
    {
        for (int i = 0; i < GetSlideCount(); i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);
            if (collision.Collider is Enemy e)
            {
                IsDead = true;
                Sound_Death.Play();
                Main.Instance.PlayerDied();
            }
        }
    }

    private void UpdateGrounded(float delta)
    {
        LastGrounded += delta;

        if (IsOnFloor())
        {
            if (LastGrounded > delta)
            {
                AnimationPlayer.Play("Land");
            }
            LastGrounded = 0.0f;        
        }    
    }

    private void UpdateSprite()
    {
        if (Mathf.Abs(Velocity.x) < 1.0f)
        {
            Sprite.PlayIfNotAlready("Idle");
        }
        else
        {
            Sprite.PlayIfNotAlready("WalkRight");
        }

        Sprite.Scale = new Vector2(2.0f * (IsRight ? 1 : -1), 2.0f);
    }
}

