using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GuidebookUI : MonoBehaviour
{
    [Header("Rank Text Rows")]
    public TextMeshProUGUI royalFlushText;
    public TextMeshProUGUI straightFlushText;
    public TextMeshProUGUI fourKindText;
    public TextMeshProUGUI fullHouseText;
    public TextMeshProUGUI flushText;
    public TextMeshProUGUI straightText;
    public TextMeshProUGUI tripsText;
    public TextMeshProUGUI twoPairText;
    public TextMeshProUGUI onePairText;
    public TextMeshProUGUI highCardText;

    private Dictionary<HandRank, TextMeshProUGUI> rankToText;

    private void Awake()
    {
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

    public void Refresh(List<CardData> playerAllCards)
    {
        if (playerAllCards == null || playerAllCards.Count == 0)
        {
            HideAllRanks();
            return;
        }

        var result = PokerHandEvaluator.EvaluateBestHand(playerAllCards);
        ShowOnlyRank(result.Rank);
    }

    private void ShowOnlyRank(HandRank activeRank)
    {
        foreach (var kvp in rankToText)
        {
            TextMeshProUGUI t = kvp.Value;
            if (t == null) continue;

            t.gameObject.SetActive(kvp.Key == activeRank);
        }
    }

    private void HideAllRanks()
    {
        foreach (var kvp in rankToText)
        {
            TextMeshProUGUI t = kvp.Value;
            if (t == null) continue;
            t.gameObject.SetActive(false);
        }
    }
}