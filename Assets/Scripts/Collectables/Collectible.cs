using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class Collectible : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip[] collectSounds; // Lista de sonidos

    [Header("UI Panel")]
    [SerializeField] private GameObject panelCollectible;
    private CanvasGroup canvasGroupCollectibles;

    [Header("Points Settings")]
    [SerializeField] private float amountPoints;
    [SerializeField] private Score score;
    [SerializeField] private TextMeshProUGUI pointsText;

    private void Start()
    {
        if (panelCollectible == null)
        {
            Debug.LogError("panelCollectible NO está asignado en el Inspector.");
            return;
        }

        // Obtener o agregar CanvasGroup
        canvasGroupCollectibles = panelCollectible.GetComponent<CanvasGroup>();
        if (canvasGroupCollectibles == null)
        {
            Debug.LogWarning("No se encontró CanvasGroup en el panelCollectible. Se agregará automáticamente.");
            canvasGroupCollectibles = panelCollectible.AddComponent<CanvasGroup>();
        }

        // Inicializar como oculto
        canvasGroupCollectibles.alpha = 0f;
        panelCollectible.SetActive(false);

        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.RegisterCollectible(gameObject);
        }
        else
        {
            Debug.LogWarning("CollectibleManager no está configurado.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayRandomSound();
        score.AddPoints(amountPoints);
        StartCoroutine(ShowAndHidePanel((int)amountPoints)); // Lanzar animación con puntos
    }

    private void PlayRandomSound()
    {
        if (collectSounds != null && collectSounds.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, collectSounds.Length);
            AudioManager.Instance?.PlaySound(collectSounds[randomIndex]);
        }
        else
        {
            Debug.LogWarning("No hay sonidos configurados para este coleccionable.");
        }
    }

    private IEnumerator ShowAndHidePanel(int points)
    {
        float fadeDuration = 0.5f;
        float visibleTime = 1.5f;
        float countDuration = 2f; // Duración del conteo animado

        // Bloquear el movimiento del personaje
        CollectibleManager.Instance.SetCharacterCanMove(false);

        // Preparar panel
        panelCollectible.SetActive(true);
        canvasGroupCollectibles.alpha = 0f;

        
        float elapsed = 0f;
        // 1) Fade in del panel
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroupCollectibles.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroupCollectibles.alpha = 1f;

        // 2) Animación de conteo de puntos
        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            int display = Mathf.RoundToInt(Mathf.Lerp(0, points, elapsed / countDuration));
            pointsText.text = $"+{display}";
            float scale = 1f + 0.2f * Mathf.Sin((elapsed / countDuration) * Mathf.PI);
            pointsText.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        // Asegurar valor final y escala normal
        pointsText.text = $"+{points}";
        pointsText.rectTransform.localScale = Vector3.one;
        // 3) Tiempo visible fijo
        yield return new WaitForSeconds(visibleTime);

        // 4) Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroupCollectibles.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroupCollectibles.alpha = 0f;
        panelCollectible.SetActive(false);

        // 5) Desactivar y permitir movimiento
        CollectibleManager.Instance?.Collect(gameObject);
        gameObject.SetActive(false);
        CollectibleManager.Instance.SetCharacterCanMove(true);
    }
}
