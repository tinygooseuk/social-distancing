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
    private Node2D GameArea;
    public Label TemplateLabel;
    public Label ScoreLabel;
    public Button AgainButton;

    public Array<Character> Players = new Array<Character>();

    // Enums
    enum Difficulty { Easy, Medium, Hard };

    // State
    public int CurrentLevel { get; private set; } = 0;
    
    public int BaseScore = 0;
    public int KillScore = 0;
    public int TotalScore => BaseScore + KillScore;

    public override void _Ready()
    {
        Instance = this;
        GD.Seed(OS.GetSystemTimeMsecs());
        GD.Randomize();

        // Find nodes
        Viewport mainViewport = RootViewport;

        GameArea = mainViewport.GetNode<Node2D>("GameContainer/Game");
        {
            TemplateLabel = GameArea.GetNode<Label>("TemplateLabel");
        }

        CanvasLayer uiLayer = GetUICanvasLayer();
        {
            ScoreLabel = uiLayer.GetNode<Label>("BG/Score");
            AgainButton = uiLayer.GetNode<Button>("BG/AgainButton");        
        }

        // Generate world
        bool areViewportsScaledDown = GetPlayerCamera(0).Zoom.x > 1.0f;

        for (int level = 0; level < MaxLevels; level++)
        {
            Difficulty desiredDifficulty = GetDifficultyEnumValue(level);

            Node2D room = InstanceRandomRoom(desiredDifficulty); 
            room.Position = new Vector2(-208.0f, level * -240.0f);
            GameArea.AddChild(room);

            float labelScale = areViewportsScaledDown ? 1.75f : 1.0f;

            Label roomLabel = (Label)TemplateLabel.Duplicate();
            roomLabel.RectPosition = new Vector2(-208.0f + 20.0f, level * -240.0f);
            roomLabel.Text = $"Level {level+1}";
            roomLabel.Visible = true;
            roomLabel.RectScale =  new Vector2(labelScale, labelScale);
            GameArea.AddChild(roomLabel);
        }

        // Create players
        Scene<Character> characterScene = R.Prefabs.Character;
        const float PLAYER_WIDTH = 20.0f;
        float totalPlayerWidth = PLAYER_WIDTH * Global.NumberOfPlayers;

        for (int playerIndex = 0; playerIndex < Global.NumberOfPlayers; playerIndex++)
        {   
            Character player = characterScene.Instance();
            player.PlayerIndex = playerIndex;
            player.Position = new Vector2
            {
                x = (-totalPlayerWidth / 2.0f) + playerIndex * PLAYER_WIDTH,
                y = 208.0f
            };

            GameArea.AddChild(player);
            Players.Add(player);
        }        
    }

    public override void _Process(float delta)
    {
        // Check current level
        int newLevel = -1;

        for (int playerIndex = 0; playerIndex < Global.NumberOfPlayers; playerIndex++)
        {
            if (IsInstanceValid(Players[playerIndex]))
            {
                newLevel = Mathf.FloorToInt(1.0f + Players[playerIndex].Position.y / -240.0f);
            }        
        }

        if (newLevel > 0)
        {
            CurrentLevel = newLevel;
        }

        // Update score label
        ScoreLabel.Text = $"Score: {TotalScore}";
    }

    public Viewport RootViewport => GetPlayerViewport(0);
    public Viewport GetPlayerViewport(int playerIndex) => GetNode<Viewport>($"/root/RootControl/Viewports/Player{playerIndex+1}_ViewportContainer/Player{playerIndex+1}_Viewport");
    public Camera2D GetPlayerCamera(int playerIndex) => (Camera2D)GetPlayerViewport(playerIndex).GetChild(0);

    public CanvasLayer GetUICanvasLayer() => GetNode<CanvasLayer>("/root/RootControl/UI");

    public void PlayerDied(int playerIndex)
    {
        //TODO: not in multiplayer?
        Players[playerIndex].Position = new Vector2(-208.0f + 40.0f, 480.0f);
        Players[playerIndex].RotationDegrees = 90.0f;
        
        AgainButton.Visible = true;
    }

    public async void RestartGame()
    {
        Scene<Node> sceneToLoad = null;

        switch (Global.NumberOfPlayers)
        {
            case 1: sceneToLoad = R.Scenes.SinglePlayer; break;
            case 2: sceneToLoad = R.Scenes.TwoPlayer; break;
            default:
                GD.PrintErr($"Unsupported # of players - {Global.NumberOfPlayers}");
                break;
        }

        if (sceneToLoad != null)
        {
            GetTree().ChangeSceneTo(await sceneToLoad.LoadAsync());
        }
    }

    private Difficulty GetDifficultyEnumValue(int level)
    {
        if (level < 5)
        {
            return Difficulty.Easy;
        } 
        else if (level < 10)
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

