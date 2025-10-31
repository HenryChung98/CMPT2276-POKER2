using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void GotoGame()
    {
        SceneManager.LoadScene("GamePlay");
    }

    public void GotoTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
