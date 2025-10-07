using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("UI Components")]
    public Image cardImage;
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI suitText;

    public void Setup(CardData data)
    {
        cardData = data;

        if (cardImage != null && cardData.cardImage != null)
            cardImage.sprite = cardData.cardImage;

        if (rankText != null)
            rankText.text = cardData.rank.ToString();

        if (suitText != null)
            suitText.text = cardData.suit.ToString();

    }

}
