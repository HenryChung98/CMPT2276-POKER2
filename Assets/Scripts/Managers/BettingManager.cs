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
        int next = (dealerIndex + 1) % players.Count;

        players[dealerIndex].BetThisRound = players[dealerIndex].Bet(smallBlind); 
        pot += players[dealerIndex].BetThisRound;
        Debug.Log($"{players[dealerIndex].Name}(dealer) post small blind");

        players[next].BetThisRound = players[next].Bet(bigBlind);
        pot += players[next].BetThisRound;
        Debug.Log($"{players[next].Name} post big blind");

        players[next].HasActed = true;
    }

    //public void PlaceBet(Player player, Player opponent, int amount)
    //{
    //if (!player.CanBet(amount))
    //{
    //    Debug.Log("Not enough chips.");
    //    return;
    //}
    //// player bets, opponent auto-calls (toy logic)
    //int playerBet = player.Bet(amount);
    //pot += playerBet;

    //int opponentBet = opponent.Bet(amount);
    //pot += opponentBet;

    //Debug.Log($"You bet {playerBet}. Opponent calls {opponentBet}. Pot is now {pot}.");
    //}
    public void Call(Player player, int amount = 0) 
    {
        if (!player.CanBet(amount))
        {
            Debug.Log("Not enough chips.");
            return;
        }
        int playerBet = player.Bet(amount);
        pot += playerBet;
        player.HasActed = true;
    }

    public void Raise(List<Player> players, Player player, int amount)
    {
        if (!player.CanBet(amount))
        {
            Debug.Log("Not enough chips.");
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
    }
    public void ResetPot()
    {
        pot = 0;
    }
}
