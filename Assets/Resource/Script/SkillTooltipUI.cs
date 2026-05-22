using UnityEngine;
using TMPro;

public class SkillTooltipUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;

    private void Awake()
    {
        HideImmediate();
    }

    private void OnDisable()
    {
        HideImmediate();
    }

    public void Show(string skillName, string description)
    {
        if (rootObject == null) return;

        if (skillNameText != null)
            skillNameText.text = skillName;

        if (skillDescriptionText != null)
            skillDescriptionText.text = description;

        rootObject.SetActive(true);
    }

    public void Hide()
    {
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (skillNameText != null)
            skillNameText.text = "";

        if (skillDescriptionText != null)
            skillDescriptionText.text = "";

        if (rootObject != null)
            rootObject.SetActive(false);
    }
}