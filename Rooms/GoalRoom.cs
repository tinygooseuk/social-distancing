using Godot;
using Godot.Collections;
using System;

public class GoalRoom : Node2D
{
    // Subnodes
    [Subnode] private Area2D GoalDetector;

    // State
    private bool WasTriggered = false;

    public override void _Ready()
    {
        this.FindSubnodes();
    }

    private void OnGoalReached(Node2D triggerer)
    {   
        if (WasTriggered) return;

        if (triggerer is Character c)
        {
            WasTriggered = true;

            // Teleport all chars in
            Array<Character> characters = new Array<Character>(GetTree().GetNodesInGroup(Groups.PLAYERS));
            foreach (Character character in characters)
            {
                character.GlobalPosition = new Vector2(character.GlobalPosition.x, GoalDetector.GlobalPosition.y);
                character.MarkLevelComplete();
            }
        }
    }
}