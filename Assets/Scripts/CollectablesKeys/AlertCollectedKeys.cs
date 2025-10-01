using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlertCollectedKeys : MonoBehaviour
{
    public AudioClip collectSoundTrigger; 
    [SerializeField] private GameObject panelCollectible;
    private CanvasGroup canvasGroupCollectibles;

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

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayRandomSound();
            StartCoroutine(ShowAndHidePanel()); // Mostrar panel y esperar a que desaparezca
        }
    }

    private void PlayRandomSound()
    {
        if (collectSoundTrigger != null)
        {

            AudioManager.Instance?.PlaySound(collectSoundTrigger);
        }
        else
        {
            Debug.LogWarning("No hay sonidos configurados para este coleccionable.");
        }
    }

    private IEnumerator ShowAndHidePanel()
    {
        float fadeDuration = 0.5f;
        float visibleTime = 1.5f;

        // Bloquear el movimiento del personaje
        CollectibleManager.Instance.SetCharacterCanMove(false);

        panelCollectible.SetActive(true);
        canvasGroupCollectibles.alpha = 0f;


        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroupCollectibles.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroupCollectibles.alpha = 1f;


        yield return new WaitForSeconds(visibleTime);

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroupCollectibles.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroupCollectibles.alpha = 0f;
        panelCollectible.SetActive(false);


        gameObject.SetActive(false);

        CollectibleManager.Instance.SetCharacterCanMove(true);
    }
}
