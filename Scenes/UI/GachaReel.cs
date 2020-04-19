using Godot;
using Godot.Collections;
using System;

enum GachaReelState
{
    WaitingToStart,
    SpinningUp,
    Holding,
    SpinningDown,
    Bumping,
    Stopped,
}

public class GachaReel : ScrollContainer
{
    // Consts
    private const int NUM_BUFFER_ITEMS = 3;

    // Subnodes
    [Subnode] private VBoxContainer ReelBox;
    [Subnode] private Tween BumpTween;

    [Subnode("Sounds/Tick")] private AudioStreamPlayer2D Sound_Tick;
    [Subnode("Sounds/Pick")] private AudioStreamPlayer2D Sound_Pick;

    // Signals
    [Signal] public delegate void ReelStopped(int item);

    // Exports 
    [Export] private float TargetSpinSpeed = 0f;
    [Export] private float DesiredHoldTime = 3f;
    [Export] private float DesiredHoldTimeRandom = 2f;
    [Export] private bool AutoStart = false;

    // Getters/Setters
    public bool IsReadyForStop = false;
    public float ItemHeight => ReelBox.RectSize.y / ReelBox.GetChildCount();  
    public int CurrentItemIndex => Mathf.RoundToInt(ScrollOffset / ItemHeight) % ReelBox.GetChildCount();
    public GachaTile CurrentItem => (GachaTile)ReelBox.GetChild(CurrentItemIndex);
    public int NumberOfItems => ReelBox.GetChildCount() - NUM_BUFFER_ITEMS;

    // Internal State
    private float SpinSpeed = 0f;
    private float ScrollOffset = 0f;
    private float ActualHoldTime = 0f;
    private int LastItemIndex = -1;
    private GachaReelState GachaReelState = GachaReelState.WaitingToStart;

    public override void _Ready()
    {
        this.FindSubnodes();

        ActualHoldTime = DesiredHoldTime + (float)GD.RandRange(0f, +DesiredHoldTime);

        if (AutoStart)
        {
            GachaReelState = GachaReelState.SpinningUp;
        }
    }

    public override void _Process(float delta)
    {
        switch (GachaReelState)
        {
            case GachaReelState.SpinningUp:
            {
                SpinSpeed = Mathf.MoveToward(SpinSpeed, TargetSpinSpeed, delta * 400f);
                if (Mathf.Abs(SpinSpeed - TargetSpinSpeed) < 5f)
                {
                    GachaReelState = GachaReelState.Holding;
                }
                break;
            }
            case GachaReelState.Holding:
            {
                ActualHoldTime -= delta;
                if (ActualHoldTime <= 0f && IsReadyForStop)
                {                    
                    GachaReelState = GachaReelState.SpinningDown;
                }
                break;
            }
            case GachaReelState.SpinningDown:
            {
                SpinSpeed = Mathf.MoveToward(SpinSpeed, 0f, delta * 100f);
                if (Mathf.Abs(SpinSpeed) < 25f)
                {
                    GachaReelState = GachaReelState.Bumping;
                    SpinSpeed = 0f;

                    EmitSignal(nameof(ReelStopped), CurrentItemIndex);
                }
                break;
            }

            case GachaReelState.Bumping:
            {
                float idealScrollOffset = (float)CurrentItemIndex * ItemHeight;
                BumpTween.InterpolateProperty(this, nameof(ScrollOffset), null, idealScrollOffset, 0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
                BumpTween.Start();

                GachaReelState = GachaReelState.Stopped;
                break;
            }
            
            default: break;
        }

        SyncScrollerToGacha(delta);
    }

    public bool RequestBump()
    {
        switch (GachaReelState)
        {
            case GachaReelState.SpinningDown:
            {
                SpinSpeed += 50f;
                return true;
            }
        }

        return false;
    }

    private void SyncScrollerToGacha(float delta)
    {
        if (Mathf.Abs(SpinSpeed) > 0f)
        {
            ScrollOffset += delta * SpinSpeed;
            
            if (ScrollOffset > ItemHeight * NumberOfItems)
            {
                ScrollOffset -= ItemHeight * NumberOfItems;
            }
        }

        ScrollVertical = Mathf.FloorToInt(ScrollOffset);  

        if (CurrentItemIndex != LastItemIndex)
        {
            if (GachaReelState == GachaReelState.SpinningDown)
            {
                // Tick
                Sound_Tick.PitchScale = 1f + Mathf.Clamp(Mathf.InverseLerp(0f, 400f, SpinSpeed), 0f, 1f);
                Sound_Tick.Play();
                
                // Vibrate
                if (Game.Instance.InputMethodManager.IsVibrationEnabled)
                {
                    for (int i = 0; i < Global.NumberOfPlayers; i++)
                    {
                        Input.StartJoyVibration(i, 0.8f, 0f, 0.15f);
                    }
                }
            }

            LastItemIndex = CurrentItemIndex;
        }      
    }

    public async void HighlightWinner(int itemHit)
    {
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

        RectClipContent = false;
        Sound_Pick.Play();
        
        // Vibrate
        if (Game.Instance.InputMethodManager.IsVibrationEnabled)
        {
            for (int i = 0; i < Global.NumberOfPlayers; i++)
            {
                Input.StartJoyVibration(i, 0.6f, 0.6f, 0.3f);
            }
        }

        for (int i = 0; i < ReelBox.GetChildCount(); i++)
        {
            GachaTile thisTile = (GachaTile)ReelBox.GetChild(i);

            if (i == itemHit)
            {
                thisTile.PlayWonAnimation();
            }
            else
            {
                thisTile.Modulate = Colors.Transparent;
            }
        }
    }

    public void SetPrizes(GachaPrize[] prizes)
    {
        Array<GachaPrize> prizeList = new Array<GachaPrize>(prizes);

        // Repeat until at least 5 items
        while (prizeList.Count < 5)
        {
            foreach (GachaPrize prize in prizes)
            {
                prizeList.Add(prize);
            }
        }

        // Add first NUM_BUFFER_ITEMS again at end of reelbox
        for (int i = 0; i < Mathf.Min(prizes.Length, NUM_BUFFER_ITEMS); i++)
        {
            prizeList.Add(prizes[i]);
        }

        // Create tiles
        Scene<GachaTile> tileScene = R.Prefabs.UI.GACHA_TILE;
        tileScene.Load();

        foreach (GachaPrize prize in prizeList)
        {
            GachaTile tile = tileScene.Instance();
            tile.GachaPrize = prize;
            ReelBox.AddChild(tile);
        }
    }
}
