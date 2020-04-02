using System;
using Godot;

public interface IShootBehaviour
{
    void Shoot(Character shooter, Bullet.ColourEnum colour);    
}

public class DefaultShootBehaviour : IShootBehaviour
{
    public void Shoot(Character shooter, Bullet.ColourEnum colour)
    {
        Scene<Bullet> bulletScene = R.Prefabs.Bullet;

        Bullet bullet = bulletScene.Instance();
        bullet.FiredByPlayerIndex = shooter.PlayerIndex;
        bullet.Colour = colour;
        bullet.Direction = new Vector2(shooter.IsFacingRight ? 1.0f : -1.0f, 0.0f);
        bullet.Position = shooter.Position + bullet.Direction * 20.0f + new Vector2(0, -4.0f);

        shooter.GetParent().AddChild(bullet);
    }
}