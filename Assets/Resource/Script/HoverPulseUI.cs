using UnityEngine;
using UnityEngine.EventSystems;

public class HoverPulseUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pulse Settings")]
    [SerializeField] private float scaleMultiplier = 1.08f;
    [SerializeField] private float speed = 2.5f;

    private Vector3 baseScale;
    private bool isHovering = false;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (!isHovering) return;

        float wave = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(1f, scaleMultiplier, wave);
        transform.localScale = baseScale * scale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        transform.localScale = baseScale; // Œ³‚É–ß‚·
    }
}
