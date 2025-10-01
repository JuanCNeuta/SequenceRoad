using UnityEngine;
using UnityEngine.Rendering;

public class AccesibilityInitializer : MonoBehaviour
{
    [Header("Toggles")]
    public ArrowToggle musicToggle;
    public ArrowToggle soundsToggle;
    public ArrowToggle voiceToggle;
    public ArrowToggle blackWhiteToggle;
    public ArrowToggle vibrateToggle;

    [Header("Volume local")]
    public Volume grayscaleVolume;

    private void Start()
    {
        // 1) Registrar Volume local en AccessibilityManager (si existe)
        if (AccessibilityManager.Instance != null && grayscaleVolume != null)
        {
            // Usa el método ApplyBlackAndWhiteTo para aplicar el estado actual
            AccessibilityManager.Instance.ApplyBlackAndWhiteTo(grayscaleVolume);
        }

        // 2) Restaurar flechas/toggles locales (si hay UI en la escena)
        RestoreArrowIfAssigned(musicToggle);
        RestoreArrowIfAssigned(soundsToggle);
        RestoreArrowIfAssigned(voiceToggle);
        RestoreArrowIfAssigned(blackWhiteToggle);
        RestoreArrowIfAssigned(vibrateToggle);

        if (AccessibilityManager.Instance != null)
            Debug.Log($"[AccesibilityInitializer] Vibration state: {AccessibilityManager.Instance.GetVibrationState()}");
    }

    private void RestoreArrowIfAssigned(ArrowToggle toggle)
    {
        if (toggle == null || AccessibilityManager.Instance == null) return;

        string key = toggle.StateKey;

        bool state = AccessibilityManager.Instance.GetArrowState(key);
        
        toggle.SetArrow(state);
    }
}

