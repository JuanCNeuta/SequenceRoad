using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System;

public class LoopBlock : MonoBehaviour, IDropHandler
{
    public static event Action OnBlockDroppedInLoopSlot;

    public TMP_Dropdown repeatDropdown; // Men� desplegable en lugar de InputField
    public Transform directionBlockSlot;
    public AudioClip[] numberSounds;  // Lista de sonidos de los n�meros

    // Propiedad que devuelve el n�mero de repeticiones seg�n el Dropdown
    public int RepeatCount => repeatDropdown.value + 1; // Dropdown empieza en 0, sumamos 1

    void Start()
    {
    }

    public GameObject GetConnectedDirectionBlock()
    {
        if (directionBlockSlot.childCount > 0)
        {
            return directionBlockSlot.GetChild(0).gameObject;
        }
        return null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            GameObject droppedObject = eventData.pointerDrag;

            // Solo permitir bloques de direcci�n en el slot del bucle
            if (IsValidDirectionBlock(droppedObject))
            {
                AddBlockToSlot(droppedObject);
                droppedObject.SetActive(false);
                Debug.Log("Bloque de direcci�n a�adido al slot del bucle.");

                OnBlockDroppedInLoopSlot?.Invoke();
                Debug.Log("LoopBlock: OnBlockDroppedInLoopSlot disparado.");
            }
            else
            {
                Debug.LogWarning("El objeto soltado no es un bloque de direcci�n v�lido.");
            }
        }
    }

    private void AddBlockToSlot(GameObject droppedObject)
    {
        // Eliminar cualquier bloque existente para evitar m�ltiples
        foreach (Transform child in directionBlockSlot)
        {
            Destroy(child.gameObject);
        }

        GameObject blockCopy = Instantiate(droppedObject, directionBlockSlot);

        RectTransform rectTransform = blockCopy.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(45, 50); // Ajuste de tama�o del bloque
            rectTransform.localScale = Vector3.one;
        }

        CanvasGroup canvasGroup = blockCopy.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(directionBlockSlot.GetComponent<RectTransform>());
    }

    private bool IsValidDirectionBlock(GameObject block)
    {
        // Verificar si el bloque tiene uno de los tags v�lidos
        return block.CompareTag("Up") || block.CompareTag("Down") || block.CompareTag("Left") || block.CompareTag("Right");
    }

    public void ResetLoopBlock()
    {
        // Restablecer el Dropdown al valor inicial (1)
        if (repeatDropdown != null)
        {
            repeatDropdown.value = 0; // "1" es la primera opci�n en el Dropdown
        }

        // Eliminar cualquier bloque de direcci�n asociado
        foreach (Transform child in directionBlockSlot)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("Bloque de bucle reiniciado.");
    }

    public int GetRepeatCount()
    {
        return repeatDropdown.value + 1;
    }
}