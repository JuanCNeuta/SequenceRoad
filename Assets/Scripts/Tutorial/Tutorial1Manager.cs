using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using LottiePlugin.UI;
using TMPro;

public class Tutorial1Manager : MonoBehaviour
{
    private CanvasGroup winPanelCanvasGroup;

    [System.Serializable]

    public class TutorialStep
    {
        public GameObject targetBlockObject;
        public GameObject expectedBlockButton;
        public GameObject targetItemSlotObject;

        public Vector2 selectorRelativeOffset = new Vector2(0f, 0f);
        public Vector2 arrowRelativeOffset = new Vector2(0f, 0f);
        public bool isArrowPointingLeft = false;
        public bool isArrowPointingUp = false;

        public Vector2 itemSlotAnimationOffset = new Vector2(0f, 0f);

        public RectTransform TargetRectTransform =>
            targetBlockObject != null ? targetBlockObject.GetComponent<RectTransform>() : null;

        public RectTransform TargetItemSlotRectTransform =>
            targetItemSlotObject != null ? targetItemSlotObject.GetComponent<RectTransform>() : null;

        public RectTransform ExpectedBlockButtonRectTransform => 
            expectedBlockButton != null ? expectedBlockButton.GetComponent<RectTransform>() : null; 

        [Header("Ghost Block for this step")]
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
    [SerializeField] private GameObject gameWinLevelPanel;
    [SerializeField] private TextMeshProUGUI winSessionNameText;

    // Ficha Fantasma
    [Header("Tutorial Ficha Fantasma")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.4f); // Color para el efecto descolorido
    public float ghostMoveDuration = 1000f; // Duraci�n del movimiento del fantasma
    public float ghostWaitDuration = 20f; // Tiempo de espera antes de repetir la animaci�n
    public Transform ghostParentTransform; // El Canvas o RectTransform padre donde se instanciar�n los fantasmas

    private Coroutine ghostAnimationCoroutine; // Para controlar la corrutina del fantasma
    private GameObject currentGhostInstance; // La instancia del fantasma actualmente activa

    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("Bloques de Movimiento")]
    public List<GameObject> allMovementBlockButtons;

    private int currentStepIndex = 0;

    [Header("Audio Configuration")]
    public float delayBeforeSteps = 0f; 
    public float delayBeforeAudio = 0f; 

    [Header("Intro Panel")]
    [SerializeField] private GameObject introPanel;

    //Sonido cuando se gana el tutorial
    [Header("Audio Win Tutorial")]
    public AudioClip winTutorial;


    private void Awake()
    {
        // Inicializaci�n del panel de victoria
        if (gameWinLevelPanel != null)
        {
            winPanelCanvasGroup = gameWinLevelPanel.GetComponent<CanvasGroup>();
            if (winPanelCanvasGroup == null)
            {
                Debug.LogError("El gameWinLevelPanel no tiene un componente CanvasGroup. Aseg�rate de a�adir uno.");
            }
            gameWinLevelPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("El gameWinLevelPanel no est� asignado en el Inspector en TutorialManager.");
        }
        // Asegurarse de que no haya instancias de fantasmas activas al inicio
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
    }


    void OnDisable()
    {
        ItemSlot.OnCharacterMovedSuccessfully -= AdvanceStep;
        DragDrop.OnDragStart -= OnDragStartHandler;
        DragDrop.OnDragEnd -= OnDragEndHandler;
        DragDrop.OnObjectDiscarded -= OnDraggedObjectDiscarded;
    }

    void Start()
    {
        if (animationSlotEffect != null)
            animationSlotEffect.gameObject.SetActive(false);

        StartCoroutine(DelayedInitializeTutorial());

    }

    private void OnDragStartHandler()
    {
        Debug.Log("<color=purple>TutorialLottieManager: DragDrop.OnDragStart detectado.</color>");
        if (currentStepIndex < steps.Count && steps[currentStepIndex].targetItemSlotObject != null)
        {
            // ACTIVA Y POSICIONA el efecto del slot SOLO cuando el usuario comienza un arrastre.
            SetItemSlotSelectorActive(true, steps[currentStepIndex].TargetItemSlotRectTransform, steps[currentStepIndex].itemSlotAnimationOffset);

            //  Asegurar que la animaci�n se reinicie expl�citamente
            StartCoroutine(DelayedLottieRestart());

            Debug.Log("<color=purple>TutorialLottieManager: animationSlotEffect ACTIVADO y reiniciado en OnDragStartHandler (inicio de drag).</color>");
        }

        // PAUSAR LA ANIMACI�N DEL FANTASMA AL INICIAR UN ARRASTRE
        StopGhostAnimation();
    }

