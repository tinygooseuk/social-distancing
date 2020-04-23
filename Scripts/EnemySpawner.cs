using Godot;
using Godot.Collections;
using System;

public class EnemySpawner : Position2D
{
    [Export] private readonly Array<PackedScene> EnemyScenes = new Array<PackedScene>();

    private void OnBodyEntered(Node other)
    {
        if (other is Character)
        {
            CallDeferred(nameof(Spawn));
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
