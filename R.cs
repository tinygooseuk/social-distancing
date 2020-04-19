using System;
using Godot;

public static class R 
{
    public static class Model 
    {
        public const string GACHA_PRIZES = "res://Model/GachaPrizes.tres";    
    }

    public static class Prefabs
    {
        public const string BULLET = "res://Prefabs/Bullet.tscn";
        public const string PIXEL = "res://Prefabs/Pixel.tscn";

        public const string CHARACTER = "res://Prefabs/Character.tscn";

        public static class UI 
        {
            public const string GACHA_TILE = "res://Prefabs/UI/GachaTile.tscn";
        }
    }

    public static class Particles 
    {
        public const string DEATH_PARTICLES = "res://Particles/DeathParticles.tscn";
        public const string JUMP_PARTICLES = "res://Particles/JumpParticles.tscn";
    }

    public static class Sounds
    {
        public const string SHOOT = "res://Sounds/Shoot.wav";
        public const string ENEMY_DEATH = "res://Sounds/EnemyDeath.wav";
        public const string PLAYER_DEATH = "res://Sounds/PlayerDeath.wav";
    
        public const string COLLECT_PIXEL = "res://Sounds/CollectPixel.wav";
    }

    public static class Rooms 
    {
        public static readonly string[] EASY_ROOMS = new[]
        {
            "res://Rooms/Easy/Easy_1.tscn",
            "res://Rooms/Easy/Easy_2.tscn",
            "res://Rooms/Easy/Easy_3.tscn",
            "res://Rooms/Easy/Easy_4.tscn",
            "res://Rooms/Easy/Easy_5.tscn",
        };

        public static readonly string[] MEDIUM_ROOMS = new[]
        {
            //"res://Rooms/Easy/Easy_1.tscn",
            "res://Rooms/Medium/Medium_2.tscn",
        };

        public static readonly string[] HARD_ROOMS = new[]
        {
            "res://Rooms/Easy/Easy_1.tscn",
        };

        public static readonly string[] NEUTRAL_ROOMS = new[]
        {
            "res://Rooms/Neutral/Neutral_1.tscn",
            "res://Rooms/Neutral/Neutral_2.tscn",
            "res://Rooms/Neutral/Neutral_3.tscn",
        };

        public const string GOAL_ROOM = "res://Rooms/GoalRoom.tscn";
    }

    public static class Scenes
    {
        public const string MAIN_MENU = "res://Scenes/MainMenu.tscn";

        public static string GetGameSceneForNumPlayers(int numPlayers) => new []
        {
            null,
            R.Scenes.SINGLE_PLAYER,
            R.Scenes.TWO_PLAYER,
            R.Scenes.THREE_PLAYER,
            R.Scenes.FOUR_PLAYER,
        }[numPlayers];
        public const string SINGLE_PLAYER = "res://Scenes/SinglePlayer.tscn";
        public const string TWO_PLAYER = "res://Scenes/TwoPlayer.tscn";
        public const string THREE_PLAYER = "res://Scenes/ThreePlayer.tscn";
        public const string FOUR_PLAYER = "res://Scenes/FourPlayer.tscn";
    }
}