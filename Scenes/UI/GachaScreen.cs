using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;
using System.Linq;

// Typedefs
public class PixelMap : System.Collections.Generic.Dictionary<EnemyColour, PixelListsForColour> {}
public class PixelListsForColour : System.Collections.Generic.List<PixelList> {}
public class PixelList : System.Collections.Generic.List<Pixel> {}

public class GachaScreen : Control
{
    // Consts
    private const float REEL_TIMER_MAX = 2.5f;

    // Signals
    [Signal] public delegate void AllReelsSpun();

    // Subnodes
    [Subnode("Reels/GachaReel1")] private GachaReel GachaReel1_Blue;
    [Subnode("Reels/GachaReel2")] private GachaReel GachaReel2_Yellow;
    [Subnode("Reels/GachaReel3")] private GachaReel GachaReel3_Red;
    [Subnode("Reels/GachaReel4")] private GachaReel GachaReel4_Green;
    public GachaReel[] GachaReels => new[] { GachaReel1_Blue, GachaReel2_Yellow, GachaReel3_Red, GachaReel4_Green };

    [Subnode("Labels/MoveLabel")] private Label Label1_Blue;
    [Subnode("Labels/JumpLabel")] private Label Label2_Yellow;
    [Subnode("Labels/AttackLabel")] private Label Label3_Red;
    [Subnode("Labels/WildLabel")] private Label Label4_Green;

    [Subnode] public Label DescriptionLabel { get; private set; }
    
    private Label[] Labels => new[] { Label1_Blue, Label2_Yellow, Label3_Red, Label4_Green };

    // State
    private PixelMap PixelMap = new PixelMap();

    private bool IsStarted = false;
    private bool IsSpinning = false;
    private bool IsAwaitingReelStop = false;
    private float ReelTimer = REEL_TIMER_MAX;
    private int ReelNumber = 0;

    public override void _Ready()
    {
        this.FindSubnodes();

        // Load in gacha data
        Asset<GachaPrizeList> prizeDatabase = R.Model.GACHA_PRIZES;
        Array<GachaPrize> prizes = prizeDatabase.Load().GachaPrizes;

        GachaReel1_Blue.SetPrizes(FilterAndExpandPrizes(prizes, EnemyColour.Blue));  
        GachaReel2_Yellow.SetPrizes(FilterAndExpandPrizes(prizes, EnemyColour.Yellow));
        GachaReel3_Red.SetPrizes(FilterAndExpandPrizes(prizes, EnemyColour.Red));
        GachaReel4_Green.SetPrizes(FilterAndExpandPrizes(prizes, EnemyColour.Green));
    }

    private GachaPrize[] FilterAndExpandPrizes(Array<GachaPrize> allPrizes, EnemyColour colour)
    {
        var outPrizes = new Array<GachaPrize>();

        foreach (GachaPrize prize in allPrizes)
        {
            if (prize.Colour != colour) continue;

            for (int i = 0; i < prize.Weight; i++)
            {
                outPrizes.Add(prize);
            }
        }
        
        return outPrizes.ToArray().Shuffle();
    }
    
    public async void Start()
    {
        IsStarted = true;

        // Spawn pixel piles
        await SpawnPixels();

        if (Global.GetCollectedPixels(EnemyColour.Blue) > 0f)
        {
            // Wait 2 secs to roll blue to allow bumping
            await ToSignal(GetTree().CreateTimer(2f), "timeout");
        }

        // Begin spinning
        IsSpinning = true;
    }

    public override void _Process(float delta)
    {
        //TODO: multiplayer
        Character character = Game.Instance.GetPlayer(0);
        Room goalRoom = character.GetCurrentRoom();

        Camera2D camera = Game.Instance.GetPlayerCamera(0);
         
        if (IsStarted)
        {
            camera.Offset = camera.Offset.LinearInterpolate(new Vector2(0f, -32f), 0.1f);

            camera.GlobalPosition = camera.GlobalPosition.LinearInterpolate(goalRoom.GlobalPosition + new Vector2(Const.SCREEN_HALF_WIDTH, Const.SCREEN_HALF_HEIGHT), 0.1f);
            camera.Zoom = camera.Zoom.LinearInterpolate(Global.NumberOfPlayers == 1 ? Vector2.One : new Vector2(2f, 2f), 0.1f);
        }

        if (!IsSpinning) return;

        // Fast forward if required
        float fastForwardAmount = Input.GetActionStrength("fast_forward") * 4f;
        Engine.TimeScale = 1f + fastForwardAmount;        

        // Allow fast forward effects
        Game.Instance.TitleCard.ShowFastForwardFX = true;
                
        // Check for bumping
        EnemyColour reelColour = (EnemyColour)ReelNumber;
        string[] inputs = { "hit_blue", "hit_yellow", "hit_red", "jump"};
        
        for (int i = 0; i < Global.NumberOfPlayers; i++)
        {
            // Has pixels to spend, and wants to spend
            int availablePixels = Global.GetCollectedPixels(reelColour);
            if (availablePixels > 0 && Input.IsActionJustPressed(inputs[ReelNumber] + "_" + i))
            {
                GachaReel reel = GachaReels[ReelNumber];
                if (reel.RequestBump())
                {
                    // Spend pixels
                    Global.DecrementCollectedPixels(reelColour);

                    // Remove top row of pixels for reel
                    PixelListsForColour pixelLists = PixelMap[reelColour];
                    PixelList pixelList = pixelLists[pixelLists.Count-1];

                    foreach (Pixel pixel in pixelList)
                    {
                        pixel.SuckUpAndOut(-64f);
                    }

                    // Remove row from pixel lists
                    pixelLists.RemoveAt(pixelLists.Count-1);
                }                
            }
        }

        if (IsAwaitingReelStop)
        {
            GachaReel reel = GachaReels[ReelNumber];
            Label label = Labels[ReelNumber];

            // Stop all players from updating cameras
            for (int i = 0; i < Global.NumberOfPlayers; i++)
            {
                Game.Instance.GetPlayer(i).UpdateCamera = false;
            }

            //TODO: multiplayer
            camera.GlobalPosition = camera.GlobalPosition.LinearInterpolate
            (
                new Vector2(0f, goalRoom.GlobalPosition.y + reel.RectPosition.y + reel.RectSize.y / 2f)
            , 0.1f);

            float yOffset = Global.RoundNumber == 0 ? -32f : -64f;
            camera.Offset = new Vector2(-Const.SCREEN_HALF_WIDTH + reel.RectPosition.x + reel.RectSize.x / 2f, yOffset); 
            camera.Zoom = camera.Zoom.LinearInterpolate(new Vector2(0.65f, 0.65f), 0.1f);

            label.Text = reel.CurrentItem.GachaPrize.Name;  
        }
        else
        {
            ReelTimer -= delta;

            if (ReelTimer <= 0f)
            {
                if (ReelNumber < GachaReels.Length)
                {
                    GachaReel reel = GachaReels[ReelNumber];
                    reel.IsReadyForStop = true;
                    reel.Connect(nameof(GachaReel.ReelStopped), this, nameof(OnReelStopped));
                    reel.Connect(nameof(GachaReel.ReelTicked), this, nameof(OnReelTicked));

                    // Hide description
                    DescriptionLabel.Visible = false;

                    IsAwaitingReelStop = true;
                }
            }
        }
    }

