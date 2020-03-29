using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public class Game : Node2D
{
    // Ref
    public static Game Instance = null;

    // Exports
    [Export] private Curve DifficultyCurve;

    [Export] private int MaxLevels = 15;
    

    // Subnodes
    private Node2D GameArea;
    public Label TemplateLabel;
    public Label ScoreLabel;
    public Button AgainButton;

    private Array<Character> Players = new Array<Character>();

    // Enums
    enum Difficulty { Easy, Medium, Hard };

    // State
    public int CurrentLevel { get; private set; } = 0;
    
    public int BaseScore = 0;
    public int KillScore = 0;
    public int TotalScore => BaseScore + KillScore;

    public override async void _Ready()
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

            Node2D room = await InstanceRandomRoomAsync(desiredDifficulty); 
            room.Position = new Vector2(-Const.SCREEN_HALF_WIDTH, level * -Const.SCREEN_HEIGHT);
            GameArea.AddChild(room);

            float labelScale = areViewportsScaledDown ? 1.75f : 1.0f;

            Label roomLabel = (Label)TemplateLabel.Duplicate();
            roomLabel.RectPosition = new Vector2(-Const.SCREEN_HALF_WIDTH + 20.0f, level * -Const.SCREEN_HEIGHT);
            roomLabel.Text = $"Level {level+1}";
            roomLabel.Visible = true;
            roomLabel.RectScale =  new Vector2(labelScale, labelScale);
            GameArea.AddChild(roomLabel);
        }

        // Create goal room
        {
            Scene<Node2D> goalRoomScene = R.Rooms.GoalRoom;

            Node2D room = await goalRoomScene.InstanceAsync();
            room.Position = new Vector2(-Const.SCREEN_HALF_WIDTH, MaxLevels * -Const.SCREEN_HEIGHT);
            GameArea.AddChild(room);
        }
         
        // Create players
        Scene<Character> characterScene = R.Prefabs.Character;
        const float PLAYER_WIDTH = 20.0f;
        float totalPlayerWidth = PLAYER_WIDTH * Global.NumberOfPlayers;

        for (int playerIndex = 0; playerIndex < Global.NumberOfPlayers; playerIndex++)
        {   
            // Create player and set up
            Character player = characterScene.Instance();
            player.PlayerIndex = playerIndex;
            player.Position = new Vector2
            {
                x = (-totalPlayerWidth / 2.0f) + playerIndex * PLAYER_WIDTH,
                y = Const.SCREEN_HALF_WIDTH
            };

            // Limit player camera to top of world
            int offset = (Global.NumberOfPlayers == 2) ? -28 : 88;

            Camera2D camera = GetPlayerCamera(playerIndex);
            camera.LimitTop = (int)((MaxLevels) * -Const.SCREEN_HEIGHT) + offset; // 88 will arbitrarily make it limit properly. cool.

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
            if (Players.Count > playerIndex && IsInstanceValid(Players[playerIndex]))
            {
                newLevel = Mathf.FloorToInt(1.0f + Players[playerIndex].Position.y / -Const.SCREEN_HEIGHT);
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

    public Character GetPlayer(int playerIndex) => (Players.Count > playerIndex) ? Players[playerIndex] : null;
    public Character GetNearestPlayer(Vector2 globalPosition) 
    {
        float nearestSqrDistance = 1000000000.0f;
        Character nearestPlayer = null;

        for (int playerIndex = 0; playerIndex < Global.NumberOfPlayers; playerIndex++)
        {
            Character thisPlayer = GetPlayer(playerIndex);
            if (!IsInstanceValid(thisPlayer)) continue;

            float thisSqrDistance = thisPlayer.GlobalPosition.DistanceSquaredTo(GlobalPosition);
            if (thisSqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = thisSqrDistance;
                nearestPlayer = thisPlayer;
            }
        }

        return nearestPlayer;

    }

    public void PlayerDied(int playerIndex)
    {
        //TODO: not in multiplayer?
        Players[playerIndex].Position = new Vector2(-Const.SCREEN_HALF_WIDTH + 40.0f, 480.0f);
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

    private async Task<Node2D> InstanceRandomRoomAsync(Difficulty d)
    {
        string[] sceneArray = null;

        switch (d)
        {
            case Difficulty.Easy: sceneArray = R.Rooms.EasyRooms; break;
            case Difficulty.Medium: sceneArray = R.Rooms.MediumRooms; break;
            case Difficulty.Hard: sceneArray = R.Rooms.HardRooms; break;            
        }

        Scene<Node2D> roomScene = sceneArray[(int)(GD.Randi() % sceneArray.Length)];
        return await roomScene.InstanceAsync();
    }
}

