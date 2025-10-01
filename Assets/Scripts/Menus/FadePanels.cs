using UnityEngine;
using System.Collections;

public class FadePanels: MonoBehaviour
{
    public static FadePanels Instance { get; private set; }

    private void Awake()
    {
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

    public IEnumerator TransitionPanels(CanvasGroup panelOut, CanvasGroup panelIn, float duration = 0.5f)
    {
        // Asegurar que el panel entrante esté activo
        panelIn.gameObject.SetActive(true);

        // Fade out del panel saliente y fade in del panel entrante
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            panelOut.alpha = Mathf.Lerp(1f, 0f, t);
            panelIn.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // Asegurar valores finales
        panelOut.alpha = 0f;
        panelIn.alpha = 1f;

        // Desactivar el panel saliente
        panelOut.gameObject.SetActive(false);
    }
}
