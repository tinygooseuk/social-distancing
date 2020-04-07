using System;
using Godot;

public class OmniDirectionalShootBehaviour : IShootBehaviour
{
    public void Shoot(Character shooter, EnemyColour colour)
    {
        Scene<Bullet> bulletScene = R.Prefabs.Bullet;

        for (int d = 0; d < 8; d++)
        {
            Bullet bullet = bulletScene.Instance();
            bullet.DisableRetry = true;
            bullet.FiredByPlayerIndex = shooter.PlayerIndex;
            bullet.Colour = colour;
            bullet.Direction = Mathf.Polar2Cartesian(1.0f, 2.0f * Mathf.Pi * (d / 8.0f));
            bullet.Position = shooter.Position + new Vector2(0, -4.0f);
            shooter.GetParent().AddChild(bullet);        
        }
    }
}


        