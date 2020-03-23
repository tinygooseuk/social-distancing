using System;
using Godot;

public static class AnimatedSpriteUtil
{
    public static void PlayIfNotAlready(this AnimatedSprite sprite, string animation, bool backwards = false)
    {
        if (sprite.Animation != animation)
        {
            sprite.Play(animation, backwards);
        }
    }
}