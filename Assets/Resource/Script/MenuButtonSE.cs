using UnityEngine;

public class MenuButtonSE : MonoBehaviour
{
    [Header("Button SE")]
    [SerializeField] private AudioClip buttonSe;

    public void PlayButtonSE()
    {
        if (AudioManager.Instance == null) return;
        if (buttonSe == null) return;

        AudioManager.Instance.PlaySE(buttonSe);
    }
}