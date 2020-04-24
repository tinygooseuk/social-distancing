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
    [Export] private int MaxLevels = 9;
   
    // Subnodes
    [Subnode] private AudioStreamPlayer BGM;
    [Subnode] private AudioStreamPlayer RoundComplete;

    //TODO: try subnodes again?
    private Node2D GameArea;
    private Label TemplateLabel;
    private Label ScoreLabel;
    private Button AgainButton;
    public TitleCard TitleCard;
    [Subnode] public InputMethodManager InputMethodManager { get; private set; }

    private readonly Godot.Collections.Array Players = new Godot.Collections.Array();

    // Enums
    public enum Difficulty { Easy, Medium, Hard, Neutral };

    // State
    public int CurrentLevel { get; private set; } = 0;
    
    public int BaseScore = 0;
    public int KillScore = 0;
    public int TotalScore => BaseScore + KillScore;

    public override async void _Ready()
    {
        base._Ready();
        
        this.FindSubnodes();

        // Seed
        Engine.TimeScale = 1f;
        Instance = this;

        // Find nodes
        Viewport mainViewport = RootViewport;

        GameArea = mainViewport.GetNode<Node2D>("GameContainer/Game");
        {
            TemplateLabel = GameArea.GetNode<Label>("TemplateLabel");
        }

        CanvasLayer uiLayer = UICanvasLayer;
        {
            ScoreLabel = uiLayer.GetNode<Label>("BG/Score");
            AgainButton = uiLayer.GetNode<Button>("BG/AgainButton");    
            TitleCard = uiLayer.GetNode<TitleCard>("TitleCard");    
        }

        // Generate world
        bool areViewportsScaledDown = GetPlayerCamera(0).Zoom.x > 1f;
        bool isFirstRound = Global.RoundNumber == 0;
        int maxLevels = isFirstRound ? 1 : MaxLevels;

        float caret = 0f;
        
        for (int level = 0; level < maxLevels; level++)
        {
            Difficulty desiredDifficulty = GetDifficultyEnumValue(level);
            if (isFirstRound)
            {
                desiredDifficulty = Difficulty.Neutral;
            }

            Room room = await InstanceRandomRoomAsync(desiredDifficulty);
                        
            // Move caret up
            caret -= room.PixelHeight;

            // Position room
            room.Position = new Vector2(-Const.SCREEN_HALF_WIDTH, caret);
            GameArea.AddChild(room);

            // Add label
            float labelScale = areViewportsScaledDown ? 1.75f : 1f;

            Label roomLabel = (Label)TemplateLabel.Duplicate();
            roomLabel.RectPosition = new Vector2(-Const.SCREEN_HALF_WIDTH + 20f, caret);
            roomLabel.Text = $"Level {level+1}";
            roomLabel.Visible = true;
            roomLabel.RectScale =  new Vector2(labelScale, labelScale);
            GameArea.AddChild(roomLabel);
        }

        // Create goal room
        {
            Scene<Room> goalRoomScene = R.Rooms.GOAL_ROOM;
            Room room = await goalRoomScene.InstanceAsync();

            // Move caret up
            caret -= room.PixelHeight;
            
            room.Position = new Vector2(-Const.SCREEN_HALF_WIDTH, caret);
            GameArea.AddChild(room);
        }
         
        // Create players
        Scene<Character> characterScene = R.Prefabs.CHARACTER;
        const float PLAYER_WIDTH = 20f;
        float totalPlayerWidth = PLAYER_WIDTH * Global.NumberOfPlayers;

        for (int playerIndex = 0; playerIndex < Global.NumberOfPlayers; playerIndex++)
        {   
            // Create player and set up
            Character player = characterScene.Instance();
            player.PlayerIndex = playerIndex;
            player.Position = new Vector2
            {
                x = (-totalPlayerWidth / 2f) + playerIndex * PLAYER_WIDTH,
                y = -32f
            };

            // Limit player camera to top of world
            Camera2D camera = GetPlayerCamera(playerIndex);

            int offset = (Global.NumberOfPlayers == 2) ? -28 : 88;
            camera.LimitTop = (int)caret + offset; // 88 will arbitrarily make it limit properly. cool.
            
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
            Character c = GetPlayer(playerIndex);
            if (Players.Count > playerIndex && IsInstanceValid(c))
            {
                newLevel = Mathf.FloorToInt(1f + c.Position.y / -Const.SCREEN_HEIGHT);
            }        
        }

        if (newLevel > 0)
        {
            CurrentLevel = newLevel;
        }

        // Update score label
        ScoreLabel.Text = $"Score: {Global.TotalScore + TotalScore:d8}";
    }

    public Viewport RootViewport => GetPlayerViewport(0);
    public Viewport GetPlayerViewport(int playerIndex) => GetNode<Viewport>($"/root/RootControl/Viewports/Player{playerIndex+1}_ViewportContainer/Player{playerIndex+1}_Viewport");
    public Camera2D GetPlayerCamera(int playerIndex) => (Camera2D)GetPlayerViewport(playerIndex).GetChild(0);

    public CanvasLayer UICanvasLayer => GetNode<CanvasLayer>("/root/RootControl/UI");

    public Character GetPlayer(int playerIndex) => (Players.Count > playerIndex) ? Players[playerIndex] as Character : null;
    public Character GetNearestPlayer(Vector2 globalPosition) 
    {
        float nearestSqrDistance = 1000000000f;
        Character nearestPlayer = null;

        for (int playerIndex = 0; playerIndex < Global.NumberOfPlayers; playerIndex++)
        {
            Character thisPlayer = GetPlayer(playerIndex);
            if (!IsInstanceValid(thisPlayer)) continue;

            float thisSqrDistance = thisPlayer.GlobalPosition.DistanceSquaredTo(globalPosition);
            if (thisSqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = thisSqrDistance;
                nearestPlayer = thisPlayer;
            }
        }

        return nearestPlayer;

    }
    
    public void MarkRoundComplete()
    {
        BGM.Stop();
        RoundComplete.Play();
    }

    public async void PlayerDied(int playerIndex)
    {
        //TODO: not in multiplayer?
        Character c = GetPlayer(playerIndex);
        if (IsInstanceValid(c))
        {
            c.Position = new Vector2(-Const.SCREEN_HALF_WIDTH + 40f, 480f);
            c.RotationDegrees = 90f;
        }
        
        // Wait 2s
        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        AgainButton.Visible = true;
        AgainButton.GrabFocus();
    }

    public async void ReloadGameScene()
    {
        Scene<Node> gameScene = R.Scenes.GetGameSceneForNumPlayers(Global.NumberOfPlayers);
        GetTree().ChangeSceneTo(await gameScene.LoadAsync());
    }

    public static Difficulty GetDifficultyEnumValue(int level)
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

    private static async Task<Room> InstanceRandomRoomAsync(Difficulty d)
    {
        string[] sceneArray = null;

        switch (d)
        {
            case Difficulty.Easy: sceneArray = R.Rooms.EASY_ROOMS; break;
            case Difficulty.Medium: sceneArray = R.Rooms.MEDIUM_ROOMS; break;
            case Difficulty.Hard: sceneArray = R.Rooms.HARD_ROOMS; break;            
            case Difficulty.Neutral: sceneArray = R.Rooms.NEUTRAL_ROOMS; break;
            default:
                throw new ArgumentOutOfRangeException(nameof(d), d, null);
        }

        Scene<Room> roomScene = sceneArray[(int)(GD.Randi() % sceneArray.Length)];
        return await roomScene.InstanceAsync();
    }

}

