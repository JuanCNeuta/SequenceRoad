using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class NivelItem : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Image icon;       // para candado o icono desbloqueado
    private Button button;

    void Awake() => button = GetComponent<Button>();

    public void Setup(int nivel, bool locked, UnityAction onClick)
    {
        levelText.text = nivel.ToString();
        icon.enabled = locked;
        button.interactable = !locked;
        button.onClick.RemoveAllListeners();
        if (!locked) button.onClick.AddListener(onClick);
    }
}
