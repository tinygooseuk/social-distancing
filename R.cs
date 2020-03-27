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
    
    }

    public static class Scenes
    {
        public static string MainMenu = "res://Scenes/MainMenu.tscn";
        public static string SinglePlayer = "res://Scenes/SinglePlayer.tscn";
        public static string TwoPlayer = "res://Scenes/TwoPlayer.tscn";
    }
}