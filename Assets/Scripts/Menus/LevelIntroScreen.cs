using System.Collections;
using UnityEngine;
using TMPro;

public class LevelIntroScreen : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panel;
    public CanvasGroup canvasGroup;
    public TMP_Text titleText;      // Titulo del nivel
    public TMP_Text objectiveText;  // Texto del objetivo
    public TMP_Text loadingText;    // Para "CARGANDO..."

    [Header("Contenido")]
    [TextArea(2, 4)]
    public string levelTitle;
    [TextArea]
    public string objective;

    [Header("Animación y timing")]
    public float typingSpeed = 0.05f;  // Segundos entre cada letra
    public float linePause = 1.0f;   // Pausa tras salto de línea en el título
    public float displayDuration = 1.0f;   // Pausa tras cada bloque completo
    public float dotSpeed = 0.5f;   // Tiempo entre cada punto al cargar
    public float fadeDuration = 0.4f;   // Fade del panel

    [Header("Timer")]
    public Timer timerComponent;

    [Header("Intro Audio")]
    [SerializeField] private AudioClip levelAudio;

    private void Start()
    {
        // Desactivar timer hasta que acabe la intro
        if (timerComponent) timerComponent.enabled = false;

        // Preparar UI
        panel.SetActive(false);
        canvasGroup.alpha = 0f;
        titleText.text = "";
        objectiveText.text = "";
        loadingText.text = "";

        StartCoroutine(ShowIntro());
    }

    private IEnumerator ShowIntro()
    {
        AudioManager.Instance.StopSFX();

        // Fade In
        panel.SetActive(true);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        AudioManager.Instance.PlaySound(levelAudio);

        // — Máquina de escribir título (incluye \n) —
        foreach (char c in levelTitle)
        {
            titleText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);

            if (c == '\n')
                yield return new WaitForSecondsRealtime(linePause);
        }
        yield return new WaitForSecondsRealtime(displayDuration);

        // — Máquina de escribir objetivo —
        foreach (char c in objective)
        {
            objectiveText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        yield return new WaitForSecondsRealtime(displayDuration);

        // — Escribir “CARGANDO” base —
        const string baseLoad = "CARGANDO";
        loadingText.text = "";
        foreach (char c in baseLoad)
        {
            loadingText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        // — Animar puntos en bucle —
        float elapsed = 0f;
        int dotCount = 0;
        while (elapsed < displayDuration)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = baseLoad + new string('.', dotCount);
            yield return new WaitForSecondsRealtime(dotSpeed);
            elapsed += dotSpeed;
        }

        // Fade Out
        t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        panel.SetActive(false);

        // Reactivar timer y empezar el nivel
        if (timerComponent) timerComponent.enabled = true;
    }
}

