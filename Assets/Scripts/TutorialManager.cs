using UnityEngine;
using UnityEngine.UI;


// you can add more states or rename them if you want.
enum TutorialState
{
    first,
    second,
    third,
    fourth,
    fifth,
    sixth,
    seventh,
    eight,
    last,
}
public class TutorialManager : MonoBehaviour
{
    private TutorialState currentState = TutorialState.first;
    public Image[] speechBubbles; // create and add speech bubble at the inspector as you need
    public Image handRankImage;

    private void Start()
    {
        foreach (var bubble in speechBubbles)
        {
            bubble.gameObject.SetActive(false);
        }
        speechBubbles[0].gameObject.SetActive(true);
        handRankImage.gameObject.SetActive(false);

    }
    private void RenderTutorial()
    {
        // Deactivate all speech bubbles first
        foreach (var bubble in speechBubbles)
        {
            bubble.gameObject.SetActive(false);
        }

        // Activate only the speech bubble based on the current TutorialState.
        switch (currentState)
        {
            case TutorialState.first:
                Debug.Log("welcome");
                speechBubbles[0].gameObject.SetActive(true);
                break;

            case TutorialState.second:
                Debug.Log("second page");
                speechBubbles[1].gameObject.SetActive(true);
                break;

            case TutorialState.third:
                Debug.Log("third page");
                speechBubbles[2].gameObject.SetActive(true);
                break;
            case TutorialState.fourth:
                Debug.Log("fourth page");
                speechBubbles[3].gameObject.SetActive(true);
                handRankImage.gameObject.SetActive(true);
                break;
            case TutorialState.fifth:
                Debug.Log("fifth page");
                speechBubbles[4].gameObject.SetActive(true);
                break;
            case TutorialState.sixth:
                Debug.Log("sixth page");
                speechBubbles[5].gameObject.SetActive(true);
                break;
            case TutorialState.seventh:
                Debug.Log("seventh page");
                speechBubbles[6].gameObject.SetActive(true);
                break;
            case TutorialState.eight:
                Debug.Log("eight page");
                speechBubbles[7].gameObject.SetActive(true);
                break;

        }
    }

    public void NextState()
    {
        if (currentState < TutorialState.last)
        {
            currentState++;
            RenderTutorial();
        }
        else
        {
            Debug.Log("Already at last state: " + currentState);
        }
    }

    public void PreviousState()
    {
        if (currentState > TutorialState.first)
        {
            currentState--;
            RenderTutorial();
        }
        else
        {
            Debug.Log("Already at first state: " + currentState);
        }
    }
}
