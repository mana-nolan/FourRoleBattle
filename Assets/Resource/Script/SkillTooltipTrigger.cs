using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Refs")]
    [SerializeField] private SkillTooltipUI tooltipUI;

    [Header("Tooltip Text")]
    [SerializeField] private string skillName;

    [TextArea(2, 6)]
    [SerializeField] private string description;

    private bool isHovering = false;

    private void OnDisable()
    {
        isHovering = false;

        if (tooltipUI != null)
            tooltipUI.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (tooltipUI == null) return;
        tooltipUI.Show(skillName, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (tooltipUI == null) return;
        tooltipUI.Hide();
    }
}