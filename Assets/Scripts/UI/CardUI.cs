using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("UI Components")]
    public Image cardImage;

    [Header("Appearance")]
    public Sprite cardBackSprite;

    private bool isFaceDown = false;

    public void Setup(CardData data, bool faceDown = false)
    {
        cardData = data;
        isFaceDown = faceDown;
        Refresh();
    }

    public void SetFaceDown(bool faceDown)
    {
        isFaceDown = faceDown;
        Refresh();
    }

    private void Refresh()
    {
        if (isFaceDown)
        {
            if (cardImage != null && cardBackSprite != null)
                cardImage.sprite = cardBackSprite;
        }
        else
        {
            if (cardImage != null && cardData.cardImage != null)
                cardImage.sprite = cardData.cardImage;
        }
    }
}