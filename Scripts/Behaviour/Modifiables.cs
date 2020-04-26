
public class Modifiables
{
    public float Gravity;
    public float MoveSpeed;

    public int NumAirJumps;
    
    public float JumpImpulse;
    public float JumpDebounce;

    public float ShootDebounce;
    
    public float Friction;
    
    public float CharacterScale;

    public Modifiables()
    {
        Reset();
    }

    public void Reset()
    {
        Gravity = 9.8f;
        MoveSpeed = 40f;

        NumAirJumps = 1;
    
        JumpImpulse = 300f;
        JumpDebounce = 0.4f;

        ShootDebounce = 0.2f;
    
        Friction = 0.15f;
    
        CharacterScale = 1f;
    }
}