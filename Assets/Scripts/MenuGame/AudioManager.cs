using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null) return;

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayHover()
    {
        if (hoverSound != null)
            sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayClick()
    {
        if (clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }
}