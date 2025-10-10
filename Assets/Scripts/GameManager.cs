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
        DealHoleCards();
        DealCommunityCards(3);
    }

    public void ResetButton()
    {
        ClearAllCardHolders();
        ShuffleDeck();
        DealHoleCards();
        DealCommunityCards(5);
    }

    // this will execute only one time at the very beginning
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
                cardData.cardImage = cardSprites[spriteIndex];


                deck.Add(cardData);
                spriteIndex++;
            }
        }
    }

    // this will execute every time when reset
    void ClearAllCardHolders()
    {
        // cards in holders go back to deck
        deck.AddRange(playerCards);
        deck.AddRange(opponentCards);
        deck.AddRange(communityCards);

        // destroying game objects
        for (int i = playerCardsHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(playerCardsHolder.GetChild(i).gameObject);
        }

        for (int i = opponentCardsHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(opponentCardsHolder.GetChild(i).gameObject);
        }

        for (int i = communityCardsHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(communityCardsHolder.GetChild(i).gameObject);
        }

        // clearing the lists
        playerCards.Clear();
        opponentCards.Clear();
        communityCards.Clear();
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

    // ============================== deal cards ==============================
    
    // render cards
    void DisplayCard(CardData cardData, Transform holder)
    {
        GameObject cardObject = Instantiate(cardPrefab, holder);
        CardUI cardComponent = cardObject.GetComponent<CardUI>();
        if (cardComponent != null)
        {
            cardComponent.Setup(cardData);
        }
    }
    private CardData DrawCard()
    {
        CardData card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    // pre-flop
    void DealHoleCards()
    {
        for (int i = 0; i < 2; i++)
        {
            // Player
            CardData playerCard = DrawCard();
            playerCards.Add(playerCard);
            DisplayCard(playerCard, playerCardsHolder);

            // Opponent
            CardData opponentCard = DrawCard();
            opponentCards.Add(opponentCard);
            DisplayCard(opponentCard, opponentCardsHolder);
        }
    }

    // deal 3, 1, 1
    void DealCommunityCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            CardData card = DrawCard();
            communityCards.Add(card);
            DisplayCard(card, communityCardsHolder);
        }
    }
    // ============================== /deal cards ==============================


    public List<CardData> GetAllCards(List<CardData> holeCards)
    {
        List<CardData> allCards = new List<CardData>();
        allCards.AddRange(holeCards);
        allCards.AddRange(communityCards);
        return allCards;
    }

}