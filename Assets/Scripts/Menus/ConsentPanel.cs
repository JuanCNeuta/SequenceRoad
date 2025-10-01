using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using TMPro;
using LottiePlugin.UI;

public class ConsentPanel : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text consentText;             // Texto de consentimiento en el panel (TMP)
    [TextArea(2, 5)]
    public string consentFullText;           // Texto completo a mostrar

    [Header("Timing")]
    public float typingSpeed = 0.05f;        // segundos entre cada letra
    public float linePause = 0.4f;           // pausa extra cuando encuentra '\n'

    [Header("Animaciones")]
    [SerializeField] private AnimatedImage animationBot;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (consentText == null) Debug.LogWarning("ConsentPanel: consentText no asignado.");
        if (string.IsNullOrWhiteSpace(consentFullText) && consentText != null)
        {
            // No sobrescribimos si StartMenu ya pone un texto; solo fallback.
            consentFullText = consentText.text;
        }

        // Asegurar que el texto empiece vacío hasta animarlo
        if (consentText != null) consentText.text = "";
    }

    /// <summary>
    /// Inicia la animacion de escritura desde el principio y reproduce Lottie de forma segura.
    /// </summary>
    public void StartAnimation()
    { 
        // Reproduce la animacion Lottie de manera segura si está asignada
        if (animationBot != null)
        {
            StartCoroutine(PlayLottieSafe());
        }
        else
        {
            Debug.LogWarning("ConsentPanel: animationBot no asignado en Inspector. Se omitira la animacion Lottie.");
        }

        // Texto: iniciar la "maquina de escribir"
        if (consentText == null) return;
        StopAnimation();
        consentText.text = "";
        typingCoroutine = StartCoroutine(TypeTextCoroutine());
    }

    private IEnumerator PlayLottieSafe()
    {
        // Si el GameObject está inactivo, activarlo para permitir la inicializacion del plugin
        if (!animationBot.gameObject.activeInHierarchy)
        {
            animationBot.gameObject.SetActive(true);
            Debug.Log("ConsentPanel: activando animationBot GameObject para inicializacion.");
        }

        // Esperar 1-2 frames para que Unity ejecute OnEnable/Start del componente Lottie
        yield return null;
        yield return null;

        // Asegurarse de que el componente esté habilitado
        if (!animationBot.enabled)
            animationBot.enabled = true;

        // Intentar Play() protegiendo con try/catch
        try
        {
            animationBot.Play();
            Debug.Log("ConsentPanel: animacion Lottie Play() invocada correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"ConsentPanel: Exception al llamar animationBot.Play(): {ex.GetType().Name}: {ex.Message}\nStack:\n{ex.StackTrace}");
            DumpNullMembers(animationBot);
        }
    }

    private void DumpNullMembers(object obj)
    {
        if (obj == null) return;

        Type t = obj.GetType();
        Debug.LogWarning($"ConsentPanel: Diagnostico del componente {t.FullName}");

        // Campos
        var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var f in fields)
        {
            try
            {
                object val = f.GetValue(obj);
                if (val == null)
                    Debug.LogWarning($"  Field null: {f.Name} ({f.FieldType.Name})");
            }
            catch { /* ignore */ }
        }

        // Propiedades (solo gettable)
        var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var p in props)
        {
            if (!p.CanRead) continue;
            try
            {
                if (p.GetIndexParameters().Length > 0) continue;
                object val = p.GetValue(obj);
                if (val == null)
                    Debug.LogWarning($"  Property null: {p.Name} ({p.PropertyType.Name})");
            }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// Detiene la animacion (sin revelar el texto).
    /// </summary>
    public void StopAnimation()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    /// <summary>
    /// Revela inmediatamente el texto completo.
    /// </summary>
    public void RevealFullText()
    {
        StopAnimation();
        if (consentText != null) consentText.text = consentFullText;
    }

    private IEnumerator TypeTextCoroutine()
    {
        if (consentText == null) yield break;
        if (string.IsNullOrEmpty(consentFullText)) yield break;

        foreach (char c in consentFullText)
        {
            consentText.text += c;

            if (c == '\n')
            {
                yield return new WaitForSecondsRealtime(linePause);
            }
            else
            {
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
        }

        typingCoroutine = null;
    }
}
