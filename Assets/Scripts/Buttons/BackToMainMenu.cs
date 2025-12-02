using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip buttonSound;
    public void GotoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        audioSource.PlayOneShot(buttonSound);
    }
}
