using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Clips")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        if (musicSource.clip == menuMusic) return;
        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayLevelMusic()
    {
        if (musicSource.clip == levelMusic) return;
        musicSource.clip = levelMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}
