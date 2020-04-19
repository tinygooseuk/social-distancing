using System;
using Godot;

public interface IShootBehaviour
{
    void Shoot(Character shooter, EnemyColour colour);    
}

public class DefaultShootBehaviour : IShootBehaviour
{
    public void Shoot(Character shooter, EnemyColour colour)
    {
        Scene<Bullet> bulletScene = R.Prefabs.BULLET;

        Bullet bullet = bulletScene.Instance();
        bullet.FiredByPlayerIndex = shooter.PlayerIndex;
        bullet.Colour = colour;
        bullet.Direction = new Vector2(shooter.IsFacingRight ? 1f : -1f, 0f);
        bullet.Position = shooter.Position + bullet.Direction * 20f + new Vector2(0, -4f);

        shooter.GetParent().AddChild(bullet);
    }
}