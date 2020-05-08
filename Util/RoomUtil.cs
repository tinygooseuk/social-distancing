using Godot;

public static class RoomUtil 
{
    public static Room GetCurrentRoom(this Node2D node)
    {
        foreach (Node gameChild in Game.Instance.GetChildren())
        {
            if (gameChild is Room room)
            {
                Rect2 roomRect = new Rect2(room.GlobalPosition, new Vector2(Const.SCREEN_WIDTH, Const.SCREEN_HEIGHT));
                if (roomRect.HasPoint(node.GlobalPosition))
                {
                    return room;
                }
            }
        }

        return null;
    }
}