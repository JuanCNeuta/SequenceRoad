using UnityEngine;
using UnityEngine.UI;

public class AllAudioToggleButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => AudioManager.Instance.ToggleAllAudio());
    }
}
