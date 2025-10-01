using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    private GameObject character;
    public float moveDistance = 1f;
    public int maxBlocks = 5;
    private List<GameObject> stackedBlocks = new List<GameObject>();

    private CanvasGroup canvasGroup;

    // Contadores de movimientos
    private int correctMoves = 0;
    private int incorrectMoves = 0;
    private int totalMoves = 0;

    // Referencias al texto de la UI (TextMeshProUGUI)
    public TextMeshProUGUI correctMovesText;
    public TextMeshProUGUI incorrectMovesText;
    public TextMeshProUGUI totalMovesText;

    //Dialogo para cuando se choca el personaje
    [SerializeField] private GameObject obstacleMessageObject;

    //Audio cuando choca el personaje
    public AudioClip touchObstaculeSound; // Sonido toma del bloque

    // Propiedades de s�lo lectura para exponer los contadores
    public int CorrectMoves => correctMoves;
    public int IncorrectMoves => incorrectMoves;
    public int TotalMoves => totalMoves;

    // Evento que se disparar� cuando el personaje se mueva exitosamente
    public static event Action OnCharacterMovedSuccessfully;

    //Sonido cuando se añade un ficha al item slot
    public AudioClip soundAddItemSlot;


    void Start()
    {
        //Se obtiene al personaje de donde se ubico en mazeGenerator
        character = FindFirstObjectByType<MazeGenerator>()?.character;

        if (character == null)
        {
            Debug.LogError("No se encontr� el personaje en la escena.");
        }

        // Inicializar transparencia y suscribirse a eventos de DragDrop
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        DragDrop.OnDragStart += DarkenSlot;
        DragDrop.OnDragEnd += ResetSlot;

        UpdateCountersUI();
    }

    private void OnDestroy()
    {
        DragDrop.OnDragStart -= DarkenSlot;
        DragDrop.OnDragEnd -= ResetSlot;
        OnCharacterMovedSuccessfully = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");

        PlaySoundAddItemSlot();

        if (eventData.pointerDrag != null && character != null)
        {
            GameObject droppedObject = eventData.pointerDrag;

            // Validar si es un bloque de movimiento o de repetici�n
            bool isValidBlock =
                droppedObject.TryGetComponent<LoopBlock>(out LoopBlock loopBlock) ||
                droppedObject.CompareTag("Up") ||
                droppedObject.CompareTag("Down") ||
                droppedObject.CompareTag("Left") ||
                droppedObject.CompareTag("Right");

            if (!isValidBlock)
            {
                Debug.Log("Objeto soltado no es v�lido. Se ignora.");
                return;
            }

            AddBlockToStack(droppedObject);
            droppedObject.SetActive(false);

            if (loopBlock != null)
            {
                ExecuteLoopBlock(loopBlock);
                loopBlock.ResetLoopBlock();
            }
            else
            {
                ExecuteDirectionBlock(droppedObject);
            }
        }
    }

    void AddBlockToStack(GameObject droppedObject)
    {

        GameObject blockCopy = Instantiate(droppedObject, transform);
        RectTransform rectTransform = blockCopy.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // Centrado
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            // Si es un LoopBlock, forzar la escala exacta deseada
            if (blockCopy.GetComponent<LoopBlock>() != null)
            {
                blockCopy.transform.localScale = new Vector3(0.75f, 0.75f, 0.9f);
            }
            else
            {
                blockCopy.transform.localScale = Vector3.one;
            }
        }

        stackedBlocks.Add(blockCopy);

        if (stackedBlocks.Count > 3)
        {
            GameObject oldest = stackedBlocks[0];
            stackedBlocks.RemoveAt(0);
            Destroy(oldest);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    void ExecuteLoopBlock(LoopBlock loopBlock)
    {
        GameObject directionBlock = loopBlock.GetConnectedDirectionBlock();

        if (directionBlock != null)
        {
            string direction = IdentifyDirectionFromTag(directionBlock);
            if (direction != null)
            {
                Vector3 moveDirection = GetDirectionVector(direction);
                StartCoroutine(ExecuteLoopWithDelay(moveDirection, direction, loopBlock.GetRepeatCount()));
            }
        }
    }


    private IEnumerator ExecuteLoopWithDelay(Vector3 moveDirection, string direction, int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            // Esperar hasta que se permita el movimiento
            while (!CollectibleManager.Instance.CanCharacterMove())
            {
                yield return null; // Espera un frame hasta que el panel desaparezca
            }

            TryMove(moveDirection, direction);
            yield return new WaitForSeconds(1f);
        }
    }

    void ExecuteDirectionBlock(GameObject directionBlock)
    {
        string direction = IdentifyDirectionFromTag(directionBlock);
        if (direction != null)
        {
            Vector3 moveDirection = GetDirectionVector(direction);
            TryMove(moveDirection, direction);
        }
    }

    string IdentifyDirectionFromTag(GameObject draggedObject)
    {
        switch (draggedObject.tag)
        {
            case "Up": return "arriba";
            case "Down": return "abajo";
            case "Left": return "izquierda";
            case "Right": return "derecha";
            default: return null;
        }
    }

    Vector3 GetDirectionVector(string direction)
    {
        switch (direction)
        {
            case "arriba": return Vector3.forward;
            case "abajo": return Vector3.back;
            case "izquierda": return Vector3.left;
            case "derecha": return Vector3.right;
            default: return Vector3.zero;
        }
    }

    void TryMove(Vector3 direction, string directionName)
    {
        if (character == null || !CollectibleManager.Instance.CanCharacterMove()) return;

        Vector3 targetPosition = character.transform.position + direction * moveDistance;
        int layerMask = ~LayerMask.GetMask("Collectibles");

        if (Physics.Raycast(character.transform.position, direction, moveDistance, layerMask))
        {
            Debug.Log($"No se puede mover {directionName}, hay un obst�culo.");
            incorrectMoves++;

            //Animacion de colisi�n
            PlayObstaculeAnimation();

            //Sonido de colisi�n
            PlayObstaculeSound();

            // Vibraci�n accesible (solo si est� activada)
            AccessibilityManager.Instance?.Vibrate();

            // Mostrar mensaje de obst�culo
            ShowObstacleMessage();

            // Hacer que el personaje mire hacia la c�mara
            LookAtCamera();
        }
        else
        {
            // Activar animaci�n de caminar
            CharacterMovement characterMovement = character.GetComponent<CharacterMovement>();
            if (characterMovement != null)
            {
                characterMovement.MoveCharacter(direction);
            }

            StartCoroutine(SmoothMove(character.transform.position, targetPosition, 0.5f));
            correctMoves++;
        }

        totalMoves++;
        UpdateCountersUI();
    }


    public void ResetCounters()
    {
        correctMoves = 0;
        incorrectMoves = 0;
        totalMoves = 0;
        UpdateCountersUI();
    }

    public void ClearArrows()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        stackedBlocks.Clear();
    }

    void UpdateCountersUI()
    {
        correctMovesText.text = $"Correctos: {correctMoves}";
        incorrectMovesText.text = $"Incorrectos: {incorrectMoves}";
        totalMovesText.text = $"Total: {totalMoves}";
    }

    private void DarkenSlot()
    {
        //canvasGroup.alpha = 0.9f;
    }

    private void ResetSlot()
    {
        //canvasGroup.alpha = 1f;
    }

    //Corutina para realizar el movimiento de manera gradual y no aparecer alla
    private IEnumerator SmoothMove(Vector3 start, Vector3 end, float duration)
{
    float elapsedTime = 0f;
    CharacterMovement characterMovement = character.GetComponent<CharacterMovement>();

    while (elapsedTime < duration)
    {
        character.transform.position = Vector3.Lerp(start, end, elapsedTime / duration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    character.transform.position = end; // Asegura que llega exactamente al destino

    // Detener la animaci�n al finalizar el movimiento
    if (characterMovement != null)
    {
        characterMovement.StopCharacter();
    }

        // Invocar el evento SOLO cuando el movimiento del personaje es exitoso y ha terminado
        OnCharacterMovedSuccessfully?.Invoke();
    }

    // MOSTRAR MENSAJE CUANDO EL PERSONAJE SE GOLPEA CON ALGO
    void ShowObstacleMessage()
    {
        if (obstacleMessageObject == null || Camera.main == null) return;

        // Offset base
        float yOffset = 1.0f;
        float xOffset = -0.5f;

        // Obtener índice de la escena actual
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Ajuste solo para niveles MazeLevel (no tutoriales)
        if (sceneIndex == 2) yOffset += 0.3f;   // MazeLevel1
        else if (sceneIndex == 4) yOffset += 1.7f; // MazeLevel2
        else if (sceneIndex == 5) yOffset += 2.0f; // MazeLevel3
        else if (sceneIndex == 6) yOffset += 2.2f; // MazeLevel4

        // Offset final
        Vector3 worldOffset = new Vector3(xOffset, yOffset, 0f);

        // Posición en el mundo relativa al personaje
        Vector3 worldPosition = character.transform.position + worldOffset;

        // Convertir a pantalla y luego a coordenadas locales del canvas
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
        RectTransform canvasRect = obstacleMessageObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        RectTransform messageRect = obstacleMessageObject.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, Camera.main, out Vector2 localPoint))
        {
            messageRect.anchoredPosition = localPoint;
        }

        obstacleMessageObject.SetActive(true);
        Invoke(nameof(HideObstacleMessage), 2f);
    }

    void HideObstacleMessage()
    {
        if (obstacleMessageObject != null)
        {
            obstacleMessageObject.SetActive(false);
        }
    }

    //MIRAR HACIA LA CAMARA
    void LookAtCamera()
    {
        if (Camera.main == null) return;
        Vector3 cameraPosition = Camera.main.transform.position;
        cameraPosition.y = character.transform.position.y; 

        character.transform.LookAt(cameraPosition);
    }

    //SONIDO CUANDO TOCA UN OBSTACULO
    private void PlayObstaculeSound()
    {
        CharacterAudio characterAudio = character.GetComponent<CharacterAudio>();
        if (characterAudio != null)
        {
            characterAudio.PlayTouchObstacleSound();
        }
        else
        {
            Debug.LogWarning("El personaje no tiene asignado un componente CharacterAudio.");
        }
    }

    private void PlayObstaculeAnimation()
    {
        // Activar animaci�n de saltar feliz
        if (character.TryGetComponent<CharacterMovement>(out var characterMovement))
        {
            characterMovement.PlayObstaculeAnimation(2f);
        }
        Debug.Log("�animacion de obstaculo activada!");
        
    }

    private void PlaySoundAddItemSlot()
    {
        if (soundAddItemSlot != null)
        {
            AudioManager.Instance?.PlaySound(soundAddItemSlot);
        }
        else
        {
            Debug.LogWarning("No hay sonido asignado para colisión en " + gameObject.name);
        }
    }

}



