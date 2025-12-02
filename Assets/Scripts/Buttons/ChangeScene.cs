using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip buttonSound;
    public void GotoGame()
    {
        audioSource.PlayOneShot(buttonSound);
        SceneManager.LoadScene("GamePlay");
    }

    public void GotoTutorial()
    {
        audioSource.PlayOneShot(buttonSound);
        SceneManager.LoadScene("Tutorial");
    }
}
