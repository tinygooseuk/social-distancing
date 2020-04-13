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

    //TODO: try subnodes again?
    private Node2D GameArea;
    public Label TemplateLabel;
    public Label ScoreLabel;
    public Button AgainButton;
    public TitleCard TitleCard;
    [Subnode] public InputMethodManager InputMethodManager { get; private set; }

    private Godot.Collections.Array Players = new Godot.Collections.Array();

    // Enums
    public enum Difficulty { Easy, Medium, Hard, Neutral };

    // State
    public int CurrentLevel { get; private set; } = 0;
    
    public int BaseScore = 0;
    public int KillScore = 0;
    public int TotalScore => BaseScore + KillScore;

    public override async void _Ready()
    {
        this.FindSubnodes();

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
            TitleCard = uiLayer.GetNode<TitleCard>("TitleCard");    
        }

        // Generate world
        bool areViewportsScaledDown = GetPlayerCamera(0).Zoom.x > 1.0f;
        bool isFirstRound = Global.RoundNumber == 0;
        int maxLevels = isFirstRound ? 1 : MaxLevels;

        for (int level = 0; level < maxLevels; level++)
        {
            Difficulty desiredDifficulty = GetDifficultyEnumValue(level);
            if (isFirstRound)
            {
                desiredDifficulty = Difficulty.Neutral;
            }

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
            room.Position = new Vector2(-Const.SCREEN_HALF_WIDTH, maxLevels * -Const.SCREEN_HEIGHT);
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
            Camera2D camera = GetPlayerCamera(playerIndex);

            int offset = (Global.NumberOfPlayers == 2) ? -28 : 88;
            camera.LimitTop = (int)((maxLevels) * -Const.SCREEN_HEIGHT) + offset; // 88 will arbitrarily make it limit properly. cool.
            
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
                newLevel = Mathf.FloorToInt(1.0f + c.Position.y / -Const.SCREEN_HEIGHT);
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

    public CanvasLayer GetUICanvasLayer() => GetNode<CanvasLayer>("/root/RootControl/UI");

    public Character GetPlayer(int playerIndex) => (Players.Count > playerIndex) ? Players[playerIndex] as Character : null;
    public Character GetNearestPlayer(Vector2 globalPosition) 
    {
        float nearestSqrDistance = 1000000000.0f;
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

    public async void PlayerDied(int playerIndex)
    {
        //TODO: not in multiplayer?
        Character c = GetPlayer(playerIndex);
        if (IsInstanceValid(c))
        {
            c.Position = new Vector2(-Const.SCREEN_HALF_WIDTH + 40.0f, 480.0f);
            c.RotationDegrees = 90.0f;
        }
        
        // Wait 2s
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

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

    private async Task<Node2D> InstanceRandomRoomAsync(Difficulty d)
    {
        string[] sceneArray = null;

        switch (d)
        {
            case Difficulty.Easy: sceneArray = R.Rooms.EasyRooms; break;
            case Difficulty.Medium: sceneArray = R.Rooms.MediumRooms; break;
            case Difficulty.Hard: sceneArray = R.Rooms.HardRooms; break;            
            case Difficulty.Neutral: sceneArray = R.Rooms.NeutralRooms; break;       
        }

        Scene<Node2D> roomScene = sceneArray[(int)(GD.Randi() % sceneArray.Length)];
        return await roomScene.InstanceAsync();
    }
}

