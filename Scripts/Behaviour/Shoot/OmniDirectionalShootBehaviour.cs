using System;
using Godot;

public class OmniDirectionalShootBehaviour : IShootBehaviour
{
    public void Shoot(Character shooter, EnemyColour colour)
    {
        Scene<Bullet> bulletScene = R.Prefabs.BULLET;

        for (int d = 0; d < 8; d++)
        {
            Bullet bullet = bulletScene.Instance();
            bullet.DisableRetry = true;
            bullet.FiredByPlayerIndex = shooter.PlayerIndex;
            bullet.Colour = colour;
            bullet.IsSilent = (d > 0); // Only one should make sound
            bullet.Direction = Mathf.Polar2Cartesian(1f, 2f * Mathf.Pi * (d / 8f));
            bullet.Position = shooter.Position + new Vector2(0, -4f);
            shooter.GetParent().AddChild(bullet);        
        }
    }
}


        