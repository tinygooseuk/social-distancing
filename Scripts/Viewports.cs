using Godot;
using System;

public class Viewports : HBoxContainer
{
    public override void _Ready()
    {
        if (GetChildCount() < 2) 
        {
            // Only one VP - nothing to do
            return;
        }
      
        ViewportContainer playerOneViewportContainer = (ViewportContainer)GetChild(0);
        Viewport playerOneViewport = (Viewport)playerOneViewportContainer.GetChild(0);

        for (int vp = 1; vp < GetChildCount(); vp++)
        {
            ViewportContainer otherPlayerViewportContainer = (ViewportContainer)GetChild(vp);
            Viewport otherPlayerViewport = (Viewport)otherPlayerViewportContainer.GetChild(0);

            otherPlayerViewport.World2d = playerOneViewport.World2d;
        }
        
    }
}