    private void OnDragEndHandler()
    {
        Debug.Log("<color=purple>TutorialLottieManager: DragDrop.OnDragEnd detectado.</color>");
    }

    // Se llama cuando el personaje se ha movido exitosamente (ItemSlot.OnCharacterMovedSuccessfully)
    public void AdvanceStep()
    {
        Debug.Log("<color=red>TutorialLottieManager: AdvanceStep() llamado. Intentando DESACTIVAR animationSlotEffect.</color>");

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
            ShowCurrentStep(); // Configura el siguiente paso
        }
        else
        {

            // Tutorial completado
            animationSelector.gameObject.SetActive(false);
            animationArrow.gameObject.SetActive(false);
            if (animationSlotEffect != null) animationSlotEffect.gameObject.SetActive(false); // Desactiva al finalizar el tutorial
            EnableAllMovementBlocks();
            this.enabled = false;

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
                Debug.LogWarning("winSessionNameText no est� asignado en Tutorial1Manager.");
            }
            Debug.Log("Panel de victoria del tutorial mostrado.");

            gameWinLevelPanel.SetActive(true);
            AudioManager.Instance.PlaySound(winTutorial);
            Debug.Log("Tutorial completado.");
        }
    }

    // Prepara el estado visual y de interactividad para el paso actual (sin activar animationSlotEffect)
    private void ShowCurrentStep()
    {
        Debug.Log($"<color=green>TutorialLottieManager: ShowCurrentStep() llamado para el paso {currentStepIndex}.</color>");

        if (currentStepIndex >= steps.Count)
        {
            animationSelector.gameObject.SetActive(false);
            animationArrow.gameObject.SetActive(false);
            if (animationSlotEffect != null) animationSlotEffect.gameObject.SetActive(false);
            EnableAllMovementBlocks();
            return;
        }

        TutorialStep currentStepData = steps[currentStepIndex]; // Define la variable aqu� para usarla en este m�todo


        if (steps[currentStepIndex].targetItemSlotObject == null)
        {
            SetItemSlotSelectorActive(false);
            Debug.Log($"<color=green>Tutorial1Manager: animationSlotEffect DESACTIVADO porque no hay targetItemSlotObject para el paso {currentStepIndex}.</color>");
        }

        ControlMovementBlockInteractivity(steps[currentStepIndex].expectedBlockButton);
        ShowCurrentStepAnimationsOnly();

        if (currentStepData.TargetItemSlotRectTransform != null)
        {
            // Activa y posiciona el efecto del slot
            SetItemSlotSelectorActive(true, currentStepData.TargetItemSlotRectTransform, currentStepData.itemSlotAnimationOffset);
            //RestartSlotLottieAnimation();
            Debug.Log($"<color=green>TutorialLottieManager: animationSlotEffect ACTIVADO y posicionado en ShowCurrentStep() para el paso {currentStepIndex}.</color>");
        }

        // INICIAR LA ANIMACI�N DEL FANTASMA PARA EL PASO ACTUAL
        StartGhostAnimation(currentStepData);

        var stepData = steps[currentStepIndex];
        if (stepData.stepAudioClip != null && AudioManager.Instance != null)
        {
            StartCoroutine(PlayStepAudioWhenReady(stepData.stepAudioClip));
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
            Debug.LogWarning($"Paso {currentStepIndex} no tiene targetBlockObject v lido. Selector y flecha de paleta DESACTIVADOS.");
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
            Debug.LogWarning("La lista 'All Movement Block Buttons' no est  asignada o est  vac a en TutorialLottieManager.");
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
            Debug.LogWarning("animationSlotEffect no est� asignado en TutorialLottieManager.");
            return;
        }

        animationSlotEffect.gameObject.SetActive(active);

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

            // Reiniciar la animaci�n cada vez que se activa
            RestartSlotLottieAnimation();
        }
    }
   
    private void OnDraggedObjectDiscarded()
    {
        Debug.Log("<color=red>TutorialLottieManager: Objeto arrastrado descartado detectado. Asegurando que animationSlotEffect est  desactivado.</color>");
        SetItemSlotSelectorActive(false); // Desactiva la animaci n del slot

        // REINICIAR LA ANIMACI�N DEL FANTASMA SI EL ARRASTRE FALL�
        if (currentStepIndex < steps.Count)
        {
            StartGhostAnimation(steps[currentStepIndex]); // Vuelve a mostrar el fantasma para el paso actual
        }
    }

    // M�TODO PARA INICIAR LA ANIMACI�N DEL FANTASMA
    private void StartGhostAnimation(TutorialStep step)
    {
        StopGhostAnimation(); // Asegurarse de detener y destruir cualquier animaci�n anterior

        // Validar que tenemos todas las referencias necesarias para el fantasma
        // Ahora, validamos la nueva referencia 'GhostEndTargetRectTransform'.
        if (step.ghostBlockPrefab == null || step.ExpectedBlockButtonRectTransform == null || step.GhostEndTargetRectTransform == null || ghostParentTransform == null) //
        {
            Debug.LogWarning("No se puede iniciar la animaci�n del fantasma: falta alguna referencia (prefab, origen, destino final o padre).");
            return;
        }

        // Instanciar el prefab del fantasma
        currentGhostInstance = Instantiate(step.ghostBlockPrefab, ghostParentTransform);
        RectTransform ghostRectTransform = currentGhostInstance.GetComponent<RectTransform>();
        Image ghostImage = currentGhostInstance.GetComponent<Image>();

        if (ghostImage != null)
        {
            ghostImage.color = ghostColor; // Aplicar el color y transparencia definidos
            ghostImage.raycastTarget = false;

            // INICIAMOS LA CORRUTINA DE ANIMACI�N PASANDO GhostEndTargetRectTransform COMO DESTINO
            ghostAnimationCoroutine = StartCoroutine(MoveGhostBlock(
                ghostRectTransform,
                step.ExpectedBlockButtonRectTransform,
                step.GhostEndTargetRectTransform //
            ));
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
    }


    // CORRUTINA PARA ANIMAR EL MOVIMIENTO DEL FANTASMA
    private IEnumerator MoveGhostBlock(RectTransform ghostRectTransform, RectTransform startTransform, RectTransform endTransform)
    {
        RectTransform parentCanvasRect = ghostParentTransform.GetComponent<RectTransform>();
        if (parentCanvasRect == null)
        {
            Debug.LogError("El 'ghostParentTransform' debe ser parte de un Canvas o tener un RectTransform. No se puede animar el fantasma.");
            yield break;
        }

        ghostRectTransform.anchorMin = startTransform.anchorMin;
        ghostRectTransform.anchorMax = startTransform.anchorMax;
        ghostRectTransform.pivot = startTransform.pivot;
        ghostRectTransform.sizeDelta = startTransform.sizeDelta;
        ghostRectTransform.localScale = startTransform.localScale;

        // Calcular la posici�n inicial de forma normal
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

        // Forzar la coordenada X de la posici�n inicial a 150f
        startPos.x = 150f;

        // Calcular la posici�n final de forma normal
        Vector2 endPos;
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


        endPos.x = 750f;

        while (true)
        {
            ghostRectTransform.anchoredPosition = startPos;
            ghostRectTransform.gameObject.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < ghostMoveDuration)
            {
                ghostRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / ghostMoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ghostRectTransform.anchoredPosition = endPos; // Asegurarse de que termine en la posici�n exacta

            yield return new WaitForSeconds(ghostWaitDuration);

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

    // M�todo alternativo para reiniciar la animaci�n
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
        // Reproducir audio inicial si existe
       
         yield return new WaitForSeconds(1f);
        ShowCurrentStep();
    }

    private IEnumerator PlayStepAudioWhenReady(AudioClip clip)
    {
        if (introPanel != null)
        {
            while (introPanel.activeSelf)
                yield return null;
        }

        // Usar el delay espec�fico para audio
        yield return new WaitForSecondsRealtime(delayBeforeAudio);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(clip);
        }
    }

    private void InitializeTutorialSteps()
    {
        // Show visuals for step 1
        ShowCurrentStep();
    }

    private IEnumerator DelayedInitializeTutorial()
    {
        if (introPanel != null)
        {
            // wait until intro panel is deactivated
            while (introPanel.activeSelf)
                yield return null;
        }
        else
        {
            Debug.LogWarning("Intro Panel reference not set on TutorialLottieManager.");
        }

        yield return new WaitForSecondsRealtime(delayBeforeSteps);

        InitializeTutorialSteps();
    }
}