    private void OnReelStopped(int itemHit)
    {
        GachaReels[ReelNumber].Disconnect(nameof(GachaReel.ReelStopped), this, nameof(OnReelStopped));
        GachaReels[ReelNumber].Disconnect(nameof(GachaReel.ReelTicked), this, nameof(OnReelTicked));

        // Play animation
        GachaReels[ReelNumber].HighlightWinner(itemHit);

        // Show description
        DescriptionLabel.Visible = true;

        // Remove pixels for reel
        EnemyColour justStoppedReel = (EnemyColour)ReelNumber;
        float delay = 0f;

        foreach (PixelList pixelList in PixelMap[justStoppedReel])
        {    
            foreach (Pixel pixel in pixelList)
            {
                pixel.SuckUpAndOut(+32f, delay: delay);
            }

            delay += 0.05f;
        }       

        // Reset for next reel
        ReelTimer = REEL_TIMER_MAX;
        ReelNumber++;

        IsAwaitingReelStop = false;        

        // No more reels?
        if (ReelNumber >= GachaReels.Length)
        {
            //TODO: multiplayer
            Game.Instance.GetPlayer(0).UpdateCamera = true;

            IsSpinning = false;
            EmitSignal(nameof(AllReelsSpun));
        }
    }

    private void OnReelTicked(int itemHit)
    {
        string description = GachaReels[ReelNumber].CurrentItem.GachaPrize.Description;
        DescriptionLabel.Text = description;
    }
    

    private async Task SpawnPixels()
    {
        // Create each cloud at correct height
        Task blue = SpawnCloudAndAwaitSettled(40f, EnemyColour.Blue);
        Task yellow = SpawnCloudAndAwaitSettled(140f, EnemyColour.Yellow);
        Task red = SpawnCloudAndAwaitSettled(240f, EnemyColour.Red);
        Task green = SpawnCloudAndAwaitSettled(340f, EnemyColour.Green); // Not yet used

        await Task.WhenAll(blue, yellow, red, green);
    }

    private void SpawnCloud(EnemyColour colour, float xBegin, float heightPercent)
    {
        const float pixelSize = 4f;
        const float bottom = 192f;

        Scene<Pixel> pixelScene = R.Prefabs.PIXEL;
        
        //TODO: multiplayer
        Character player1 = Game.Instance.GetPlayer(0);
        int collectedPixels = Global.GetCollectedPixels(colour);

        PixelListsForColour pixelLists = new PixelListsForColour();
        for (int yCoord = 0; yCoord < collectedPixels; yCoord++)
        {
            float y = bottom - ((float)yCoord) * pixelSize;                

            PixelList thisHeight = new PixelList();
            for (float x = xBegin; x < xBegin + 40f; x += pixelSize)
            {
                Pixel p = pixelScene.Instance();
                p.CustomSuckTarget = player1.GetCurrentRoom().GlobalPosition + new Vector2(x, y);
                p.LifetimeMultiplier = 0.25f;
                p.PixelSprite.Modulate = colour.ToColor();
                p.PixelSprite.Scale = new Vector2(pixelSize, pixelSize);
                p.CollisionShape.Shape = new RectangleShape2D { Extents = p.PixelSprite.Scale };
                GetParent().AddChild(p);

                p.GlobalPosition = player1.GlobalPosition + new Vector2((float)GD.RandRange(-20f, +20f), (float)GD.RandRange(-64f, -32f));

                thisHeight.Add(p);
            }
            pixelLists.Add(thisHeight);
        }

        PixelMap.Add(colour, pixelLists);
    }

    private async Task SpawnCloudAndAwaitSettled(float xCoord, EnemyColour colour)
    {
        if (Global.GetCollectedPixels(colour) > 0f)
        {
            SpawnCloud(colour, xCoord, Mathf.Min(1f, Global.GetCollectedPixels(colour) / 100f));
            await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
        }
        else
        {
            PixelMap.Add(colour, new PixelListsForColour());    
        }
    }
}
