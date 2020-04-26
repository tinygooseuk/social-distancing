using System;
using System.Collections;
using System.Linq;
using Godot;

public class Room : Node2D
{
    [Subnode("Passthrough")] private TileMap PassthroughTileMap;
    [Subnode("Permanent")] private TileMap PermanentTileMap;

    [Subnode("EnemySpawners")] private Node2D _EnemySpawners;
    public IEnumerable EnemySpawners => _EnemySpawners.GetChildren().Cast<EnemySpawner>().ToArray();
    
    public override void _Ready()
    {
        base._Ready();
        
        this.FindSubnodes(); 
    }

    public int TileHeight
    {
        get
        {
            if (!IsInstanceValid(PermanentTileMap)) this.FindSubnodes();
            if (!IsInstanceValid(PermanentTileMap)) return (int)(Const.SCREEN_HEIGHT / 16f);
            
            int height = 0;

            while (PermanentTileMap.GetCell(0, height) != -1)
                height++;
            
            return height;
        }
    }

    public float PixelHeight => TileHeight * 16f;
}