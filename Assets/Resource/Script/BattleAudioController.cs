using UnityEngine;

public class BattleAudioController : MonoBehaviour
{
    [Header("Battle BGM")]
    [SerializeField] private AudioClip battleBgm;

    private void Start()
    {
        if (AudioManager.Instance == null) return;
        if (battleBgm == null) return;

        AudioManager.Instance.PlayBGM(battleBgm);
    }
}