using System;
using Godot;

public static class R 
{
    public static class Prefabs
    {
        public static string Bullet = "res://Prefabs/Bullet.tscn";
        public static string Pixel = "res://Prefabs/Pixel.tscn";
        public static string DeathParticles = "res://Prefabs/DeathParticles.tscn";

        public static string Character = "res://Prefabs/Character.tscn";

    }
    public static class Sounds
    {
        public static string Shoot = "res://Sounds/Shoot.wav";
        public static string EnemyDeath = "res://Sounds/EnemyDeath.wav";
        public static string PlayerDeath = "res://Sounds/PlayerDeath.wav";
    
        public static string CollectPixel = "res://Sounds/CollectPixel.wav";
    }

    public static class Rooms 
    {
        public static string[] EasyRooms = new[]
        {
            "res://Rooms/Easy/Easy_1.tscn"
        };

        public static string[] MediumRooms = new[]
        {
            "res://Rooms/Easy/Easy_1.tscn"
        };

        public static string[] HardRooms = new[]
        {
            "res://Rooms/Easy/Easy_1.tscn"
        };

        public static string GoalRoom = "res://Rooms/GoalRoom.tscn";
    }

    public static class Scenes
    {
        public static string MainMenu = "res://Scenes/MainMenu.tscn";


        public static string GetGameSceneForNumPlayers(int numPlayers) => new []
        {
            null,
            R.Scenes.SinglePlayer,
            R.Scenes.TwoPlayer,
            R.Scenes.ThreePlayer,
            R.Scenes.FourPlayer,
        }[numPlayers];
        public static string SinglePlayer = "res://Scenes/SinglePlayer.tscn";
        public static string TwoPlayer = "res://Scenes/TwoPlayer.tscn";
        public static string ThreePlayer = "res://Scenes/ThreePlayer.tscn";
        public static string FourPlayer = "res://Scenes/FourPlayer.tscn";
    }
}