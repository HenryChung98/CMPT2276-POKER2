using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private int currentPage = 1;
    private const int first = 1;
    private const int last = 8;

    public Image handRankImage;
    public DIalougeBox dialogueBox;
    public Image speechBubble;
    public Transform[] objectTransforms;

    private void Start()
    {
        RenderTutorial();
    }
    private void RenderTutorial()
    {
        handRankImage.gameObject.SetActive(false);
        dialogueBox.UpdateDialogue(currentPage);

        switch (currentPage)
        {
            case 1:
                Debug.Log("welcome");
                break;
            case 2:
                Debug.Log("second page");
                break;
            case 3:
                Debug.Log("third page");
                break;
            case 4:
                Debug.Log("fourth page");
                handRankImage.gameObject.SetActive(true);
                break;
            case 5:
                Debug.Log("fifth page");
                break;
            case 6:
                Debug.Log("sixth page");
                break;
            case 7:
                Debug.Log("seventh page");
                break;
            case 8:
                Debug.Log("eight page");
                break;
        }
        //speechBubble.transform.position = objectTransforms[currentPage - 1].position;
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
            Debug.Log("Already at last state: " + currentPage);
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
            Debug.Log("Already at first state: " + currentPage);
        }
    }
}
