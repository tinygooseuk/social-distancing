using Godot;
using System;
using System.Linq;

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
    [Subnode] private Area2D InsideWallDetector;

    [Subnode("Sounds/Jump")] private AudioStreamPlayer2D Sound_Jump;
    [Subnode("Sounds/Drop")] private AudioStreamPlayer2D Sound_Drop;
    [Subnode("Sounds/PlayerDeath")] private AudioStreamPlayer2D Sound_Death;
    
    // Consts
    private static float GRAVITY = 9.8f;
    private static float MOVE_SPEED = 40.0f;
    private static float JUMP_IMPULSE = 300.0f;
    private static float JUMP_DEBOUNCE = 0.4f;
    private static float FRICTION = 0.15f;

    // State
    public Camera2D Camera;

    private float LastGrounded = 100.0f;
    private float LastJump = 100.0f;
    private float LastBullet = 100.0f;
    private bool IsRight = true;
    private Vector2 Velocity = Vector2.Zero;
    private bool IsDead = false;
    private bool IsLevelComplete = false;
    
    private bool IsPassingThrough = false;
    private bool IsInsideWall = false;
    private float ForceFallTime = 0.0f;

    private Vector2 LerpedCameraOffset = Vector2.Zero;

    private Vector2 CameraShakeMagnitude = Vector2.Zero;
    private Vector2 CameraShakeOffset = Vector2.Zero;

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

        // Update inside-ness
        int insides = 0;
        foreach (var overlapObject in InsideWallDetector.GetOverlappingBodies())
        {
            if (overlapObject is TileMap)
            {
                insides++;
            }
        }
        IsInsideWall = insides > 0 || ForceFallTime > 0.0f;
    }

    public override void _Process(float delta)
    {
        // Input
        ProcessInput(delta);
        ProcessCameraInput(delta);

        // Camera shake
        if (CameraShakeMagnitude.Length() > 0.001f)
        {
            CameraShakeOffset = new Vector2 
            {
                x = (float)GD.RandRange(-CameraShakeMagnitude.x, +CameraShakeMagnitude.x), 
                y = (float)GD.RandRange(-CameraShakeMagnitude.y, +CameraShakeMagnitude.y), 
            };
            CameraShakeMagnitude *= 0.9f;

            float weakShake = Mathf.Clamp(Mathf.Abs(CameraShakeMagnitude.y / 2.0f), 0.01f, 1.0f);
            float strongShake = Mathf.Clamp(Mathf.Abs(CameraShakeMagnitude.x / 2.0f), 0.01f, 1.0f);

            Input.StartJoyVibration(PlayerIndex, weakShake, strongShake, 0.5f);
        }
        else
        {
            CameraShakeOffset = Vector2.Zero;
            Input.StopJoyVibration(PlayerIndex);
        }

        bool isCancellableSpeed = Velocity.y >= 0;// || Mathf.Abs(Velocity.x) > Mathf.Abs(Velocity.y);
        if (IsPassingThrough && isCancellableSpeed && !IsInsideWall)
        {
            IsPassingThrough = false;
            SetCollisionMaskBit(6, true);
        }

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

        // Add movement
        float move = Input.GetActionStrength($"move_right_{PlayerIndex}") - Input.GetActionStrength($"move_left_{PlayerIndex}");
        Velocity.x += move * MOVE_SPEED;

        // Change facing direction if needed
        if (Mathf.Abs(move) > 0.1f)
        {
            IsRight = Mathf.Sign(move) == 1;
        }

        // Check for jump
        bool jump = Input.IsActionJustPressed($"jump_{PlayerIndex}") && IsGrounded && LastJump > JUMP_DEBOUNCE;
        if (jump) 
        {
            AnimationPlayer.Play("Jump");
         
            if (!IsLevelComplete && Input.IsActionPressed($"move_down_{PlayerIndex}"))
            {
                IsInsideWall = true;
                ForceFallTime = 0.2f;
                Velocity.y = +5.0f;
                Sound_Drop.Play();
            } 
            else 
            {
                Velocity.y = -JUMP_IMPULSE;
                Sound_Jump.Play();
            }
            LastJump = 0.0f;

            IsPassingThrough = true;
            SetCollisionMaskBit(6, false);
        }        
        LastJump += delta;        

        // Force falling
        if (ForceFallTime > 0.0f)
        {
            ForceFallTime -= delta;
        }

        // Check for shooting
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
        else
        {
            LastBullet += delta;
        }
    }

    private void ProcessCameraInput(float delta)
    {
        if (!IsDead)
        {
            Vector2 desiredCameraOffset = new Vector2
            {
                x = Input.GetActionStrength($"look_right_{PlayerIndex}") - Input.GetActionStrength($"look_left_{PlayerIndex}"),
                y = Input.GetActionStrength($"look_down_{PlayerIndex}") - Input.GetActionStrength($"look_up_{PlayerIndex}"),
            };

            if (IsLevelComplete)
            {
                desiredCameraOffset.y = (Global.NumberOfPlayers == 2) ? +1.0f : -0.69f; // arbitrary-ish
            }

            LerpedCameraOffset.x = 0.0f;// (Global.NumberOfPlayers == 1) ? 0.0f : Mathf.Lerp(LerpedCameraOffset.x, desiredCameraOffset.x * 70.0f, 0.1f);
            LerpedCameraOffset.y = Mathf.Lerp(LerpedCameraOffset.y, desiredCameraOffset.y * 70.0f, 0.1f);
        }

        float rootCameraOffsetY = Mathf.Clamp(Position.y * 0.2f, -40.0f, 0.0f);
        Camera.Offset = new Vector2(LerpedCameraOffset.x, rootCameraOffsetY + LerpedCameraOffset.y) + CameraShakeOffset;                
        Camera.GlobalPosition = GlobalPosition;
    }

    private void FireBullet(Bullet.ColourEnum colour)
    {
        Scene<Bullet> bulletScene = R.Prefabs.Bullet;

        Bullet bullet = bulletScene.Instance();
        bullet.FiredByPlayerIndex = PlayerIndex;
        bullet.Colour = colour;
        bullet.Direction = IsRight ? 1.0f : -1.0f;
        bullet.Position = Position + new Vector2(bullet.Direction * 20.0f, -4.0f);
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

        ShakeCamera(new Vector2(250.0f, 250.0f));
    }

    public void MarkLevelComplete()
    {
        IsLevelComplete = true;
    }

    public void ShakeCamera(Vector2 magnitude) => CameraShakeMagnitude += magnitude;

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

        // Grab reference to our camera
        Camera = Game.Instance.GetPlayerCamera(PlayerIndex);

        // Set colour too
        Modulate = new []
        {
            new Color(0.91f, 0.48f, 0.03f),
            new Color(0.57f, 0.03f, 0.91f),
            new Color(0.8f, 0.91f, 0.03f),
            new Color(0.91f, 0.03f, 0.11f),
        }[PlayerIndex];

        // Update all pitches
        foreach (AudioStreamPlayer2D sound in GetNode("Sounds").GetChildren())
        {
            sound.PitchScale = 1.0f + ((float)PlayerIndex) / 16.0f;
        }
    }
}

