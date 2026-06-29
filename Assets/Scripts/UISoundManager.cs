using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip clickSound;
    public AudioClip errorSound;
    public AudioClip dosSound;

    public void PlayClick()
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void PlayError()
    {
        audioSource.PlayOneShot(errorSound);
    }
    public void PlayDos()
    {
        audioSource.PlayOneShot(dosSound);
    }
}