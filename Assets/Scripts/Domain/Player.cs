using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public bool IsHuman { get; private set; }
    public int Chips { get; set; }
    public List<CardData> HoleCards { get; private set; }
    public Transform CardsHolder { get; private set; }
    public int BetThisRound { get; private set; }
    public bool HasActed { get; private set; }

    // initializing player object
    public Player(int initialChips, Transform cardsHolder, bool isHuman)
    {
        IsHuman = isHuman;
        Chips = initialChips;
        HoleCards = new List<CardData>();
        CardsHolder = cardsHolder;
        BetThisRound = 0;
        HasActed = false;
    }

    public bool CanBet(int amount) => Chips >= amount;

    public void AddChips(int amount) => Chips += amount;

    public int Bet(int amount)
    {
        // reason why using mathf.Min is to prevent the chips become negative value (like when all-in is happened)
        int actualBet = Mathf.Min(amount, Chips); 
        Chips -= actualBet;
        BetThisRound += actualBet;
        HasActed = true;
        return actualBet;
    }

    public void ResetStatus()
    {
        BetThisRound = 0;
        HasActed = false;
    }
}
