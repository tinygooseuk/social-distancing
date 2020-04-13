using Godot;
using System;

public class Spike : Area2D
{    
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.Die();
        }

        if (body is Character character)
        {
            character.Die();
        }
    }    
}
