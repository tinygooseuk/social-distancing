using Godot;
using Godot.Collections;
using System;

public class Game : Node2D
{
    // Ref
    public static Game Instance = null;

    // Exports
    [Export] private Array<PackedScene> EasyScenes = new Array<PackedScene>();
    [Export] private Array<PackedScene> MediumScenes = new Array<PackedScene>();
    [Export] private Array<PackedScene> HardScenes = new Array<PackedScene>();

    [Export] private Curve DifficultyCurve;

    [Export] private int MaxLevels = 30;
    

    // Subnodes
    [Subnode] private Node2D GameArea;
    [Subnode("GameArea/Player1")] public Character Player1;
    [Subnode("GameArea/TemplateLabel")] public Label TemplateLabel;
    [Subnode("UI/BG/Score")] public Label Score;
    [Subnode("UI/BG/AgainButton")] public Button AgainButton;

    // Enums
    enum Difficulty { Easy, Medium, Hard };

    // State
    public int CurrentLevel { get; private set; } = 0;

    public override void _Ready()
    {
        Instance = this;
        GD.Seed(OS.GetSystemTimeMsecs());

        this.FindSubnodes();

        // Generate world
        for (int level = 0; level < MaxLevels; level++)
        {
            Difficulty desiredDifficulty = GetDifficultyEnumValue(level);

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

    public override void _Process(float delta)
    {
        if (IsInstanceValid(Player1))
        {
            int newLevel = Mathf.FloorToInt(1.0f + Player1.Position.y / -240.0f);
            if (newLevel > 0)
            {
                CurrentLevel = newLevel;
            }

            AgainButton.Visible = true;
            AgainButton.Text = $"AI: {GetAIDifficultyScale(CurrentLevel)}";
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

    private Difficulty GetDifficultyEnumValue(int level)
    {
        if (level < 10)
        {
            return Difficulty.Easy;
        } 
        else if (level < 20)
        {
            return Difficulty.Medium;
        }
        else
        {
            return Difficulty.Hard;
        }
    }
    public float GetAIDifficultyScale(int level)
    {
        float progression = (float)level / MaxLevels;
        
        return DifficultyCurve.Interpolate(progression);
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

