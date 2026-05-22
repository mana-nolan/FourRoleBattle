using UnityEngine;

public class CardSelectFrame : MonoBehaviour
{
    [SerializeField] private GameObject selectFrame; // SelectFrame‚ð“ü‚ê‚é

    private void Awake()
    {
        if (selectFrame == null)
        {
            Transform t = transform.Find("SelectFrame");
            if (t != null) selectFrame = t.gameObject;
        }

        SetSelected(false);
    }

    public void SetSelected(bool on)
    {
        if (selectFrame != null) selectFrame.SetActive(on);
    }
}