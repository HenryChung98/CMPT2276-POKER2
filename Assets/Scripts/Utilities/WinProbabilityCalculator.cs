using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WinProbabilityCalculator
{
    private DeckManager deckManager;

    public WinProbabilityCalculator(DeckManager deckManager)
    {
        this.deckManager = deckManager;
    }

    public float CalculateWinProbability(
        List<CardData> playerHoleCards,
        List<CardData> communityCards,
        int simulations = 1000)
    {
        int wins = 0;
        int ties = 0;

        List<CardData> knownCards = new List<CardData>();
        knownCards.AddRange(playerHoleCards);
        knownCards.AddRange(communityCards);

        for (int i = 0; i < simulations; i++)
        {
            // Get available cards (full deck minus known cards)
            List<CardData> availableCards = GetAvailableCards(knownCards);

            // Complete the community cards if needed
            List<CardData> simulatedCommunity = new List<CardData>(communityCards);
            int cardsNeeded = 5 - communityCards.Count;

            for (int j = 0; j < cardsNeeded; j++)
            {
                int randomIndex = Random.Range(0, availableCards.Count);
                simulatedCommunity.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }

            // Deal opponent 2 random hole cards
            List<CardData> opponentHoleCards = new List<CardData>();
            for (int j = 0; j < 2; j++)
            {
                int randomIndex = Random.Range(0, availableCards.Count);
                opponentHoleCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }

            List<CardData> playerAllCards = new List<CardData>(playerHoleCards);
            playerAllCards.AddRange(simulatedCommunity);

            List<CardData> opponentAllCards = new List<CardData>(opponentHoleCards);
            opponentAllCards.AddRange(simulatedCommunity);

            var playerResult = PokerHandEvaluator.EvaluateBestHand(playerAllCards);
            var opponentResult = PokerHandEvaluator.EvaluateBestHand(opponentAllCards);

            int comparison = PokerHandEvaluator.Compare(playerResult, opponentResult);

            if (comparison > 0)
            {
                wins++;
            }

            else if (comparison == 0)
            {
                ties++;
            }
        }

        // Win probability (ties count as half a win)
        return (wins + (ties * 0.5f)) / simulations;
    }

    private List<CardData> GetAvailableCards(List<CardData> knownCards)
    {
        List<CardData> availableCards = new List<CardData>();

        // get all possible cards
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                // Check if this card is already known
                bool isKnown = knownCards.Any(c => c.suit == suit && c.rank == rank);

                if (!isKnown)
                {
                    // Create a temporary card for simulation
                    CardData card = ScriptableObject.CreateInstance<CardData>();
                    card.suit = suit;
                    card.rank = rank;
                    availableCards.Add(card);
                }
            }
        }

        return availableCards;
    }
}