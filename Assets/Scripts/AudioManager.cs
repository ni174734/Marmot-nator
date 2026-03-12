using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    [SerializeField] private AudioSource titleMusic;
    [SerializeField] private AudioSource levelMusic;
    private bool isTitleMusicPlaying = false;
    private bool isLevelMusicPlaying = false;

    public void playTitleMusic()
    {
        if (!isTitleMusicPlaying)
        {
            titleMusic.Play();
            isTitleMusicPlaying = true;
        }
    }

    public void playLevelMusic()
    {
        if (!isLevelMusicPlaying)
        {
            levelMusic.Play();
            isLevelMusicPlaying = true;
        }
    }
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
