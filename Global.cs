using System;
using Godot;

public static class Global
{
    public static int NumberOfPlayers = 1;
    
    public static void RoundEnded(Game endedGame)
    {
        //TODO: move any vars into persistent storage here
    }

    public static void Reset()
    {
        //TODO: reset any game-specific vars here
    }
}

public static class Const
{
    public const float SCREEN_WIDTH = 416.0f;
    public const float SCREEN_HEIGHT = 240.0f;

    public const float SCREEN_HALF_WIDTH = SCREEN_WIDTH * 0.5f;
    public const float SCREEN_HALF_HEIGHT = SCREEN_HEIGHT * 0.5f;
}

public static class Groups 
{
    public const string PLAYERS = "players";
    public const string ENEMIES = "enemies";
}