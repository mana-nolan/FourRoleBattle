using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    [Header("Menu BGM")]
    [SerializeField] private AudioClip menuBgm;

    private void Start()
    {
        if (AudioManager.Instance == null) return;
        if (menuBgm == null) return;

        AudioManager.Instance.PlayBGM(menuBgm);
    }
}