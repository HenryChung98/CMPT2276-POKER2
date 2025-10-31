using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public static ChangeScene Instance { get; private set; }

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

        public void GotoGame()
    {
        SceneManager.LoadScene("GamePlay");
    }

    public void GotoTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void GotoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
