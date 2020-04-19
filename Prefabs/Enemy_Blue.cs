using Godot;
using System;

public class Enemy_Blue : Enemy
{
    // Consts
    private const float CHANGE_TIME = 2f;

    // State
    enum StateEnum
    {
        FollowPlayer,
        GoLeft,
        FollowPlayerAgain,
        GoRight,
    }
    private StateEnum State = StateEnum.FollowPlayer;
    private float Timer = 0f;

    protected override EnemyColour Colour => EnemyColour.Blue;

    public override void _Process(float delta)
    {
        base._Process(delta);

        RotationDegrees += Velocity.x / 10f;

        Timer += delta;
        if (Timer >= CHANGE_TIME)
        {
            Timer = 0f;

            State = (StateEnum)(((int)State + 1) % (int)EnumUtil.GetCount<StateEnum>());
        }
    }

    protected override Vector2 Move(Vector2 playerPosition, float difficultyScale)
    {
        switch (State)
        {
            case StateEnum.FollowPlayer:
            case StateEnum.FollowPlayerAgain:
                return new Vector2(Mathf.Sign(playerPosition.x - Position.x) * 10f * difficultyScale, 0f);

            case StateEnum.GoLeft:
                return new Vector2(-10f * difficultyScale, 0f);
            
            case StateEnum.GoRight:
                return new Vector2(+10f * difficultyScale, 0f);

            default:
                return Vector2.Zero;
        }    
    }
}
