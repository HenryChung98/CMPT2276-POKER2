using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    
    [Header("UI Elements")]
    public TextMeshProUGUI potText;
    public TextMeshProUGUI playerBankText;
    public TextMeshProUGUI oppBankText;

    [Header("Card Prefab")]
    public GameObject cardPrefab;
    public Sprite cardBackSprite;

    [Header("Buttons")]
    public Button callButton;
    public Button raiseButton;
    public Button foldButton;

    [Header("Transforms")]
    public Transform deckTransform;

    [Header("Player & Opponent Cards")]
    public List<CardUI> playerCards = new List<CardUI>();
    public List<CardUI> opponentCards = new List<CardUI>();

    [Header("Win Probability")]
    public TextMeshProUGUI winProbabilityText;
    private WinProbabilityCalculator probabilityCalculator;



    public void UpdateMoneyUI(int pot, int playerChips, int opponentChips)
    {
        if (potText != null) potText.text = $"Pot: {pot}";
        if (playerBankText != null) playerBankText.text = $"{playerChips}";
        if (oppBankText != null) oppBankText.text = $"{opponentChips}";
    }

    public void DisplayCard(CardData cardData, Transform holder, bool faceDown = false) // One more argument(Player player = null)
    {
        // cards go from deck to holder
        GameObject cardObject = Instantiate(cardPrefab, deckTransform);
        var ui = cardObject.GetComponent<CardUI>();
        ui.cardBackSprite = cardBackSprite;
        ui.Setup(cardData, faceDown);

        /*
        if (player != null)
        {
            RegisterCard(player, ui);
        }*/

        StartCoroutine(MoveCardToHolder(cardObject, holder));
    }

    public void ClearCardHolder(Transform holder)
    {
        for (int i = holder.childCount - 1; i >= 0; i--)
        {
            Destroy(holder.GetChild(i).gameObject);
        }
    }

    public void RevealCards(Transform holder)
    {
        for (int i = 0; i < holder.childCount; i++)
        {
            var ui = holder.GetChild(i).GetComponent<CardUI>();
            ui.SetFaceDown(false);
        }
    }

    public void UpdateButtonStates(int activePlayerIndex, List<Player> allPlayers)
    {
        // if at least one player all-in, nobody is allowed to raise
        bool anyoneAllIn = false;
        foreach (var player in allPlayers)
        {
            if (player.HasAllIn)
            {
                anyoneAllIn = true;
                break;
            }
        }

        if (activePlayerIndex == 0) {
            callButton.interactable = true && !gameManager.isAnimating;
            raiseButton.interactable = true && !gameManager.isAnimating && !anyoneAllIn;
            foldButton.interactable = true && !gameManager.isAnimating && !anyoneAllIn;
        }
        else {
            callButton.interactable = false;
            raiseButton.interactable = false;
            foldButton.interactable = false;
        }
    }

    // animation - move card from deck to holder
    private IEnumerator MoveCardToHolder(GameObject card, Transform holder)
    {
        Vector3 startPos = card.transform.position;

        card.transform.SetParent(holder);
        Canvas.ForceUpdateCanvases();
        yield return null; // Layout Group needs time to calculate (wait 1 frame)

        Vector3 endPos = card.transform.position;
        card.transform.position = startPos;

        float duration = 0.5f;
        float elapsed = 0f;

        // animation
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            card.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        card.transform.position = endPos;
    }




    public void RegisterCard(Player player, CardUI cardUI)
    {
        if (player.IsHuman)
            playerCards.Add(cardUI);
        else
            opponentCards.Add(cardUI);
    }


    public void ClearAllHighlight()
    {
        foreach (var card in playerCards)
            card.Highlight(false);

        foreach (var card in opponentCards)
            card.Highlight(false);
    }

    public void HighlightHand(Player player, PokerHandEvaluator.HandResult result, bool showKickers)
    {
        List<CardUI> hand = player.IsHuman ? playerCards : opponentCards;
        if (hand == null || result == null)
            return;

        //check flush suit for this hand (if the result is a flush-type)
        Suit? flushSuit = null;
        if (result.Rank == HandRank.Flush ||
            result.Rank == HandRank.StraightFlush ||
            result.Rank == HandRank.RoyalFlush)
        {
            var suitCounts = new Dictionary<Suit, int>();

            foreach (var cardUI in hand)
            {
                if (cardUI == null || cardUI.cardData == null) continue;

                if (result.highlightedRank != null &&
                    result.highlightedRank.Contains(cardUI.cardData.rank))
                {
                    var s = cardUI.cardData.suit;
                    if (!suitCounts.ContainsKey(s))
                        suitCounts[s] = 0;

                    suitCounts[s]++;
                }
            }

            if (suitCounts.Count > 0)
            {
                //suit that appears most often among the highlighted ranks ¡÷ the flush suit
                flushSuit = suitCounts
                    .OrderByDescending(kv => kv.Value)
                    .First().Key;
            }
        }


        foreach (var cardUI in hand)
        {
            if (cardUI == null || cardUI.cardData == null)
            {
                continue;
            }

            bool isMainHand;

            //For flush-type hands: require BOTH rank match & correct suit
            if (flushSuit.HasValue &&
                (result.Rank == HandRank.Flush ||
                 result.Rank == HandRank.StraightFlush ||
                 result.Rank == HandRank.RoyalFlush))
            {
                isMainHand =
                    result.highlightedRank != null &&
                    result.highlightedRank.Contains(cardUI.cardData.rank) &&
                    cardUI.cardData.suit == flushSuit.Value;
            }
            else
            {
                //for all other hands, rank-only is fine (pairs, trips, etc.)
                isMainHand =
                    result.highlightedRank != null &&
                    result.highlightedRank.Contains(cardUI.cardData.rank);
            }


            //kicker cards (only when showKickers == true)
            bool isKicker = showKickers &&
                            result.highlightedKickers != null &&
                            result.highlightedKickers.Contains(cardUI.cardData.rank);

            if (isMainHand)
            {
                cardUI.Highlight(true, Color.green);
            }
            else if (isKicker)
            {
                cardUI.Highlight(true, Color.yellow);
            }
            else
            {
                cardUI.Highlight(false);
            }
        }
    }
    public void InitializeProbabilityCalculator(DeckManager deckManager)
    {
        probabilityCalculator = new WinProbabilityCalculator(deckManager);
    }

    public void UpdateWinProbability(List<CardData> playerHoleCards, List<CardData> communityCards)
    {
        if (winProbabilityText == null || probabilityCalculator == null) return;

        // Only calculate if we have hole cards
        if (playerHoleCards.Count < 2)
        {
            winProbabilityText.text = "Win Chance: --";
            return;
        }

        float winProb = probabilityCalculator.CalculateWinProbability(
            playerHoleCards,
            communityCards,
            simulations: 1000  
        );

        winProbabilityText.text = $"Win Chance: {winProb:P0}"; // Shows as "45%"

        
        if (winProb >= 0.7f)
            winProbabilityText.color = Color.green;
        else if (winProb >= 0.4f)
            winProbabilityText.color = Color.yellow;
        else
            winProbabilityText.color = Color.red;
    }

}