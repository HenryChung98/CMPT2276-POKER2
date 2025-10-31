using UnityEngine;

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

    private void RenderTutorial()
    {
        switch (currentState)
        {
            case TutorialState.first:
                Debug.Log("welcome");
                break;

            case TutorialState.second:
                Debug.Log("second page");
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
