using Godot;
using Godot.Collections;
using System;

public class Main : Node2D
{
    // Ref
    public static Main Instance = null;

    // Exports
    [Export] private Array<PackedScene> EasyScenes = new Array<PackedScene>();
    [Export] private Array<PackedScene> MediumScenes = new Array<PackedScene>();
    [Export] private Array<PackedScene> HardScenes = new Array<PackedScene>();

    // Subnodes
    [Subnode] private Node2D GameArea;
    [Subnode("GameArea/Player1")] public Character Player1;
    [Subnode("GameArea/TemplateLabel")] public Label TemplateLabel;
    [Subnode("UI/BG/Score")] public Label Score;
    [Subnode("UI/BG/AgainButton")] public Button AgainButton;

    // Enums
    enum Difficulty { Easy, Medium, Hard };

    public override void _Ready()
    {
        Instance = this;

        this.FindSubnodes();

        // Generate world
        for (int level = 0; level < 50; level++)
        {
            Difficulty desiredDifficulty = GetDifficulty(level);

            Node2D room = InstanceRandomRoom(desiredDifficulty); 
            room.Position = new Vector2(0.0f, level * -240.0f);
            GameArea.AddChild(room);

            Label roomLabel = (Label)TemplateLabel.Duplicate();
            roomLabel.RectPosition = new Vector2(20.0f, level * -240.0f);
            roomLabel.Text = $"Level {level+1}";
            roomLabel.Visible = true;
            GameArea.AddChild(roomLabel);
        }
    }

    public void PlayerDied()
    {
        Player1.Position = new Vector2(40.0f, 480.0f);
        Player1.RotationDegrees = 90.0f;
        
        AgainButton.Visible = true;
    }

    public void RestartGame()
    {
        GetTree().ChangeScene("res://Main.tscn");
    }

    private Difficulty GetDifficulty(int level)
    {
        if (level < 10)
        {
            return Difficulty.Easy;
        } 
        else if (level < 30)
        {
            return Difficulty.Medium;
        }
        else
        {
            return Difficulty.Medium;
        }
    }

    private Node2D InstanceRandomRoom(Difficulty d)
    {
        Array<PackedScene> sceneArray = null;

        switch (d)
        {
            case Difficulty.Easy: sceneArray = EasyScenes; break;
            case Difficulty.Medium: sceneArray = MediumScenes; break;
            case Difficulty.Hard: sceneArray = HardScenes; break;            
        }

        PackedScene scene = sceneArray[(int)(GD.Randi() % sceneArray.Count)];
        return (Node2D)scene.Instance();
    }
}

