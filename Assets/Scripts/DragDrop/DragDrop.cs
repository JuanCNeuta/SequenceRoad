using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    public AudioClip touchDragSound; // Sonido toma del bloque
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public static event Action OnDragStart; // Evento para iniciar arrastre
    public static event Action OnDragEnd; // Evento para finalizar arrastre
    public static event Action OnObjectDiscarded; // Evento para verificar si se solto en item slot

    private Transform originalParent;
    private int originalSiblingIndex;
    private Transform dragLayer; // contenedor donde lo pondremos durante el drag

    //Sonido final de arrastre
    public AudioClip dragDropEndSound;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Busca un GameObject llamado "DragLayer" en el canvas
        dragLayer = GameObject.Find("DragLayer")?.transform;
        if (dragLayer == null)
        {
            Debug.LogWarning("No se encontró DragLayer en la escena.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");

        // Reproduce el sonido
        PlayTouchSound();

        // GUARDAR VALOR DEL DROPDOWN
        int repeatValue = 0;
        LoopBlock originalLoop = GetComponent<LoopBlock>();
        if (originalLoop != null)
        {
            repeatValue = originalLoop.repeatDropdown.value;
        }

        // CREAR COPIA Y POSICIONAR
        GameObject newObject = Instantiate(gameObject, transform.parent);
        newObject.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;

        // ASIGNAR EL MISMO VALOR AL NUEVO
        LoopBlock newLoop = newObject.GetComponent<LoopBlock>();
        if (newLoop != null)
        {
            newLoop.repeatDropdown.value = repeatValue;
        }

        OnDragStart?.Invoke();

        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0.9f, 0.1f));
        canvasGroup.blocksRaycasts = false;

        // Reproducir sonido del número (usa valor guardado)
        if (originalLoop != null)
        {
            StartCoroutine(PlayLoopSounds(originalLoop));
        }

        // Mover el objeto al DragLayer temporalmente para que esté encima
        if (dragLayer != null)
        {
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetParent(dragLayer, true); // true mantiene la posición mundial
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");

        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0.6f, 1f, 0.1f));

        canvasGroup.blocksRaycasts = true;

        // Notificar que el arrastre ha terminado
        OnDragEnd?.Invoke();

        // Restaurar al padre original
        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        // Verificar si el objeto fue soltado en un `ItemSlot`
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<ItemSlot>() == null)
        {
            Destroy(gameObject);
            Debug.Log("Objeto destruido porque no se soltó en un ItemSlot.");

            // Se invoca al nuevo evento para saber que no se solto donde deberia 
            OnObjectDiscarded?.Invoke();
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    private void PlayTouchSound()
    {
        if (touchDragSound != null)
        {
            AudioManager.Instance?.PlaySound(touchDragSound);
        }
        else
        {
            Debug.LogWarning("No hay un sonido configurado para la ficha.");
        }
    }

    private IEnumerator PlayLoopSounds(LoopBlock loopBlock)
    {
        if (loopBlock == null)
        {
            Debug.LogError("LoopBlock es nulo en PlayLoopSounds.");
            yield break;
        }

        yield return new WaitForSeconds(0.8f);

        int repeatCount = loopBlock.GetRepeatCount();

        if (loopBlock.numberSounds == null || loopBlock.numberSounds.Length == 0)
        {
            Debug.LogError("numberSounds no está asignado o está vacío.");
            yield break;
        }

        if (repeatCount > 0 && repeatCount <= loopBlock.numberSounds.Length)
        {
            AudioManager.Instance?.PlaySound(loopBlock.numberSounds[repeatCount - 1]);
            yield return new WaitForSeconds(0.5f);
        }

        GameObject directionBlock = loopBlock.directionBlockSlot.childCount > 0 ? loopBlock.directionBlockSlot.GetChild(0).gameObject : null;
        if (directionBlock != null)
        {
            DragDrop directionDragDrop = directionBlock.GetComponent<DragDrop>();
            directionDragDrop?.PlayTouchSound();
        }
    }

}

