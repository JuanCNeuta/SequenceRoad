using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AccessibilityManager : MonoBehaviour
{
    public static AccessibilityManager Instance;

    [Header("Modo Blanco y Negro")]
    [SerializeField] private Volume grayscaleVolume; // Para control del color de la pantalla
    private bool isBWMode = false;

    [Header("Vibraci�n")]
    private bool isVibrationEnabled = true;

    private Dictionary<string, bool> arrowStates = new Dictionary<string, bool>();

    private void Awake()
    {
        // Patr�n Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Alterna el modo blanco y negro activando o desactivando un panel superpuesto.
    public void ToggleBlackAndWhite()
    {
        isBWMode = !isBWMode;

        // Activa/desactiva el efecto de escala de grises si el Volume est� asignado
        if (grayscaleVolume != null)
            grayscaleVolume.gameObject.SetActive(isBWMode);
        else
            Debug.LogWarning("Falta asignar el Volume 'grayscaleVolume' en el Inspector.");
    }

    // Alterna el estado de vibraci�n para activarla o desactivarla.
    public void ToggleVibration()
    {
        isVibrationEnabled = !isVibrationEnabled;
        Vibrate();
        Debug.Log("Vibraci�n " + (isVibrationEnabled ? "activada" : "desactivada"));
    }

    // Ejecuta una vibraci�n si est� habilitada. Funciona solo en dispositivos Android.
    public void Vibrate()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        if (isVibrationEnabled)
        {
            Handheld.Vibrate();
        }
    #else
        if (isVibrationEnabled)
        {
            Debug.Log("Vibraci�n (simulada en editor)");
        }
    #endif
    }

    public void SetArrowState(string key, bool value)
    {
        arrowStates[key] = value;
    }

    public bool GetArrowState(string key)
    {
        return arrowStates.ContainsKey(key) && arrowStates[key];
    }

    // Devuelve el estado actual del modo Blanco&Negro
    public bool GetBlackAndWhiteState()
    {
        return isBWMode;
    }

    // Devuelve el estado actual de vibraci�n
    public bool GetVibrationState()
    {
        return isVibrationEnabled;
    }

    // Aplica el estado BW a un Volume local
    public void ApplyBlackAndWhiteTo(UnityEngine.Rendering.Volume vol)
    {
        if (vol == null) return;
        vol.gameObject.SetActive(isBWMode);
    }

    // Devuelve si una flecha concreta est� activa
    public bool TryGetArrowState(string key, out bool value)
    {
        if (string.IsNullOrEmpty(key)) { value = false; return false; }
        if (arrowStates.TryGetValue(key, out value)) return true;
        value = false;
        return false;
    }

}