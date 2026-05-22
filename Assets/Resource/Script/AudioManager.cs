using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Default BGM")]
    [SerializeField] private AudioClip defaultBgm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (defaultBgm != null)
        {
            PlayBGM(defaultBgm);
        }
    }

    // ----------------------------
    // BGM
    // ----------------------------
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null || clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource == null) return;
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    // ----------------------------
    // SE
    // ----------------------------
    public void PlaySE(AudioClip clip, float volumeScale = 1f)
    {
        if (seSource == null || clip == null) return;
        seSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void SetSEVolume(float volume)
    {
        if (seSource == null) return;
        seSource.volume = Mathf.Clamp01(volume);
    }
}