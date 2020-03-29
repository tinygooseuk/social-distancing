using System;
using Godot;
using System.Threading.Tasks;

public static class SoundFXUtil
{
    public static async void PlaySound2D(this SceneTree tree, Asset<AudioStream> audioToPlay, Vector2? location = null, Node2D relativeTo = null)
    {
        // Figure out final location
        Vector2 useLocation = location.GetValueOrDefault(Vector2.Zero);

        if (relativeTo != null)
        {
            useLocation = relativeTo.ToGlobal(useLocation);
        }

        AudioStreamPlayer2D player = new AudioStreamPlayer2D
        {
            Stream = await audioToPlay.LoadAsync(),
            Playing = true,
        };
        Game.Instance.AddChild(player);
        player.GlobalPosition = useLocation;

        await tree.ToSignal(tree.CreateTimer(player.Stream.GetLength() + 0.01f), "timeout");
        player.QueueFree();
    }
}