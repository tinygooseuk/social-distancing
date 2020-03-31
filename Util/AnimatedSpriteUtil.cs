using System;
using Godot;
using System.Collections.Generic;

public static class AnimatedSpriteUtil
{
    public static void PlayIfNotAlready(this AnimatedSprite sprite, string animation, bool backwards = false)
    {
        if (sprite.Animation != animation)
        {
            sprite.Play(animation, backwards);
        }
    }

    public static IEnumerable<Pixel> BurstIntoPixels(this AnimatedSprite sprite, KinematicBody2D body, bool suck = true, Vector2? overridePosition = null, Transform2D? overrideTransform = null)
    {
        SceneTree tree = sprite.GetTree();

        Vector2 usePosition = overridePosition.GetValueOrDefault(body.Position);
        Transform2D useTransform = overrideTransform.GetValueOrDefault(new Transform2D
        {
            origin = Vector2.Zero,
            x = sprite.GlobalTransform.x,
            y = sprite.GlobalTransform.y,
        });

        AtlasTexture spriteTexture = (AtlasTexture)sprite.Frames.GetFrame("Idle", 0);
        Image spriteImage = spriteTexture.Atlas.GetData();

        Scene<Pixel> pixelScene = R.Prefabs.Pixel;
        pixelScene.Load();

        

        int numPixels = tree.GetNodesInGroup("pixels").Count;
        int skip = 1 + Mathf.RoundToInt(numPixels / 100);
        int counter = 0;

        List<Pixel> pixels = new List<Pixel>();

        spriteImage.Lock();
        {
            for (int y = 0; y < spriteTexture.GetHeight(); y++)
            {
                for (int x = 0; x < spriteTexture.GetWidth(); x++)
                {
                    if (counter++ % skip > 0)
                    {
                        continue;
                    }

                    Vector2 pixelOffset = spriteTexture.Region.Position;
                    Color pixelColour = spriteImage.GetPixel((int)pixelOffset.x + x, (int)pixelOffset.y + y);
                    bool isPixel = pixelColour.a > 0.5f;
                    bool isBlack = false;                    

                    if (!isPixel)
                    {
                        // not a pixel. see if it's a black outline pixel
                        float neighbourAlpha = 0.0f;
                        for (int xx = -1; xx <= +1; xx++)
                        {
                            for (int yy = -1; yy <= +1; yy++)
                            {
                                if (xx == 0 && yy == 0) continue;
                                
                                int nx = (int)pixelOffset.x + x + xx;
                                if (nx < 0 || nx >= spriteImage.GetWidth()) continue;
                                
                                int ny = (int)pixelOffset.y + y + yy;
                                if (ny < 0 || ny >= spriteImage.GetHeight()) continue;

                                neighbourAlpha += spriteImage.GetPixel(nx, ny).a;
                            }   
                        }

                        if (neighbourAlpha > 1.0f)
                        {
                            isPixel = true;
                            isBlack = true;
                        }
                    }

                    if (isPixel)
                    {
                        Vector2 offset = new Vector2((float)x, (float)y) - spriteTexture.GetSize() / 2.0f;

                        Pixel pixel = pixelScene.Instance();
                        pixel.CanSuck = suck;
                        pixel.Position = usePosition + useTransform.Xform(offset);
                        pixel.Modulate = isBlack ? Colors.Black : pixelColour * body.Modulate;
                        body.GetParent().AddChild(pixel);

                        pixels.Add(pixel);
                    }                
                }   
            }
        }
        spriteImage.Unlock();

        return pixels;
    }
}