using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum AIAction { Call, Raise, Fold }

public class AIController
{
    private readonly float delay = 0.6f;


    public AIAction MakeDecision()
    {
        AIAction action = AIAction.Call;


        return action;
    }
}


