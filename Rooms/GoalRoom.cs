using Godot;
using Godot.Collections;
using System;

public class GoalRoom : Room
{
    // Subnodes
    [Subnode] private Area2D GoalDetector;
    [Subnode("GoalBanner/GoalBannerSprite")] private Sprite GoalBanner;
    [Subnode] GachaScreen Gacha;

    // State
    private static bool IsFirstRound => Global.RoundNumber == 0;
    private bool WasTriggered = false;

    public override void _Ready()
    {
        this.FindSubnodes();

        if (IsFirstRound)
        {
            GoalBanner.QueueFree();
            GoalBanner = null;
        }
    }

    private async void OnGoalReached(Node2D triggerer)
    {   
        if (WasTriggered) return;

        if (triggerer is Character c)
        {
            WasTriggered = true;

            // Tell game
            Game.Instance.MarkRoundComplete();
            
            // Teleport all chars in
            Array<Character> characters = new Array<Character>(GetTree().GetNodesInGroup(Groups.PLAYERS));
            foreach (Character character in characters)
            {
                character.GlobalPosition = new Vector2(character.GlobalPosition.x, GoalDetector.GlobalPosition.y);
                character.MarkRoundComplete();
            }

            if (IsFirstRound)
            {
                // Hack for 1st round camera :shrug:
                for (int i = 0; i < Global.NumberOfPlayers; i++)
                {
                    Camera2D camera = Game.Instance.GetPlayerCamera(i);
                    camera.LimitTop -= 33;
                    camera.LimitBottom = -240;
                }
            }
            else 
            {
                // Burst everything
                await ToSignal(GetTree().CreateTimer(2f), "timeout");            
                GoalBanner.BurstIntoPixels((KinematicBody2D)GoalBanner.GetParent(), suck: true, pixelSize: 4, lifetimeMultiplier: 4f);
                GoalBanner.QueueFree();

                // Vibrate
                if (Game.Instance.InputMethodManager.IsVibrationEnabled)
                {
                    for (int i = 0; i < Global.NumberOfPlayers; i++)
                    {
                        Game.Instance.GetPlayer(i).Vibrate(1f, 1f, 0.5f);
                    }
                }

                await ToSignal(GetTree().CreateTimer(7f), "timeout");            
            }

            // Show gacha            
            Gacha.Visible = true;
            
            Gacha.Start();
            await ToSignal(Gacha, nameof(GachaScreen.AllReelsSpun));

            // Wait a sec
            await ToSignal(GetTree().CreateTimer(2f), "timeout"); 

            // Wait for animation to end too
            await Game.Instance.TitleCard.AnimateOut();

            // Work out and apply all prizes
            //TODO: player index!!
            foreach (GachaReel reel in Gacha.GachaReels)
            {
                GachaPrize prize = reel.CurrentItem.GachaPrize;

                if (prize.UnlockedShootBehaviour != ShootBehavioursEnum.None)
                {
                    Global.ShootBehaviours[0] = ShootBehavioursFactory.Create(prize.UnlockedShootBehaviour);
                }

                if (prize.UnlockedBehaviourModfier != BehaviourModifiersEnum.None)
                {
                    Global.BehaviourModifiers[0].Add(BehaviourModifiersFactory.Create(prize.UnlockedBehaviourModfier));
                }
            }

            // Mark end of round
            Global.EndRound(Game.Instance);
        }
    }
}
