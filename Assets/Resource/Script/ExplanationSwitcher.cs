using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExplanationSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    private int currentIndex = 0;

    private void Start()
    {
        ShowPage(0);
    }

    public void NextPage()
    {
        if (currentIndex >= pages.Length - 1) return;
        currentIndex++;
        ShowPage(currentIndex);
    }

    public void PrevPage()
    {
        // ★ 1ページ目ならMainSceneへ戻る
        if (currentIndex == 0)
        {
            SceneManager.LoadScene("MainScene");
            return;
        }

        currentIndex--;
        ShowPage(currentIndex);
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }

        prevButton.interactable = true; // 1ページ目でも押せるようにする
        nextButton.interactable = index < pages.Length - 1;
    }
}
