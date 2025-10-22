using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string Name { get; private set; }
    public bool IsHuman { get; private set; }
    public Transform CardsHolder { get; private set; }
    public int Chips { get; set; }
    public List<CardData> HoleCards { get; set; }
    public int BetThisRound { get; set; }
    public bool HasActed { get; set; }
    public bool HasFolded { get; set; }

    // initializing player object
    public Player(string name, int initialChips, Transform cardsHolder, bool isHuman)
    {
        Name = name;
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
        Debug.Log($"{this.Name} bet {BetThisRound} this round");
        return actualBet;
    }

    public void ResetStatus()
    {
        BetThisRound = 0;
        HasActed = false;
    }
}
