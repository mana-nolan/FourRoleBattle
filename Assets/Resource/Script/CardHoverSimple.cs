using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverSimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform visual;
    [SerializeField] private float liftAmount = 25f;
    [SerializeField] private float speed = 10f;

    private Vector3 originalLocalPos;
    private bool hovering;

    private void Start()
    {
        if (visual == null)
        {
            Transform t = transform.Find("Visual");
            if (t != null) visual = t.GetComponent<RectTransform>();
        }

        if (visual == null)
        {
            Debug.LogError("[CardHoverSimple] Visual is NULL. Assign child Visual RectTransform.");
            enabled = false;
            return;
        }

        originalLocalPos = visual.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovering = true;
    public void OnPointerExit(PointerEventData eventData) => hovering = false;

    private void Update()
    {
        if (visual == null) return;

        Vector3 target = hovering
            ? originalLocalPos + Vector3.up * liftAmount
            : originalLocalPos;

        visual.localPosition = Vector3.Lerp(visual.localPosition, target, Time.unscaledDeltaTime * speed);
    }
}