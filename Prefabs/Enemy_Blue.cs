using Godot;
using System;

public class Enemy_Blue : Enemy
{
    // Consts
    private const float CHANGE_TIME = 2.0f;

    // State
    enum StateEnum
    {
        FollowPlayer,
        GoLeft,
        FollowPlayerAgain,
        GoRight,
        
        MAX,
    }
    private StateEnum State = StateEnum.FollowPlayer;
    private float Timer = 0.0f;

    protected override Color GetColour() => Colors.Blue;

    public override void _Process(float delta)
    {
        base._Process(delta);

        RotationDegrees += Velocity.x / 10.0f;

        Timer += delta;
        if (Timer >= CHANGE_TIME)
        {
            Timer = 0.0f;

            State = (StateEnum)(((int)State + 1) % (int)StateEnum.MAX);
        }
    }

    protected override Vector2 Move(Vector2 playerPosition)
    {
        Vector2 move = new Vector2(); 
        
        switch (State)
        {
            case StateEnum.FollowPlayer:
            case StateEnum.FollowPlayerAgain:
                move.x = Mathf.Sign(playerPosition.x - Position.x) * 10.0f;
                break;

            case StateEnum.GoLeft:
                move.x = -10.0f;
                break;
            case StateEnum.GoRight:
                move.x = +10.0f;
                break;
        }    

        return move;
    }
}
