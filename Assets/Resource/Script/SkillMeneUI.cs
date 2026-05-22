using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillMenuUI : MonoBehaviour
{
    public enum SkillType
    {
        None = 0,
        SwapRandom = 1,
        PeekRandom = 2, // ★追加：相手手札ランダム1枚を見る
    }

    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot; // SkillMenuPanel 自身でもOK

    [Header("Buttons")]
    [SerializeField] private Button swapSkillButton;
    [SerializeField] private Button peekSkillButton;   // ★追加
    [SerializeField] private Button cancelButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI titleText;

    public event Action<SkillType> OnSkillSelected;
    public event Action OnCanceled;

    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        if (swapSkillButton != null)
        {
            swapSkillButton.onClick.RemoveAllListeners();
            swapSkillButton.onClick.AddListener(() => OnClickSkill(SkillType.SwapRandom));
        }

        if (peekSkillButton != null)
        {
            peekSkillButton.onClick.RemoveAllListeners();
            peekSkillButton.onClick.AddListener(() => OnClickSkill(SkillType.PeekRandom));
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnClickCancel);
        }
    }

    public void Open(int remainingUses)
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = $"スキルを選んでください（残り {remainingUses} 回）";
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        if (swapSkillButton != null) swapSkillButton.interactable = interactable;
        if (peekSkillButton != null) peekSkillButton.interactable = interactable;
        if (cancelButton != null) cancelButton.interactable = interactable;
    }

    private void OnClickSkill(SkillType skill)
    {
        OnSkillSelected?.Invoke(skill);
    }

    private void OnClickCancel()
    {
        OnCanceled?.Invoke();
    }
}