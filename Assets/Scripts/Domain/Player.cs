using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Player
{
    //chips, betThisRound, hasActed, holeCards, playerType? / CanBet, AddChips, RemoveChips
    public int Chips { get; set; }
    public List<CardData> HoleCards { get; private set; }
    public Transform CardsHolder { get; private set; }

    public bool BetThisRound { get; private set; }
    public bool HasActed { get; private set; }
    public bool IsHuman { get; private set; }

    public Player(int initialChips, Transform cardsHolder, bool isHuman)
    {
        Chips = initialChips;
        CardsHolder = cardsHolder;
        IsHuman = isHuman;
        HoleCards = new List<CardData>();
    }
}
