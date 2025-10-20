using System.Collections.Generic;
using UnityEngine;

public class DeckManager
{
    private readonly List<CardData> deck = new();
    private readonly Sprite[] cardSprites;

    public DeckManager(Sprite[] cardSprites)
    {
        this.cardSprites = cardSprites;
    }

    public void CreateDeck()
    {
        int spriteIndex = 0;
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                CardData cardData = ScriptableObject.CreateInstance<CardData>();
                cardData.suit = suit;
                cardData.rank = rank;
                cardData.cardImage = cardSprites[spriteIndex];
                deck.Add(cardData);
                spriteIndex++;
            }
        }
    }

    public void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public CardData DrawCard()
    {
        CardData card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public void ReturnCards(List<CardData> cards)
    {
        deck.AddRange(cards);
    }
}
