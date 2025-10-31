using UnityEngine;
using UnityEngine.UI;


// you can add more states or rename them if you want.
enum TutorialState
{
    first,
    second,
    third,
    last,
}
public class TutorialManager : MonoBehaviour
{
    private TutorialState currentState = TutorialState.first;
    public Image[] speechBubbles; // create and add speech bubble at the inspector as you need

    private void Start()
    {
        foreach (var bubble in speechBubbles)
        {
            bubble.gameObject.SetActive(false);
        }
        speechBubbles[0].gameObject.SetActive(true);

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
