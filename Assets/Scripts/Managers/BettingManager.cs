using System.Collections.Generic;
using UnityEngine;

public class BettingManager
{
    private int pot = 0;
    public readonly int smallBlind = 5;
    public readonly int bigBlind = 10;

    public int Pot => pot; // encapsulation (to make readonly)


    public void PostBlind(List<Player> players, int dealerIndex)
    {
        int smallBlinder = (dealerIndex + 1) % players.Count;
        int bigBlinder = (dealerIndex + 2) % players.Count;

        players[smallBlinder].BetThisRound = players[smallBlinder].Bet(smallBlind); 
        pot += players[smallBlinder].BetThisRound;

        players[bigBlinder].BetThisRound = players[bigBlinder].Bet(bigBlind);
        pot += players[bigBlinder].BetThisRound;

        players[bigBlinder].HasActed = true;
    }

    public void Call(Player player, int amount = 0) 
    {
        int playerBet = player.Bet(amount);
        pot += playerBet;
        player.HasActed = true;
    }

    public void Raise(List<Player> players, Player player, int amount)
    {
        if (!player.CanBet(amount))
        {
            Call(player, amount);
            Debug.Log("Not enough chips. Call is called instead");
            return;
        }
        foreach (var p in players)
        {
            if (p != player)
            {
                p.HasActed = false;
            }
        }
        int playerBet = player.Bet(amount);
        pot += playerBet;
        player.HasActed = true;
    }

    public void PayoutChips(Player player, int amount)
    {
        player.Chips += amount;
        pot -= amount;
    }
}
