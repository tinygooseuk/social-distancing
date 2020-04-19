using Godot;
using Godot.Collections;
using System;

public class EnemySpawner : Position2D
{
    [Export] private readonly Array<PackedScene> EnemyScenes = new Array<PackedScene>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //CallDeferred("Spawn");
    }

    private void OnBodyEntered(Node other)
    {
        if (other is Character)
        {
            Spawn();
        }
    }
    
    private void Spawn()
    {
        PackedScene scene = EnemyScenes[(int)(GD.Randi() % EnemyScenes.Count)];

        var enemy = (Enemy)scene.Instance();
        enemy.Position = Position;
        GetParent().AddChild(enemy);
        
        QueueFree();
    }
}
