using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GuidebookUI : MonoBehaviour
{
    [Header("Root / Visibility")]
    public GameObject guidebookRoot; // drag Guidebook panel here

    [Header("Rank Text Rows")]
    public TextMeshProUGUI royalFlushText;
    public TextMeshProUGUI straightFlushText;
    public TextMeshProUGUI fourKindText;
    public TextMeshProUGUI fullHouseText;
    public TextMeshProUGUI flushText;
    public TextMeshProUGUI straightText;
    public TextMeshProUGUI tripsText;      // Three of a Kind
    public TextMeshProUGUI twoPairText;
    public TextMeshProUGUI onePairText;
    public TextMeshProUGUI highCardText;

    [Header("Description Box")]
    public TextMeshProUGUI descriptionText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private Dictionary<HandRank, TextMeshProUGUI> rankToText;

    private void Awake()
    {
        // map hand category -> UI row text
        rankToText = new Dictionary<HandRank, TextMeshProUGUI>()
        {
            { HandRank.RoyalFlush,     royalFlushText     },
            { HandRank.StraightFlush,  straightFlushText  },
            { HandRank.FourOfAKind,    fourKindText       },
            { HandRank.FullHouse,      fullHouseText      },
            { HandRank.Flush,          flushText          },
            { HandRank.Straight,       straightText       },
            { HandRank.ThreeOfAKind,   tripsText          },
            { HandRank.TwoPair,        twoPairText        },
            { HandRank.OnePair,        onePairText        },
            { HandRank.HighCard,       highCardText       },
        };
    }

    // Called by GameManager to update highlight/description
    public void Refresh(List<CardData> playerAllCards)
    {
        if (playerAllCards == null || playerAllCards.Count == 0)
        {
            ClearAllHighlights();
            if (descriptionText != null)
                descriptionText.text = "No cards yet.";
            return;
        }

        var result = PokerHandEvaluator.EvaluateBestHand(playerAllCards);
        HighlightRank(result.Rank);

        if (descriptionText != null)
            descriptionText.text = result.Description;
    }

    private void HighlightRank(HandRank activeRank)
    {
        foreach (var kvp in rankToText)
        {
            TextMeshProUGUI t = kvp.Value;
            if (t == null) continue;
            t.color = normalColor;
            t.fontStyle = FontStyles.Normal;
        }

        if (rankToText.TryGetValue(activeRank, out var activeText) && activeText != null)
        {
            activeText.color = highlightColor;
            activeText.fontStyle = FontStyles.Bold;
        }
    }

    private void ClearAllHighlights()
    {
        foreach (var kvp in rankToText)
        {
            TextMeshProUGUI t = kvp.Value;
            if (t == null) continue;
            t.color = normalColor;
            t.fontStyle = FontStyles.Normal;
        }
    }

    //This is the toggle method you¡¦ll call from your Guidebook Menu button
    public void ToggleGuidebook()
    {
        if (guidebookRoot == null) return;

        bool show = !guidebookRoot.activeSelf;
        guidebookRoot.SetActive(show);
        Time.timeScale = show ? 0f : 1f;
    }
}
