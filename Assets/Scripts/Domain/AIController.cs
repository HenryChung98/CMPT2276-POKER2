using UnityEngine;

public enum AIAction { Call, Raise, Fold }

public class AIController
{
    private const float delay = 0.6f;
    private readonly Player actor;
    AIAction action;

    public AIController(Player actor)
    {
        this.actor = actor;
    }

    public AIAction MakeDecision()
    {
        action = AIAction.Call;


        return action;
    }
}


