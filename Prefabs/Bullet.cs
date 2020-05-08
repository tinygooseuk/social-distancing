using Godot;
using System;
using System.Threading.Tasks;
using System.Linq;

public class Bullet : KinematicBody2D
{
	// Subnodes
	[Subnode] private AnimatedSprite AnimatedSprite;
	[Subnode] private Tween IntroTween;
	
	// Consts
	private const float SPEED = 400f;

	// State
	public int FiredByPlayerIndex = 0;
	public Vector2 Direction = Vector2.Zero;
	public EnemyColour Colour = EnemyColour.Red;
	
	public bool DisableRetry = false;
	public bool IsSilent = false;
	public bool IsHoming = false;

	// Private state
	private bool FirstFrame = true;
	private Enemy HomingTarget = null;
	
	public override void _Ready()
	{
		this.FindSubnodes();

		// Set colour
		Modulate = Colour.ToColor();

		// Tween up the size
		IntroTween.InterpolateProperty(AnimatedSprite, "scale", null, new Vector2(2f, 2f), 0.1f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
		IntroTween.Start();

		// Don't collide with current char
		SetCollisionMaskBit(FiredByPlayerIndex, false);

		// Show on next frame
		AnimatedSprite.CallDeferred("set_visible", true);

		// Set state/abilities
		Character shotBy = Game.Instance.GetPlayer(FiredByPlayerIndex);
		if (IsInstanceValid(shotBy))
		{
			IsHoming = shotBy.Mods.BulletsAreHoming;
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		KinematicCollision2D collision = MoveAndCollide(Direction * delta * SPEED);
		if (collision != null && IsInstanceValid(collision.Collider))
		{
			bool dontExplode = false;
			
			if (FirstFrame && !DisableRetry)
			{
				Character firedByPlayer = Game.Instance.GetPlayer(FiredByPlayerIndex);
				if (IsInstanceValid(firedByPlayer))
				{
					firedByPlayer.MarkBulletFailed();
					dontExplode = true;
				}
			}
			
			switch (Colour)
			{
				case EnemyColour.Red when collision.Collider is Enemy_Red er:
					dontExplode = true;
					KillEnemy(er);
					break;
				case EnemyColour.Yellow when collision.Collider is Enemy_Yellow ey:
					dontExplode = true;
					KillEnemy(ey);
					break;
				case EnemyColour.Blue when collision.Collider is Enemy_Blue eb:
					dontExplode = true;
					KillEnemy(eb);
					break;
				default:
					QueueFree();
					break;
			}

			if (!dontExplode)
			{
				// Splat!
				foreach (var pixel in AnimatedSprite.BurstIntoPixels(this, false, lifetimeMultiplier: 0.25f))
				{
					const float MAX_FORCE = 50f;
					pixel.ApplyCentralImpulse(new Vector2((float)GD.RandRange(-MAX_FORCE, +MAX_FORCE), (float)GD.RandRange(-MAX_FORCE, +MAX_FORCE)));   
				}
			}

			return;
		}

		if (FirstFrame && !IsSilent)
		{
			// Play shot sound        
			Asset<AudioStream> Sound_Shoot = R.Sounds.SHOOT;
			GetTree().PlaySound2D(Sound_Shoot, relativeTo: this);
		}

		if (IsHoming)
		{
			if (FirstFrame) 
			{
				HomingTarget = FindNearestEnemy();
			}

			if (IsInstanceValid(HomingTarget))
			{
				Vector2 targetDirection = (HomingTarget.GlobalPosition - GlobalPosition).Normalized();
				Direction = Direction.LinearInterpolate(targetDirection, 0.5f);
			}
		}

		FirstFrame = false;
	}

	private Enemy FindNearestEnemy()
	{
		Room enemyRoom = this.GetCurrentRoom();
		var enemiesInRoom = GetTree().GetNodesInGroup(Enemy.ENEMY_GROUP_NAME).OfType<Enemy>().Where(enemy => enemy.GetCurrentRoom() == enemyRoom);

		Enemy nearestEnemy = null;
		float minDistanceSqr = float.MaxValue;
		foreach (Enemy enemy in enemiesInRoom)
		{
			float thisDistanceSqr = enemy.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			if (thisDistanceSqr < minDistanceSqr)
			{
				minDistanceSqr = thisDistanceSqr;
				nearestEnemy = enemy;
			}
		}

		return nearestEnemy;
	}

	private void KillEnemy(Enemy e)
	{
		e.Die();

		Game.Instance.KillScore += (int)(500f + (float)Game.Instance.CurrentLevel / 10f);
		
		// Play enemy death sound
		Asset<AudioStream> Sound_EnemyDeath = R.Sounds.ENEMY_DEATH;
		GetTree().PlaySound2D(Sound_EnemyDeath, relativeTo: this);

		// Shake correct camera
		Character c = Game.Instance.GetPlayer(FiredByPlayerIndex);
		if (IsInstanceValid(c))
		{
			Vector2 randomShake = new Vector2((float)GD.RandRange(-8f, +8f), (float)GD.RandRange(-8f, +8f));
			c.ShakeCamera(Direction * 8f + randomShake);
		}
	}
}
