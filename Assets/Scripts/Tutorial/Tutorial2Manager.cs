using LottiePlugin.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System; 
using System.Collections;
using TMPro; 

public class Tutorial2Manager : MonoBehaviour
{
    private CanvasGroup winPanelCanvasGroup; 

    [System.Serializable]
    public class TutorialStep
    {
        public GameObject targetBlockObject;        // El bot�n del bloque en la paleta (para el selector/flecha)
        public GameObject expectedBlockButton;      // El bot�n que el usuario debe arrastrar

        public GameObject targetItemSlotObject;

        public GameObject highlightOnDragStartTarget;

        public Vector2 selectorRelativeOffset = new Vector2(0f, 0f);
        public Vector2 arrowRelativeOffset = new Vector2(0f, 0f);
        public bool isArrowPointingLeft = false;
        public bool isArrowPointingUp = false;

        public Vector2 itemSlotAnimationOffset = new Vector2(0f, 0f); // Offset para el Lottie del slot

        public RectTransform TargetRectTransform =>
            targetBlockObject != null ? targetBlockObject.GetComponent<RectTransform>() : null;

        public RectTransform TargetItemSlotRectTransform =>
            targetItemSlotObject != null ? targetItemSlotObject.GetComponent<RectTransform>() : null;

        public RectTransform HighlightOnDragStartRectTransform =>
            highlightOnDragStartTarget != null ? highlightOnDragStartTarget.GetComponent<RectTransform>() : null;

        [Header("Ghost Block para este paso")]
        public GameObject ghostBlockPrefab;

        [Tooltip("El GameObject (RectTransform) que define la posici�n final del bloque fantasma.")]
        public GameObject ghostEndTargetObject;

        public RectTransform GhostEndTargetRectTransform =>
            ghostEndTargetObject != null ? ghostEndTargetObject.GetComponent<RectTransform>() : null;

        [Header("Audio Settings")]
        public AudioClip stepAudioClip;


    }

    public RawImage animationSelector;
    public RawImage animationArrow;
    public RawImage animationSlotEffect; // Efecto visual en el ItemSlot

