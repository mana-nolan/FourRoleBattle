using UnityEngine;

public class StartButtonPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float scaleMultiplier = 1.08f; // 最大倍率
    [SerializeField] private float speed = 2.0f;            // 脈動スピード
    [SerializeField] private bool useUnscaledTime = true;   // Time.timeScale無視

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // 0..1..0 の往復（脈打ち）
        float wave = (Mathf.Sin(t * speed) + 1f) * 0.5f;
        float s = Mathf.Lerp(1f, scaleMultiplier, wave);

        transform.localScale = baseScale * s;
    }

    private void OnDisable()
    {
        // 画面遷移などで無効化された時にサイズが戻るように
        transform.localScale = baseScale;
    }
}
