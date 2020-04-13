using System;
using System.Collections.Generic;
using Godot;

public static class Global
{
    // Gamestate
    public static int NumberOfPlayers = 1;
    public static int RoundNumber = 0;

    public static int TotalScore = 0;

    // Shared character state
    private static int[] CollectedPixels = new[] { 0, 0, 0, 0 };
    public static int GetCollectedPixels(EnemyColour colour) => CollectedPixels[(int)colour];
    public static void SetCollectedPixels(EnemyColour colour, int amount) => CollectedPixels[(int)colour] = amount;
    public static void IncrementCollectedPixels(EnemyColour colour) => CollectedPixels[(int)colour]++;
    public static void DecrementCollectedPixels(EnemyColour colour) => CollectedPixels[(int)colour]--;
    
    // Character modifiers
    public static List<List<IBehaviourModifier>> BehaviourModifiers = new List<List<IBehaviourModifier>>();

    // Behaviours
    public static List<IShootBehaviour> ShootBehaviours = new List<IShootBehaviour>();

    public static void EndRound(Game endedGame)
    {
        //TODO: move any vars from game into persistent storage here
        RoundNumber++;

        for (int i = 0; i < EnumUtil.GetCount<EnemyColour>(); i++)
        {
            CollectedPixels[i] = 0;
        }

        TotalScore += endedGame.TotalScore;

        Game.Instance.ReloadGameScene();
    }

    public static void Reset()
    {
        Engine.TimeScale = 1f;
        RoundNumber = 0;
        TotalScore = 0;
        
        CollectedPixels[(int)EnemyColour.Blue] = 0;
        CollectedPixels[(int)EnemyColour.Yellow] = 0;
        CollectedPixels[(int)EnemyColour.Red] = 0;

        BehaviourModifiers.Clear();
        ShootBehaviours.Clear();

        for (int i = 0; i < Const.MAX_SUPPORTED_PLAYERS; i++)
        {
            BehaviourModifiers.Add(new List<IBehaviourModifier>());
            ShootBehaviours.Add(new DefaultShootBehaviour());
        }

        //TODO: reset any game-specific vars here
    }
}

public static class Const
{
    public const float SCREEN_WIDTH = 416f;
    public const float SCREEN_HEIGHT = 240f;

    public const float SCREEN_HALF_WIDTH = SCREEN_WIDTH * 0.5f;
    public const float SCREEN_HALF_HEIGHT = SCREEN_HEIGHT * 0.5f;

    public const int MAX_PIXELS = 12;
    public const int MAX_SUPPORTED_PLAYERS = 4;
}

public static class Groups 
{
    public const string PLAYERS = "players";
    public const string ENEMIES = "enemies";
}