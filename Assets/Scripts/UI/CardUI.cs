using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("UI Components")]
    public Image cardImage;
    public Image highlightBorder; // For highlighting hand

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

    public void Highlight(bool enable, Color highlightColor)
    {
        if (highlightBorder != null)
        {
            highlightBorder.enabled = enable;
            highlightBorder.color = highlightColor;
        }
        else if (cardImage != null)
        {
            if (enable)
            {
                cardImage.color = highlightColor;
            }
            else
            {
                cardImage.color = Color.white;
            }
        }
    }

    //Overload
    public void Highlight(bool enable)
    {
        Highlight(enable, Color.yellow);
    }
}