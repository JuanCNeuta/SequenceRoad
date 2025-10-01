using UnityEngine;
using UnityEngine.UI;

public class VibrateToggleButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => AccessibilityManager.Instance.ToggleVibration());
    }
}
