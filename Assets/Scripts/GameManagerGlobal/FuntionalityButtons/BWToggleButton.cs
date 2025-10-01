using UnityEngine;
using UnityEngine.UI;

public class BWToggleButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => AccessibilityManager.Instance.ToggleBlackAndWhite());
    }
}
