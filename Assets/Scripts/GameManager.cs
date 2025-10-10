using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform playerCardsHolder;
    public Transform opponentCardsHolder;
    public Transform communityCardsHolder;
    public Sprite[] cardSprites;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> playerCards = new List<CardData>();
    private List<CardData> opponentCards = new List<CardData>();
    private List<CardData> communityCards = new List<CardData>();

    void Start()
    {
        CreateDeck();
        ShuffleDeck();
        DealPlayerCards();
        DealCommunityCards();
    }

    public void ResetButton()
    {
        ClearPlayerCardHolder();
        ClearCommunityHolder();

        playerCards.Clear();
        opponentCards.Clear();
        communityCards.Clear();

        ShuffleDeck();
        DealPlayerCards();
        DealCommunityCards();
    }

    // ============================== functions for reset ==============================
    void ClearPlayerCardHolder()
    {
        for (int i = playerCardsHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(playerCardsHolder.GetChild(i).gameObject);
        }

        for (int i = opponentCardsHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(opponentCardsHolder.GetChild(i).gameObject);
        }
    }
    void ClearCommunityHolder()
    {
        for (int i = communityCardsHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(communityCardsHolder.GetChild(i).gameObject);
        }
    }

    void CreateDeck()
    {
        int spriteIndex = 0;
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                CardData cardData = ScriptableObject.CreateInstance<CardData>();
                cardData.suit = suit;
                cardData.rank = rank;

                if (spriteIndex < cardSprites.Length)
                {
                    cardData.cardImage = cardSprites[spriteIndex];
                }

                deck.Add(cardData);
                spriteIndex++;
            }
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    // ============================== /functions for reset ==============================

    // ============================== deal cards ==============================
    void DealPlayerCards()
    {
        for (int i = 0; i < 2; i++)
        {
            if (deck.Count > 0)
            {
                // Player card
                CardData drawnCard = deck[0];
                deck.RemoveAt(0);
                playerCards.Add(drawnCard);
                DisplayCard(drawnCard, playerCardsHolder);

                // Opponent card
                CardData drawnCardOpponent = deck[0];
                deck.RemoveAt(0);
                opponentCards.Add(drawnCardOpponent);
                DisplayCard(drawnCardOpponent, opponentCardsHolder);
            }
            else
            {
                Debug.Log("Deck is empty!");
            }
        }
    }

    void DealCommunityCards()
    {
        for (int i = 0; i < 5; i++)
        {
            if (deck.Count > 0)
            {
                CardData drawnCard = deck[0];
                deck.RemoveAt(0);
                communityCards.Add(drawnCard);
                DisplayCommunityCard(drawnCard);

                //Debug.Log($"Community Card {i + 1}: {drawnCard.rank} of {drawnCard.suit} (Value: {(int)drawnCard.rank})");
            }
            else
            {
                Debug.Log("Deck is empty!");
                break;
            }
        }
        //Debug.Log($"Community cards dealt: {communityCards.Count}. Remaining deck: {deck.Count}");
    }
    // ============================== /deal cards ==============================

    // ============================== display cards ==============================
    void DisplayCard(CardData cardData, Transform holder)
    {
        GameObject cardObject = Instantiate(cardPrefab, holder);
        CardUI cardComponent = cardObject.GetComponent<CardUI>();

        if (cardComponent != null)
        {
            cardComponent.Setup(cardData);
        }
    }

    void DisplayCommunityCard(CardData cardData)
    {
        GameObject cardObject = Instantiate(cardPrefab, communityCardsHolder);
        CardUI cardComponent = cardObject.GetComponent<CardUI>();

        if (cardComponent != null)
        {
            cardComponent.Setup(cardData);
        }
    }
    // ============================== /display cards ==============================

    // Player cards
    public List<CardData> GetAllPlayerCards()
    {
        List<CardData> allCards = new List<CardData>();
        allCards.AddRange(playerCards);
        allCards.AddRange(communityCards);
        return allCards;
    }

    // Opponent cards
    public List<CardData> GetAllOpponentCards()
    {
        List<CardData> allCards = new List<CardData>();
        allCards.AddRange(opponentCards);
        allCards.AddRange(communityCards);
        return allCards;
    }

}