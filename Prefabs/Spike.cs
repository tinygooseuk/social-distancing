using Godot;
using System;

public class Spike : Area2D
{    
    private void OnBodyEntered(Node2D body)
    {
        switch (body)
        {
            case Enemy enemy:
                enemy.Die();
                break;
            case Character character:
                character.Die();
                break;
        }
    }    
}
