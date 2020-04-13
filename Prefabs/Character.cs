using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

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
            this.FindSubnodes(); // just in case
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
        
    // References
    public Camera2D Camera;

    // State
    private float LastGrounded = 100.0f;
    private float LastJump = 100.0f;
    private float LastBullet = 100.0f;
    private EnemyColour LastBulletColour = EnemyColour.Red;
    private float LastDied = 0.0f;
    private bool IsGrounded => LastGrounded < 0.2f;

    private bool ShouldRefireBullet = false;
    
    public bool IsFacingRight { get; private set; } = true;
    private Vector2 Velocity = Vector2.Zero;
    private bool IsDead = false;
    private bool IsRoundComplete = false;
    public bool UpdateCamera = true;
    
    private bool IsPassingThrough = false;
    private bool IsInsideWall = false;
    private float ForceFallTime = 0.0f;

    // Camera state
    private Vector2 LerpedCameraOffset = Vector2.Zero;

    private Vector2 CameraShakeMagnitude = Vector2.Zero;
    private Vector2 CameraShakeOffset = Vector2.Zero;

    public Modifiables Mods => GetModifiables();
    private Modifiables _CachedModifiables = null;

    #region Engine Callbacks
    public override void _Ready()
    {
        this.FindSubnodes();

        //TODO: Behaviours
        /*ShootBehaviour = ShootBehavioursFactory.CreateRandom();
        GD.Print($"Shoot: {ShootBehaviour}");

        for (int i = 0; i < 3; i++)
        {
            IBehaviourModifier mod = BehaviourModifiersFactory.CreateRandom();
            GD.Print($"+Mod: {mod}");
            AddModifier(mod);
        }*/
    }

    public override void _PhysicsProcess(float delta)
    {
        // "Physics"
        ApplyPhysics(delta);

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

            if (Game.Instance.InputMethodManager.InputMethod == InputMethodManager.InputMethodEnum.Controller)
            {
                Input.StartJoyVibration(PlayerIndex, weakShake, strongShake, 0.5f);
            }
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

        // Death timer
        if (IsDead)
        {
            LastDied += delta;
        }

        // Graphics
        if (Mathf.Abs(Velocity.x) < 1.0f)
        {
            Sprite.PlayIfNotAlready("Idle");
        }
        else
        {
            Sprite.PlayIfNotAlready("WalkRight");
        }

        Sprite.Scale = new Vector2(2.0f * (IsFacingRight ? 1 : -1), Sprite.Scale.y);
        
        // Update score
        float score = -(Position.y - 207.0f);
        Game.Instance.BaseScore = (int)Mathf.Max(score, Game.Instance.BaseScore);
    }
    #endregion

    #region Inputs
    private void ProcessInput(float delta)
    {
        if (IsDead)
        {
            return;
        }

        // Add movement
        float move = Input.GetActionStrength($"move_right_{PlayerIndex}") - Input.GetActionStrength($"move_left_{PlayerIndex}");
        Velocity.x += move * Mods.MoveSpeed;

        // Change facing direction if needed
        bool wasRight = IsFacingRight;
        if (Mathf.Abs(move) > 0.1f)
        {
            IsFacingRight = Mathf.Sign(move) == 1;
        }
        if (wasRight != IsFacingRight)
        {
            ShouldRefireBullet = false;
        }

        // Check for jump
        bool canJumpOrDrop = IsGrounded && LastJump > Mods.JumpDebounce;
        bool canDrop = !IsRoundComplete;
        bool wantsJump = Input.IsActionJustPressed($"jump_{PlayerIndex}");
        bool wantsDrop = Input.IsActionPressed($"move_down_{PlayerIndex}");

        if (canJumpOrDrop) 
        {
            if (wantsJump)
            {
                if (canDrop && wantsDrop)
                {
                    DropDown();
                } 
                else 
                {
                    Jump();
                }
            } 
            else if (canDrop && wantsDrop && Game.Instance.InputMethodManager.InputMethod == InputMethodManager.InputMethodEnum.Keyboard)
            {
                // Special case: for keyboard input, don't require jump press too
                DropDown();
            }
        }        
        LastJump += delta;        

        // Force falling if required
        if (ForceFallTime > 0.0f)
        {
            ForceFallTime -= delta;
        }

        // Check for shooting
        if (LastBullet > Mods.ShootDebounce)
        {
            if (Input.IsActionPressed($"hit_red_{PlayerIndex}"))
            {
                FireBullet(EnemyColour.Red);
            }
            else if (Input.IsActionPressed($"hit_yellow_{PlayerIndex}"))
            {
                FireBullet(EnemyColour.Yellow);
            }
            else if (Input.IsActionPressed($"hit_blue_{PlayerIndex}"))
            {
                FireBullet(EnemyColour.Blue);
            }
        } 
        else
        {
            LastBullet += delta;
        }

        // Check for retry shooting
        if (ShouldRefireBullet)
        {
            FireBullet(LastBulletColour);
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

            if (IsRoundComplete)
            {
                desiredCameraOffset.y = (Global.NumberOfPlayers == 2) ? +1.0f : -0.69f; // arbitrary-ish
            }

            LerpedCameraOffset.x = 0.0f;// (Global.NumberOfPlayers == 1) ? 0.0f : Mathf.Lerp(LerpedCameraOffset.x, desiredCameraOffset.x * 70.0f, 0.1f);
            LerpedCameraOffset.y = Mathf.Lerp(LerpedCameraOffset.y, desiredCameraOffset.y * 70.0f, 0.1f);
        } 

        if (UpdateCamera && (!IsDead || LastDied > 1.0f))
        {
            float rootCameraOffsetY = Mathf.Clamp(Position.y * 0.2f, -40.0f, 0.0f);
            Camera.Offset = new Vector2(LerpedCameraOffset.x, rootCameraOffsetY + LerpedCameraOffset.y) + CameraShakeOffset;                
            Camera.GlobalPosition = GlobalPosition;
        }
    }

    private void FireBullet(EnemyColour colour)
    {
        if (IsRoundComplete)
        {
            // Can't shoot at goal area!
            return;
        }

        ShouldRefireBullet = false;
        LastBulletColour = colour;
        LastBullet = 0.0f;

        Global.ShootBehaviours[PlayerIndex]?.Shoot(this, colour);     
    }

    private async void Jump()
    {
        if (IsRoundComplete)
        {
            return;
        }

        // Set state
        LastJump = 0.0f;

        // Play animation
        AnimationPlayer.Play("Jump");
        
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        // Set passthrough
        IsPassingThrough = true;
        SetCollisionMaskBit(6, false);

        // Add impulse
        Velocity.y = -Mods.JumpImpulse;

        // Play sound
        Sound_Jump.Play();

        // Cancel firing
        ShouldRefireBullet = false;

        // Spawn particles
        Scene<Particles2D> jumpParticlesScene = R.Particles.JumpParticles;

        Particles2D jumpParticles = jumpParticlesScene.Instance();
        jumpParticles.Position = Position + new Vector2(0.0f, 16.0f);
        GetParent().AddChild(jumpParticles);    
    }

    private void DropDown()
    {
        // Set state
        LastJump = 0.0f;

        IsPassingThrough = true;
        SetCollisionMaskBit(6, false);

        // Add impulse
        Velocity.y = +5.0f;

        // Play animation
        AnimationPlayer.Play("Jump");    

        // Play sound
        Sound_Drop.Play();

        IsInsideWall = true;
        ForceFallTime = 0.2f;
    }
    #endregion

    #region Physics
    public void SetVelocityY(float newVelocityY)
    {
        if (newVelocityY < 0.0f)
        {
            // Sorta simulate jump
            LastJump = 0.0f;

            IsPassingThrough = true;
            SetCollisionMaskBit(6, false);
        }
        
        Velocity.y = newVelocityY;
    }
    private void ApplyPhysics(float delta)    
    {
        if (IsDead)
        {
            Velocity.y += Mods.Gravity;
            Velocity.x = 0.0f;

            Velocity = MoveAndSlide(Velocity, upDirection: Vector2.Up);
            return;
        }

        Velocity.y += Mods.Gravity;
        Velocity.x *= (1.0f - Mods.Friction);

        Velocity = MoveAndSlide(Velocity, upDirection: Vector2.Up);

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
    #endregion

    #region Publically Exposed
    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        Sound_Death.Play();
        
        // Queue pixel creation, tell game we died
        Transform2D deathTransform = new Transform2D
        {
            origin = Vector2.Zero,
            x = Sprite.GlobalTransform.x,
            y = Sprite.GlobalTransform.y,
        };
        CallDeferred(nameof(BurstIntoPixels), Position, deathTransform);
        Game.Instance.PlayerDied(PlayerIndex);
       
        // Zoom in 
        Vector2 zoomIn = new Vector2(1.0f / 2.5f, 1.0f / 2.5f);

        Tween zoomTween = new Tween();
        zoomTween.InterpolateMethod(Engine.Singleton, "set_time_scale", 1.0f, 0.35f, 0.1f, Tween.TransitionType.Circ, Tween.EaseType.Out);
        zoomTween.InterpolateProperty(Camera, "zoom", Vector2.One, zoomIn, 0.3f, Tween.TransitionType.Circ, Tween.EaseType.Out);

        zoomTween.InterpolateProperty(Camera, "zoom", zoomIn, Vector2.One, 0.3f, Tween.TransitionType.Circ, Tween.EaseType.In, delay: 1.5f);
        zoomTween.InterpolateMethod(Engine.Singleton, "set_time_scale", 0.1f, 1.0f, 0.01f, Tween.TransitionType.Circ, Tween.EaseType.In, delay: 1.5f);
        zoomTween.FireAndForget(this);

        // MASSIVE camera shake
        //ShakeCamera(new Vector2(250.0f, 250.0f));
    }

    private void BurstIntoPixels(Vector2 position, Transform2D transform)
    {
        // Delayed sprite bursting. Because Die() is called from enemy movement, we have to defer this.
        Sprite.BurstIntoPixels(body: this, overridePosition: position, overrideTransform: transform, suck: false);
    }

    public void MarkLevelComplete()
    {
        IsRoundComplete = true;
    }

    public void MarkBulletFailed()
    {
        ShouldRefireBullet = true;
    }

    public void ShakeCamera(Vector2 magnitude) => CameraShakeMagnitude += magnitude;
    #endregion

    #region Modifiers/Behaviours
    private Modifiables GetModifiables()
    {
        if (_CachedModifiables != null) return _CachedModifiables;

        Modifiables mods = new Modifiables();
        foreach (IBehaviourModifier modifier in Global.BehaviourModifiers[PlayerIndex])
        {
            modifier.Modify(mods);
        }

        _CachedModifiables = mods;

        ApplyModifiers();
        return _CachedModifiables;
    }

    private void ApplyModifiers()
    {
        Scale = new Vector2(Mods.CharacterScale, Mods.CharacterScale);
    }

    public void AddModifier(IBehaviourModifier mod)
    {
        _CachedModifiables = null;
        Global.BehaviourModifiers[PlayerIndex].Add(mod);
    }
    public void AddModifier<T>() where T : IBehaviourModifier, new() => AddModifier(new T());
    #endregion

    #region Misc
    public Room GetCurrentRoom()
    {
        foreach (Node gameChild in Game.Instance.GetChildren())
        {
            if (gameChild is Room room)
            {
                Rect2 roomRect = new Rect2(room.GlobalPosition, new Vector2(Const.SCREEN_WIDTH, Const.SCREEN_HEIGHT));
                if (roomRect.HasPoint(GlobalPosition))
                {
                    return room;
                }
            }
        }

        return null;
    }
    private void UpdatePlayerIndex()
    {
        // Set collision layer based on player index
        CollisionLayer = (uint)PlayerIndex+1;
        InsideWallDetector.CollisionLayer = (uint)PlayerIndex+1;

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
    #endregion
}

