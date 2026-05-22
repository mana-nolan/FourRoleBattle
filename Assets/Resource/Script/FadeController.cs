using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fadeImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private bool fadeInOnStart = true;

    [Header("Editor Convenience")]
    [SerializeField] private bool hideFadeImageWhenNotFading = true; // ★普段は隠す

    private bool isTransitioning = false;

    private void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError("[FadeController] fadeImage is not assigned.");
            return;
        }

        // ★普段は見えない＆クリックも邪魔しない
        fadeImage.raycastTarget = false;
        SetAlpha(0f);

        if (hideFadeImageWhenNotFading)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (fadeImage == null) return;

        if (fadeInOnStart)
        {
            // ★フェードインする時だけ表示する
            if (hideFadeImageWhenNotFading)
                fadeImage.gameObject.SetActive(true);

            SetAlpha(1f);
            StartCoroutine(Fade(1f, 0f, fadeInDuration, blockRaycast: false));
        }
    }

    public void LoadMainScene()
    {
        LoadSceneWithFade("MainScene");
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning) return;
        if (fadeImage == null) return;

        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;

        if (hideFadeImageWhenNotFading)
            fadeImage.gameObject.SetActive(true);

        // フェードアウト：透明→黒
        yield return Fade(0f, 1f, fadeOutDuration, blockRaycast: true);

        SceneManager.LoadScene(sceneName);

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to, float duration, bool blockRaycast)
    {
        fadeImage.raycastTarget = blockRaycast;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, duration <= 0f ? 1f : t / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);

        // ★透明になったら普段は隠す
        if (to <= 0.001f)
        {
            fadeImage.raycastTarget = false;

            if (hideFadeImageWhenNotFading)
                fadeImage.gameObject.SetActive(false);
        }
    }

    private void SetAlpha(float a)
    {
        var c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
