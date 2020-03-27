using Godot;
using System;

public class Character : KinematicBody2D
{
    // Exports
    [Export] private int _PlayerIndex = 0;
    public int PlayerIndex 
    {
        get => _PlayerIndex;
        set
        {
             _PlayerIndex = value; 
            UpdatePlayerIndex();
        }
    }   

    // Subnodes    
    [Subnode] private AnimatedSprite Sprite;
    [Subnode] private CollisionShape2D Collider;
    [Subnode] private AnimationPlayer AnimationPlayer;

    [Subnode("Sounds/Jump")] private AudioStreamPlayer2D Sound_Jump;
    [Subnode("Sounds/PlayerDeath")] private AudioStreamPlayer2D Sound_Death;
    
    // Consts
    private static float GRAVITY = 9.8f;
    private static float MOVE_SPEED = 40.0f;
    private static float JUMP_IMPULSE = 300.0f;
    private static float FRICTION = 0.15f;

    // State
    public Camera2D Camera;

    private float LastGrounded = 100.0f;
    private float LastBullet = 100.0f;
    private bool IsRight = true;
    private Vector2 Velocity = Vector2.Zero;
    private bool IsDead = false;

    private Vector2 LerpedCameraOffset = Vector2.Zero;

    private bool IsGrounded => LastGrounded < 0.2f;

    public override void _Ready()
    {
        this.FindSubnodes();
    }

    public override void _PhysicsProcess(float delta)
    {
        // "Physics"
        ApplyPhysics();
        UpdateGrounded(delta);
    }

    public override void _Process(float delta)
    {
        // Input
        ProcessInput(delta);
        ProcessCameraInput(delta);

        // Graphics
        UpdateSprite();
        
        // Update score
        float score = -(Position.y - 207.0f);
        Game.Instance.BaseScore = (int)Mathf.Max(score, Game.Instance.BaseScore);
    }

    private void ProcessInput(float delta)
    {
        if (IsDead)
        {
            return;
        }

        float move = Input.GetActionStrength($"move_right_{PlayerIndex}") - Input.GetActionStrength($"move_left_{PlayerIndex}");
        Velocity.x += move * MOVE_SPEED;

        if (Mathf.Abs(move) > 0.1f)
        {
            IsRight = Mathf.Sign(move) == 1;
        }

        bool jump = Input.IsActionJustPressed($"jump_{PlayerIndex}") && IsGrounded;
        if (jump) 
        {
            AnimationPlayer.Play("Jump");
            Sound_Jump.Play();
            Velocity.y = -JUMP_IMPULSE;
        }

        LastBullet += delta;
        if (LastBullet > 0.5f)
        {
            if (Input.IsActionJustPressed($"hit_red_{PlayerIndex}"))
            {
                FireBullet(Bullet.ColourEnum.Red);
            }
            if (Input.IsActionJustPressed($"hit_yellow_{PlayerIndex}"))
            {
                FireBullet(Bullet.ColourEnum.Yellow);
            }
            if (Input.IsActionJustPressed($"hit_blue_{PlayerIndex}"))
            {
                FireBullet(Bullet.ColourEnum.Blue);
            }
        }
    }

    private void ProcessCameraInput(float delta)
    {
        if (!IsDead)
        {
            float desiredCameraOffsetX = Input.GetActionStrength($"look_right_{PlayerIndex}") - Input.GetActionStrength($"look_left_{PlayerIndex}");
            LerpedCameraOffset.x = (Global.NumberOfPlayers == 1) ? 0.0f : Mathf.Lerp(LerpedCameraOffset.x, desiredCameraOffsetX * 70.0f, 0.1f);

            float rootCameraOffsetY = Mathf.Clamp(Position.y * 0.2f, -40.0f, 0.0f);
            float desiredCameraOffsetY = Input.GetActionStrength($"look_down_{PlayerIndex}") - Input.GetActionStrength($"look_up_{PlayerIndex}");
            LerpedCameraOffset.y = Mathf.Lerp(LerpedCameraOffset.y, desiredCameraOffsetY * 70.0f, 0.1f);

            Camera.Offset = new Vector2(LerpedCameraOffset.x, rootCameraOffsetY + LerpedCameraOffset.y);
        }
                
        Camera.GlobalPosition = GlobalPosition;
    }

    private void FireBullet(Bullet.ColourEnum colour)
    {
        Scene<Bullet> bulletScene = R.Prefabs.Bullet;

        Bullet bullet = bulletScene.Instance();
        bullet.FiredByPlayerIndex = PlayerIndex;
        bullet.Colour = colour;
        bullet.Direction = IsRight ? 1.0f : -1.0f;
        bullet.Position = Position + new Vector2(bullet.Direction * 30.0f, -6.0f);
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

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        Sound_Death.Play();
        Game.Instance.PlayerDied(PlayerIndex);
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

    private void UpdatePlayerIndex()
    {
        // Set collision layer based on player index
        CollisionLayer = (uint)PlayerIndex+1;

        // Set colour too
        Modulate = new []
        {
            new Color(0.91f, 0.48f, 0.03f),
            new Color(0.57f, 0.03f, 0.91f),
            new Color(0.8f, 0.91f, 0.03f),
            new Color(0.91f, 0.03f, 0.11f),
        }[PlayerIndex];
    }
}

