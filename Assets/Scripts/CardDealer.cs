using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    private DeckManager deckManager;
    private UIManager uiManager;
    private float delay;

    public void Initialize(DeckManager deckManager, UIManager uiManager, float delay)
    {
        this.deckManager = deckManager;
        this.uiManager = uiManager;
        this.delay = delay;
    }

    public IEnumerator DealHoleCards(Player player, Transform playerHolder, Player opponent, Transform opponentHolder)
    {
        for (int i = 0; i < 2; i++)
        {
            CardData playerCard = deckManager.DrawCard();
            player.HoleCards.Add(playerCard);
            uiManager.DisplayCard(playerCard, playerHolder, faceDown: false); // Face up
            yield return new WaitForSeconds(delay);

            CardData opponentCard = deckManager.DrawCard();
            opponent.HoleCards.Add(opponentCard);
            uiManager.DisplayCard(opponentCard, opponentHolder, faceDown: true); // Face down!
            yield return new WaitForSeconds(delay);
        }
    }
    public void DealCommunityCards(List<CardData> communityCardList, Transform communityHolder, int num)
    {
        if (communityCardList.Count >= 5) return;

        for (int i = 0; i < num && communityCardList.Count < 5; i++)
        {
            CardData card = deckManager.DrawCard();
            communityCardList.Add(card);
            uiManager.DisplayCard(card, communityHolder);
        }
    }

    public void ClearAllCards(Player player, Player opponent, List<CardData> communityCardList, List<CardData> foldedCards,
                              Transform playerHolder, Transform opponentHolder, Transform communityHolder)
    {
        // cards in holders go back to deck
        deckManager.ReturnCards(player.HoleCards);
        deckManager.ReturnCards(opponent.HoleCards);
        deckManager.ReturnCards(communityCardList);
        deckManager.ReturnCards(foldedCards);

        // destroying game objects (UI)
        uiManager.ClearCardHolder(playerHolder);
        uiManager.ClearCardHolder(opponentHolder);
        uiManager.ClearCardHolder(communityHolder);

        // clearing the lists
        player.HoleCards.Clear();
        opponent.HoleCards.Clear();
        communityCardList.Clear();
        foldedCards.Clear();
    }
}