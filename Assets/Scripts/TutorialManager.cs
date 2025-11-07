using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // messages
    [SerializeField]
    [TextArea]
    private List<string> dialogueLines;

    // page
    private int currentPage = 0;
    private const int first = 0;
    private const int last = 7;

    [Header("UI Elements")]
    public Transform[] objectTransforms;
    public Image speechBubble;
    public TextMeshProUGUI speechBubbleText;
    public Image handRankImage;

    private void Start()
    {
        RenderTutorial();
    }
    private void RenderTutorial()
    {
        handRankImage.gameObject.SetActive(false);
        speechBubbleText.SetText(dialogueLines[currentPage]);

        switch (currentPage)
        {
            case 0:
                Debug.Log("welcome");
                break;
            case 1:
                Debug.Log("second page");
                break;
            case 2:
                Debug.Log("third page");
                break;
            case 3:
                Debug.Log("fourth page");
                handRankImage.gameObject.SetActive(true);
                break;
            case 4:
                Debug.Log("fifth page");
                break;
            case 5:
                Debug.Log("sixth page");
                break;
            case 6:
                Debug.Log("seventh page");
                break;
            case 7:
                Debug.Log("eight page");
                break;
        }
        speechBubble.transform.position = objectTransforms[currentPage].position;
    }

    public void NextState()
    {
        if (currentPage < last)
        {
            currentPage++;
            RenderTutorial();
        }
        else
        {
            Debug.Log("Already at last page");
        }
    }

    public void PreviousState()
    {
        if (currentPage > first)
        {
            currentPage--;
            RenderTutorial();
        }
        else
        {
            Debug.Log("Already at first page");
        }
    }
}