    [Header("Panel de Victoria del Tutorial")]
    [SerializeField] private GameObject gameWinLevelPanel; // Este es el panel que mostrar� la victoria
    [SerializeField] private TextMeshProUGUI winSessionNameText;

    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("Bloques de Movimiento")]
    public List<GameObject> allMovementBlockButtons;

    [Header("Animaciones de Victoria del Tutorial")]
    [SerializeField] private AnimatedImage animationConfeti;
    [SerializeField] private AnimatedImage animationTrophy;
    [SerializeField] private AnimatedImage animationHappy;
    [SerializeField] private AnimatedImage animationThumb;

    [Header("Tutorial Ficha Fantasma")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.4f); // Color para el efecto descolorido
    public float ghostMoveDuration = 2.0f; // Duraci�n del movimiento del fantasma
    public float ghostWaitDuration = 2.0f; // Tiempo de espera antes de repetir la animaci�n
    public Transform ghostParentTransform; // El Canvas o RectTransform padre donde se instanciar�n los fantasmas

    private Coroutine ghostAnimationCoroutine; // Para controlar la corrutina del fantasma
    private GameObject currentGhostInstance; // La instancia del fantasma actualmente activa
  
    private int currentStepIndex = 0;
    private bool allStepsCompleted = false; // Flag para saber si los pasos est�n listos

    public GameObject stackBlock;

    [Header("Audio Configuration")]
    public float delayBeforeSteps = 2f;

    //Sonido cuando se gana el tutorial
    [Header("Audio Win Tutorial")]
    public AudioClip winTutorial;

    void Awake()
    {
        // Inicializar el CanvasGroup del panel de victoria del tutorial
        if (gameWinLevelPanel != null)
        {
            winPanelCanvasGroup = gameWinLevelPanel.GetComponent<CanvasGroup>();
            if (winPanelCanvasGroup == null)
            {
                winPanelCanvasGroup = gameWinLevelPanel.AddComponent<CanvasGroup>();
                Debug.LogWarning("Tutorial2Manager: 'gameWinLevelPanel' no ten�a CanvasGroup. Se a�adi� uno.");
            }
            winPanelCanvasGroup.alpha = 0f;
            gameWinLevelPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Tutorial2Manager: 'gameWinLevelPanel' no est� asignado en el Inspector. El panel de victoria no se mostrar�.");
        }

        if (currentGhostInstance != null)
        {
            Destroy(currentGhostInstance);
            currentGhostInstance = null;
        }
    }

    void OnEnable()
    {
        ItemSlot.OnCharacterMovedSuccessfully += AdvanceStep;
        DragDrop.OnDragStart += OnDragStartHandler;
        DragDrop.OnDragEnd += OnDragEndHandler;
        DragDrop.OnObjectDiscarded += OnDraggedObjectDiscarded;
        LoopBlock.OnBlockDroppedInLoopSlot += OnBlockDroppedInLoopSlotHandler;

        // Suscribirse al evento del coleccionable
        CollectableTutorial.OnCharacterTouchCollectable += OnCollectableTouchedAndTutorialDone;
    }

    void OnDisable()
    {
        ItemSlot.OnCharacterMovedSuccessfully -= AdvanceStep;
        DragDrop.OnDragStart -= OnDragStartHandler;
        DragDrop.OnDragEnd -= OnDragEndHandler;
        DragDrop.OnObjectDiscarded -= OnDraggedObjectDiscarded;
        LoopBlock.OnBlockDroppedInLoopSlot -= OnBlockDroppedInLoopSlotHandler;

        // Desuscribirse del evento del coleccionable
        CollectableTutorial.OnCharacterTouchCollectable -= OnCollectableTouchedAndTutorialDone;
    }

    void Start()
    {
        // Desactivar stackBlock (ItemSlot principal) Inicialmente
        if (stackBlock != null)
        {
            Debug.Log($"<color=cyan>Tutorial2Manager: Intentando inicializar 'stackBlock': {stackBlock.name}.</color>");
            CanvasGroup canvasGroup = stackBlock.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false; // Desactiva la detecci�n de raycasts
                canvasGroup.blocksRaycasts = false;    // Desactiva la interactividad
                canvasGroup.alpha = 0.5f;            // Hace el objeto semitransparente
                Debug.Log("<color=red>Tutorial2Manager: 'stackBlock' (ItemSlot principal) BLOQUEADO y semitransparente al inicio (CanvasGroup encontrado y modificado).</color>");
            }
            else
            {
                Debug.LogWarning("Tutorial2Manager: 'stackBlock' NO tiene un componente CanvasGroup adjunto. No se puede bloquear al inicio.");
            }
        }
        else
        {
            Debug.LogWarning("Tutorial2Manager: 'stackBlock' no est� asignado en el Inspector. No se puede bloquear al inicio.");
        }

        if (animationSlotEffect != null)
        {
            animationSlotEffect.gameObject.SetActive(false);
            Debug.Log("<color=green>Tutorial2Manager: animationSlotEffect desactivado en Start().</color>");
        }

        // Reiniciar el flag de pasos completados
        allStepsCompleted = false;

        // Reproducir audio inicial y luego mostrar el primer paso
        StartCoroutine(InitializeTutorial());


    }

    // Este m�todo se llamar� cuando se toque CUALQUIER coleccionable
    private void OnCollectableTouchedAndTutorialDone()
    {
        Debug.Log("<color=yellow>Tutorial2Manager: Evento Collectible.OnCharacterTouchCollectable recibido.</color>");

        // Solo si todos los pasos del tutorial han sido completados, activamos el panel de victoria
        if (allStepsCompleted)
        {
            Debug.Log("<color=lime>Tutorial2Manager: Todos los pasos completados y coleccionable tocado. Iniciando panel de victoria.</color>");

            //DETENER ANIMACI�N DEL FANTASMA AL FINALIZAR 
            StopGhostAnimation();

            // Desactiva los elementos del tutorial
            animationSelector.gameObject.SetActive(false);
            animationArrow.gameObject.SetActive(false);
            if (animationSlotEffect != null) animationSlotEffect.gameObject.SetActive(false);
            EnableAllMovementBlocks(); // Asegura que todos los bloques est�n interactuables al final.
            EnableStackBlock(); // Asegura que el stackBlock est� habilitado al final.
            this.enabled = false; // Desactiva este script de tutorial.

            StartCoroutine(FadeWinLevelPanelAndPlayAnimations(0f)); // 
        }
        else
        {
            Debug.Log("<color=yellow>Tutorial2Manager: Coleccionable tocado, pero los pasos del tutorial a�n no han finalizado.</color>");
        }
    }

    private void OnDragStartHandler()
    {
        Debug.Log("<color=purple>Tutorial2Manager: DragDrop.OnDragStart detectado.</color>");

        // PAUSAR LA ANIMACI�N DEL FANTASMA AL INICIAR UN ARRASTRE
        StopGhostAnimation();

        if (currentStepIndex < steps.Count)
        {
            RectTransform targetRectToHighlight = null;
            Vector2 currentOffset = Vector2.zero;

            if (currentStepIndex == 1 && stackBlock != null)
            {
                targetRectToHighlight = stackBlock.GetComponent<RectTransform>();

                Debug.Log("<color=purple>Tutorial2Manager: Resaltando 'stackBlock' para el paso 1 (arrastre de LoopBlock).</color>");
            }
            else if (steps[currentStepIndex].HighlightOnDragStartRectTransform != null)
            {
                targetRectToHighlight = steps[currentStepIndex].HighlightOnDragStartRectTransform;
                currentOffset = steps[currentStepIndex].itemSlotAnimationOffset;
                Debug.Log("<color=purple>Tutorial2Manager: Usando HighlightOnDragStartRectTransform para animaci�n de slot.</color>");
            }
            else if (steps[currentStepIndex].TargetItemSlotRectTransform != null)
            {
                targetRectToHighlight = steps[currentStepIndex].TargetItemSlotRectTransform;
                currentOffset = steps[currentStepIndex].itemSlotAnimationOffset;
                Debug.Log("<color=purple>Tutorial2Manager: Usando TargetItemSlotRectTransform para animaci�n de slot (HighlightOnDragStartRectTransform es nulo).</color>");
            }

            if (targetRectToHighlight != null)
            {
                SetItemSlotSelectorActive(true, targetRectToHighlight, currentOffset);
                RestartSlotLottieAnimation();
                Debug.Log("<color=purple>Tutorial2Manager: animationSlotEffect ACTIVADO y reiniciado en OnDragStartHandler (inicio de drag) sobre: " + targetRectToHighlight.name + ".</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>Tutorial2Manager: Ni HighlightOnDragStartRectTransform ni TargetItemSlotRectTransform ni stackBlock son v�lidos para el paso {currentStepIndex}. No se iluminar� el objetivo de arrastre.</color>");
                SetItemSlotSelectorActive(false);
            }
        }
    }

    private void OnDragEndHandler()
    {
        Debug.Log("<color=purple>Tutorial2Manager: DragDrop.OnDragEnd detectado.</color>");
    }

    private void OnBlockDroppedInLoopSlotHandler()
    {
        // Asume que el paso 0 del tutorial de bucles es arrastrar una ficha de movimiento al LoopBlock.
        if (currentStepIndex == 0)
        {
            Debug.Log("<color=green>Tutorial2Manager: Bloque soltado en LoopBlock (Paso 0 completado). Avanzando paso.</color>");
            AdvanceStep(); // Avanza al siguiente paso (arrastrar la ficha de bucle al ItemSlot)
        }
        else
        {
            Debug.Log($"<color=orange>Tutorial2Manager: Bloque soltado en LoopBlock pero no es el paso esperado (actual: {currentStepIndex}).</color>");
        }
    }

    private void OnBlockDroppedInItemSlotHandler()
    {
        if (currentStepIndex == 1) // Asume que el paso 1 es arrastrar el LoopBlock al ItemSlot principal.
        {
            Debug.Log("<color=green>Tutorial2Manager: Bloque soltado en ItemSlot (Paso 1 completado). Avanzando paso.</color>");
            AdvanceStep(); // Esto har� que el stackBlock se habilite
        }
        else
        {
            Debug.Log($"<color=orange>Tutorial2Manager: Bloque soltado en ItemSlot pero no es el paso esperado (actual: {currentStepIndex}).</color>");
        }
    }

    // Se llama cuando se completa un paso del tutorial.
    public void AdvanceStep()
    {
        Debug.Log("<color=red>Tutorial2Manager: AdvanceStep() llamado. Intentando DESACTIVAR animationSlotEffect.</color>");

        if (animationSlotEffect != null && animationSlotEffect.gameObject.activeInHierarchy)
        {
            animationSlotEffect.gameObject.SetActive(false);
            Debug.Log("<color=red>animationSlotEffect DESACTIVADA en AdvanceStep. Su estado actual es: " + animationSlotEffect.gameObject.activeInHierarchy + "</color>");
        }
        else if (animationSlotEffect != null)
        {
            Debug.Log("<color=red>animationSlotEffect ya estaba inactivo al llamar AdvanceStep().</color>");
        }
        else
        {
            Debug.LogWarning("<color=red>animationSlotEffect es nulo en AdvanceStep().</color>");
        }

        // DETENER LA ANIMACI�N DEL FANTASMA AL AVANZAR DE PASO
        StopGhostAnimation();
 
        currentStepIndex++;
        Debug.Log($"<color=blue>Avanzando al paso de tutorial: {currentStepIndex}</color>");

        if (currentStepIndex < steps.Count)
        {
            ShowCurrentStep(); // Configura el siguiente paso.

            if (currentStepIndex == 1) // Si el tutorial acaba de avanzar al paso 2
            {
                EnableStackBlock();
            }
        }
        else
        {
            // Todos los pasos del tutorial completados.
            allStepsCompleted = true; // Marca que todos los pasos se completaron

            StopGhostAnimation();

            Debug.Log("<color=green>Tutorial2Manager: �Todos los pasos del tutorial han sido completados! Ahora esperando que se recoja el coleccionable.</color>");

        }
    }

    private void EnableStackBlock()
    {
        if (stackBlock == null)
        {
            Debug.LogWarning("Tutorial2Manager: No se puede habilitar 'stackBlock' porque no est� asignado.");
            return;
        }

        CanvasGroup canvasGroup = stackBlock.GetComponent<CanvasGroup>();
        RawImage rawImage = null;

        if (canvasGroup == null)
        {
            rawImage = stackBlock.GetComponent<RawImage>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            Debug.Log("<color=green>Tutorial2Manager: 'stackBlock' (ItemSlot principal) HABILITADO y opaco.</color>");
        }
        else if (rawImage != null)
        {
            rawImage.raycastTarget = true;
            Color currentColor = rawImage.color;
            currentColor.a = 1f;
            rawImage.color = currentColor;
            Debug.Log("<color=green>Tutorial2Manager: 'stackBlock' (ItemSlot principal) HABILITADO y opaco (usando RawImage).</color>");
        }
        else
        {
            Debug.LogWarning("Tutorial2Manager: 'stackBlock' no tiene CanvasGroup ni RawImage para habilitarlo.");
        }
    }

    private void ShowCurrentStep()
    {
        Debug.Log($"<color=green>Tutorial2Manager: ShowCurrentStep() llamado para el paso {currentStepIndex}.</color>");

        if (currentStepIndex >= steps.Count)
        {
            animationSelector.gameObject.SetActive(false);
            animationArrow.gameObject.SetActive(false);
            if (animationSlotEffect != null) animationSlotEffect.gameObject.SetActive(false);
            EnableAllMovementBlocks();
            return;
        }

        TutorialStep currentStepData = steps[currentStepIndex]; // Definir la variable para usar en el m�todo

        if (currentStepIndex != 1 && steps[currentStepIndex].targetItemSlotObject == null && steps[currentStepIndex].highlightOnDragStartTarget == null)
        {
            SetItemSlotSelectorActive(false);
            Debug.Log($"<color=green>Tutorial2Manager: animationSlotEffect DESACTIVADO porque no hay targetItemSlotObject ni highlightOnDragStartTarget para el paso {currentStepIndex}.</color>");
        }
        else if (currentStepIndex == 1) // Paso 1, donde el objetivo es el stackBlock
        {
            Debug.Log("<color=green>Tutorial2Manager: En el paso 1, el selector del slot ser� activado en OnDragStartHandler.</color>");
        }

        ControlMovementBlockInteractivity(steps[currentStepIndex].expectedBlockButton);
        ShowCurrentStepAnimationsOnly();

        StartGhostAnimation(currentStepData);

        // Reproducir audio del step
        if (steps[currentStepIndex].stepAudioClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(steps[currentStepIndex].stepAudioClip);
        }

    }

    private void ShowCurrentStepAnimationsOnly()
    {
        if (currentStepIndex >= steps.Count) return;

        TutorialStep step = steps[currentStepIndex];
        RectTransform targetBlockRect = step.TargetRectTransform;

        if (targetBlockRect == null)
        {
            animationSelector.gameObject.SetActive(false);
            animationArrow.gameObject.SetActive(false);
            Debug.LogWarning($"Paso {currentStepIndex} no tiene targetBlockObject v�lido. Selector y flecha de paleta DESACTIVADOS.");
            return;
        }

        RectTransform parentCanvasRect = animationSelector.transform.parent.GetComponent<RectTransform>();
        if (parentCanvasRect == null)
        {
            Debug.LogError("Las animaciones Lottie (Selector/Arrow) deben estar dentro de un Canvas o tener un RectTransform padre.");
            animationSelector.gameObject.SetActive(false);
            animationArrow.gameObject.SetActive(false);
            return;
        }

        animationSelector.gameObject.SetActive(true);
        animationArrow.gameObject.SetActive(true);

        Vector2 targetPositionInLottieParentSpace;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvasRect,
            targetBlockRect.position,
            null,
            out targetPositionInLottieParentSpace
        );

        animationSelector.rectTransform.anchoredPosition = targetPositionInLottieParentSpace + step.selectorRelativeOffset;
        float padding = 10f;
        animationSelector.rectTransform.sizeDelta = new Vector2(targetBlockRect.rect.width + padding, targetBlockRect.rect.height + padding);

        Vector2 arrowCalculatedOffset = Vector2.zero;
        if (step.isArrowPointingLeft)
        {
            arrowCalculatedOffset.x = -(targetBlockRect.rect.width / 2f + animationArrow.rectTransform.rect.width / 2f);
        }
        else if (step.isArrowPointingUp)
        {
            arrowCalculatedOffset.y = (targetBlockRect.rect.height / 2f + animationArrow.rectTransform.rect.height / 2f);
        }
        else
        {
            arrowCalculatedOffset.x = (targetBlockRect.rect.width / 2f + animationArrow.rectTransform.rect.width / 2f);
        }
        animationArrow.rectTransform.anchoredPosition = targetPositionInLottieParentSpace + arrowCalculatedOffset + step.arrowRelativeOffset;

        Debug.Log($"<color=blue>Lottie Selector y Arrow ACTIVADOS y posicionados para el bloque de la paleta en el paso {currentStepIndex}.</color>");
    }

    private void ControlMovementBlockInteractivity(GameObject expectedBlockButton)
    {
        if (allMovementBlockButtons == null || allMovementBlockButtons.Count == 0)
        {
            Debug.LogWarning("La lista 'All Movement Block Buttons' no est� asignada o est� vac�a en Tutorial2Manager.");
            return;
        }

        Debug.Log($"<color=purple>--- Controlando interactividad de bloques de la paleta. Esperado para este paso: {expectedBlockButton?.name} ---</color>");

        foreach (GameObject blockButton in allMovementBlockButtons)
        {
            if (blockButton == null) continue;

            CanvasGroup canvasGroup = blockButton.GetComponent<CanvasGroup>();
            RawImage rawImage = null;
            if (canvasGroup == null)
            {
                rawImage = blockButton.GetComponent<RawImage>();
            }

            bool enableThisBlock = (blockButton == expectedBlockButton);

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = enableThisBlock;
                canvasGroup.interactable = enableThisBlock;
                canvasGroup.alpha = enableThisBlock ? 1f : 0.5f;
            }
            else if (rawImage != null)
            {
                rawImage.raycastTarget = enableThisBlock;
                Color currentColor = rawImage.color;
                currentColor.a = enableThisBlock ? 1f : 0.5f;
                rawImage.color = currentColor;
            }

            if (blockButton == expectedBlockButton)
            {
                blockButton.SetActive(true);
            }
        }
    }

    private void EnableAllMovementBlocks()
    {
        if (allMovementBlockButtons == null || allMovementBlockButtons.Count == 0) return;

        foreach (GameObject blockButton in allMovementBlockButtons)
        {
            if (blockButton == null) continue;

            CanvasGroup canvasGroup = blockButton.GetComponent<CanvasGroup>();
            RawImage rawImage = null;
            if (canvasGroup == null)
            {
                rawImage = blockButton.GetComponent<RawImage>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1f;
            }
            else if (rawImage != null)
            {
                rawImage.raycastTarget = true;
                Color currentColor = rawImage.color;
                currentColor.a = 1f;
                rawImage.color = currentColor;
            }
            blockButton.SetActive(true);
        }
    }

    private void SetItemSlotSelectorActive(bool active, RectTransform targetRect = null, Vector2 offset = default(Vector2))
    {
        if (animationSlotEffect == null)
        {
            Debug.LogWarning("animationSlotEffect no est� asignado en Tutorial2Manager.");
            return;
        }

        animationSlotEffect.gameObject.SetActive(active);

        // Asegurarse de que el efecto Lottie no capture eventos de raycast
        if (animationSlotEffect.GetComponent<RawImage>() != null)
        {
            animationSlotEffect.GetComponent<RawImage>().raycastTarget = false;
        }
        else if (animationSlotEffect.GetComponent<CanvasGroup>() != null)
        {
            animationSlotEffect.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        if (active && targetRect != null)
        {
            RectTransform parentCanvasRect = animationSlotEffect.transform.parent.GetComponent<RectTransform>();
            if (parentCanvasRect == null)
            {
                Debug.LogError("El selector de ItemSlot (animationSlotEffect) debe estar dentro de un Canvas o tener un RectTransform padre.");
                return;
            }

            Vector2 targetPositionInLottieParentSpace;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvasRect,
                targetRect.position,
                null,
                out targetPositionInLottieParentSpace
            );

            animationSlotEffect.rectTransform.anchoredPosition = targetPositionInLottieParentSpace + offset;
            float padding = 5f;
            animationSlotEffect.rectTransform.sizeDelta = new Vector2(
                targetRect.rect.width + padding,
                targetRect.rect.height + padding
            );

            RestartSlotLottieAnimation();
        }

    }

    private void OnDraggedObjectDiscarded()
    {
        Debug.Log("<color=red>Tutorial2Manager: Objeto arrastrado descartado detectado. Asegurando que animationSlotEffect est� desactivado.</color>");
        SetItemSlotSelectorActive(false); // Desactiva la animaci�n del slot

        // REINICIAR LA ANIMACI�N DEL FANTASMA SI EL ARRASTRE FALL�
        if (currentStepIndex < steps.Count)
        {
            StartGhostAnimation(steps[currentStepIndex]); // Vuelve a mostrar el fantasma para el paso actual
        }
        
    }

    // M�todo que inicia la coroutine de fade y animaciones
    private IEnumerator FadeWinLevelPanelAndPlayAnimations(float delay)
    {
        yield return new WaitForSeconds(delay); // Espera los segundos especificados

        if (gameWinLevelPanel == null || winPanelCanvasGroup == null)
        {
            Debug.LogError("Tutorial Win Level Panel o CanvasGroup no asignado/encontrado.");
            yield break;
        }

        gameWinLevelPanel.SetActive(true);
        gameWinLevelPanel.transform.SetAsLastSibling(); // Asegura que el panel est� encima de todo
        AudioManager.Instance.PlaySound(winTutorial);

        // L�gica de Fade In
        float duration = 1.5f; // Duraci�n del fade
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            winPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }
        winPanelCanvasGroup.alpha = 1f; // Asegura que se vea completamente

        // L�gica para mostrar el nombre de la sesi�n
        string playerName = "Jugador"; // Valor por defecto
        if (GameSessionManager.Instance != null) // Asume que GameSessionManager es un Singleton
        {
            playerName = GameSessionManager.Instance.PlayerName;
        }
        if (winSessionNameText != null)
        {
            winSessionNameText.text = $"{playerName}!";
        }
        else
        {
            Debug.LogWarning("winSessionNameText no est� asignado en Tutorial2Manager.");
        }
        Debug.Log("Panel de victoria del tutorial mostrado.");
    }

    // M�TODO PARA INICIAR LA ANIMACI�N DEL FANTASMA
    private void StartGhostAnimation(TutorialStep step)
    {
        StopGhostAnimation(); // Asegurarse de detener y destruir cualquier animaci�n anterior

        // Validar que tenemos todas las referencias necesarias para el fantasma
        if (step.ghostBlockPrefab == null || ghostParentTransform == null)
        {
            Debug.LogWarning("No se puede iniciar la animaci�n del fantasma: falta el prefab o el padre.");
            return;
        }

        // Determinar el origen y destino seg�n el paso actual
        RectTransform startTransform = null;
        RectTransform endTransform = null;
        Vector2 customEndPosition = Vector2.zero; // Nueva variable para posici�n personalizada
        bool useCustomEndPosition = false; // Flag para saber si usar posici�n personalizada

        if (currentStepIndex == 0)
        {
            // Paso 0: Desde el bloque de movimiento esperado ? LoopBlock (o targetItemSlotObject)
            startTransform = step.TargetRectTransform; // El bloque de la paleta (ej: mover derecha)
            endTransform = step.GhostEndTargetRectTransform ?? step.TargetItemSlotRectTransform;

            // Configurar posici�n personalizada sin modificar el objeto real
            if (endTransform != null)
            {
                customEndPosition = new Vector2(160, endTransform.anchoredPosition.y);
                useCustomEndPosition = true;
            }
        }
        else if (currentStepIndex == 1)
        {
            // Paso 1: Desde el LoopBlock ? stackBlock (ItemSlot principal)
            startTransform = steps[0].GhostEndTargetRectTransform ?? steps[0].TargetItemSlotRectTransform; // El LoopBlock del paso anterior
            endTransform = stackBlock?.GetComponent<RectTransform>() ?? step.GhostEndTargetRectTransform;
        }
        else
        {
            // Para pasos adicionales, usar la configuraci�n est�ndar
            startTransform = step.TargetRectTransform;
            endTransform = step.GhostEndTargetRectTransform ?? step.TargetItemSlotRectTransform;
        }

        // Validar que tenemos origen y destino v�lidos
        if (startTransform == null || endTransform == null)
        {
            Debug.LogWarning($"No se puede iniciar la animaci�n del fantasma para el paso {currentStepIndex}: falta origen o destino.");
            return;
        }

        // Instanciar el prefab del fantasma
        currentGhostInstance = Instantiate(step.ghostBlockPrefab, ghostParentTransform);
        RectTransform ghostRectTransform = currentGhostInstance.GetComponent<RectTransform>();
        Image ghostImage = currentGhostInstance.GetComponent<Image>();

        if (ghostRectTransform != null && ghostImage != null)
        {
            ghostImage.color = ghostColor; // Aplicar el color y transparencia definidos
            ghostImage.raycastTarget = false; // No debe interferir con la interacci�n

            // Iniciar la corrutina de animaci�n con posici�n personalizada si es necesario
            ghostAnimationCoroutine = StartCoroutine(MoveGhostBlock(
                ghostRectTransform,
                startTransform,
                endTransform,
                useCustomEndPosition ? customEndPosition : Vector2.zero,
                useCustomEndPosition
            ));

            string endPositionInfo = useCustomEndPosition ? $"posici�n personalizada ({customEndPosition.x}, {customEndPosition.y})" : endTransform.name;
            Debug.Log($"<color=cyan>Tutorial2Manager: Animaci�n del fantasma iniciada para el paso {currentStepIndex} desde {startTransform.name} hacia {endPositionInfo}.</color>");
        }
        else
        {
            Debug.LogWarning("El prefab del bloque fantasma no tiene un componente Image o RectTransform. Destruyendo instancia.");
            Destroy(currentGhostInstance);
            currentGhostInstance = null;
        }
    }

    // M�TODO PARA DETENER Y DESTRUIR LA INSTANCIA DEL FANTASMA
    private void StopGhostAnimation()
    {
        if (ghostAnimationCoroutine != null)
        {
            StopCoroutine(ghostAnimationCoroutine);
            ghostAnimationCoroutine = null;
        }
        if (currentGhostInstance != null)
        {
            Destroy(currentGhostInstance); // Destruye la instancia del fantasma de la escena
            currentGhostInstance = null;
        }
        Debug.Log("<color=cyan>Tutorial2Manager: Animaci�n del fantasma detenida y limpiada.</color>");
    }

    // CORRUTINA PARA ANIMAR EL MOVIMIENTO DEL FANTASMA
    private IEnumerator MoveGhostBlock(RectTransform ghostRectTransform, RectTransform startTransform, RectTransform endTransform, Vector2 customEndPosition = default(Vector2), bool useCustomEndPosition = false)
    {
        RectTransform parentCanvasRect = ghostParentTransform.GetComponent<RectTransform>();
        if (parentCanvasRect == null)
        {
            Debug.LogError("El 'ghostParentTransform' debe ser parte de un Canvas o tener un RectTransform. No se puede animar el fantasma.");
            yield break;
        }

        // Configurar el fantasma para que coincida con el bloque de origen
        ghostRectTransform.anchorMin = startTransform.anchorMin;
        ghostRectTransform.anchorMax = startTransform.anchorMax;
        ghostRectTransform.pivot = startTransform.pivot;
        ghostRectTransform.sizeDelta = startTransform.sizeDelta;
        ghostRectTransform.localScale = startTransform.localScale;

        // Calcular la posici�n inicial
        Vector2 startPos;
        if (startTransform.parent == parentCanvasRect)
        {
            startPos = startTransform.anchoredPosition;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvasRect,
                startTransform.position,
                null,
                out startPos
            );
        }

        // Calcular la posici�n final
        Vector2 endPos;
        if (useCustomEndPosition)
        {
            // Usar la posici�n personalizada
            endPos = customEndPosition;
            Debug.Log($"<color=cyan>MoveGhostBlock: Usando posici�n final personalizada: ({endPos.x}, {endPos.y})</color>");
        }
        else
        {
            // Usar la posici�n del endTransform
            if (endTransform.parent == parentCanvasRect)
            {
                endPos = endTransform.anchoredPosition;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvasRect,
                    endTransform.position,
                    null,
                    out endPos
                );
            }
        }

        // Bucle infinito para la animaci�n repetitiva
        while (true)
        {
            // Posicionar el fantasma en el inicio
            ghostRectTransform.anchoredPosition = startPos;
            ghostRectTransform.gameObject.SetActive(true);

            // Animar el movimiento del fantasma
            float elapsedTime = 0f;
            while (elapsedTime < ghostMoveDuration)
            {
                ghostRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / ghostMoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ghostRectTransform.anchoredPosition = endPos;

            // Esperar antes de repetir la animaci�n
            yield return new WaitForSeconds(ghostWaitDuration);

            // Ocultar brevemente el fantasma antes de repetir
            ghostRectTransform.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void RestartSlotLottieAnimation()
    {
        if (animationSlotEffect != null && animationSlotEffect.gameObject.activeInHierarchy)
        {
            // M�todo 1: Usando referencia directa al componente AnimatedImage
            AnimatedImage lottieComponent = animationSlotEffect.GetComponent<AnimatedImage>();

            if (lottieComponent != null)
            {
                try
                {
                    // Detener y reiniciar la animaci�n
                    lottieComponent.Stop();
                    lottieComponent.Play();
                    Debug.Log("<color=cyan>Animaci�n Lottie del slot reiniciada exitosamente usando AnimatedImage.</color>");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error al reiniciar animaci�n Lottie: {e.Message}. Intentando m�todo alternativo...");

                    // M�todo alternativo: Desactivar y reactivar el GameObject
                    StartCoroutine(RestartLottieByToggle());
                }
            }
            else
            {
                Debug.LogWarning("No se encontr� el componente AnimatedImage. Usando m�todo alternativo...");
                StartCoroutine(RestartLottieByToggle());
            }
        }
    }
    private IEnumerator RestartLottieByToggle()
    {
        if (animationSlotEffect != null)
        {
            // Guardar la posici�n y configuraci�n actual
            Vector2 currentPosition = animationSlotEffect.rectTransform.anchoredPosition;
            Vector2 currentSize = animationSlotEffect.rectTransform.sizeDelta;
            bool wasActive = animationSlotEffect.gameObject.activeInHierarchy;

            // Desactivar brevemente
            animationSlotEffect.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();

            // Reactivar y restaurar configuraci�n
            if (wasActive)
            {
                animationSlotEffect.gameObject.SetActive(true);
                animationSlotEffect.rectTransform.anchoredPosition = currentPosition;
                animationSlotEffect.rectTransform.sizeDelta = currentSize;

                Debug.Log("<color=cyan>Animaci�n Lottie del slot reiniciada usando toggle method.</color>");
            }
        }
    }
    private IEnumerator DelayedLottieRestart()
    {
        yield return new WaitForEndOfFrame(); // Esperar un frame para asegurar que el GameObject est� completamente activo
        RestartSlotLottieAnimation();
    }

    private IEnumerator InitializeTutorial()
    {
        AudioManager.Instance.StopSFX();   
        yield return new WaitForSeconds(10f);
        ShowCurrentStep();
    }

}