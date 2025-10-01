using LottiePlugin.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectibleManager : MonoBehaviour
{
    private CanvasGroup winPanelCanvasGroup;

    public static CollectibleManager Instance { get; private set; }

    public TextMeshProUGUI collectibleCounterText;

    private List<GameObject> activeCollectibles = new List<GameObject>();
    private int remainingCollectibles;
    private int initialCollectibles;

    private bool canCharacterMove = true;

    [Header("Panel de Victoria")]
    [SerializeField] private GameObject gameWinLevelPanel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI collectedText;
    [SerializeField] private TextMeshProUGUI winCorrectText;
    [SerializeField] private TextMeshProUGUI winIncorrectText;
    [SerializeField] private TextMeshProUGUI winTotalMovesText;

    [SerializeField] private TextMeshProUGUI winSessionNameText;
    [SerializeField] private TextMeshProUGUI winThisLevelScoreText;
    [SerializeField] private TextMeshProUGUI winTotalScoreText;

    [Header("Animaciones")]
    [SerializeField] private AnimatedImage animationConfeti;
    [SerializeField] private AnimatedImage animationTrophy;
    [SerializeField] private AnimatedImage animationHappy;
    [SerializeField] private AnimatedImage animationThumb;

    void Start()
    {
        winPanelCanvasGroup = gameWinLevelPanel.GetComponent<CanvasGroup>();
        winPanelCanvasGroup.alpha = 0f;
        gameWinLevelPanel.SetActive(false);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscar nuevamente el TextMeshProUGUI en la nueva escena
        collectibleCounterText = GameObject.Find("RemainingObjects")?.GetComponent<TextMeshProUGUI>();

        if (collectibleCounterText == null)
        {
            Debug.LogWarning("No se encontr� el TextMeshProUGUI en la nueva escena.");
        }
        else
        {
            UpdateCounterUI(); // Actualiza el contador con los valores actuales
        }

        // Buscar nuevamente el panel de victoria en la nueva escena
        gameWinLevelPanel = GameObject.Find("WinLevel");

        if (gameWinLevelPanel == null)
        {
            Debug.LogWarning("No se encontr� el panel de victoria en la nueva escena.");
        }
        else
        {
            winPanelCanvasGroup = gameWinLevelPanel.GetComponent<CanvasGroup>();
            gameWinLevelPanel.SetActive(false);
        }
    }

    public void SetCharacterCanMove(bool canMove)
    {
        canCharacterMove = canMove;
    }

    public bool CanCharacterMove()
    {
        return canCharacterMove;
    }

    public void RegisterCollectible(GameObject collectible)
    {
        if (!activeCollectibles.Contains(collectible))
        {
            activeCollectibles.Add(collectible);
        }
        UpdateCounterUI();
        Debug.Log($"Registrado coleccionable: {collectible.name}. Total ahora: {activeCollectibles.Count}");
    }

    public void Collect(GameObject collectible)
    {
        if (activeCollectibles.Contains(collectible))
        {
           
            collectible.SetActive(false);
            remainingCollectibles--;

            UpdateCounterUI();

            if (remainingCollectibles <= 0)
            {
                WinLevel();
                Debug.Log("�Nivel completado!");
            }
        }
    }

    public void InitializeCollectibles(int totalCollectibles)
    {
        initialCollectibles = totalCollectibles;
        remainingCollectibles = totalCollectibles;
        UpdateCounterUI();
        Debug.Log($"Inicializando con {remainingCollectibles} coleccionables.");
    }

    public void ResetManager()
    {
        activeCollectibles.Clear();
        remainingCollectibles = 0;
        UpdateCounterUI();
    }

    private void UpdateCounterUI()
    {
        if (collectibleCounterText != null)
        {
            collectibleCounterText.text = $"Objetos Restantes: {remainingCollectibles}";
        }
    }

    private void WinLevel()
    {
        // Detiene el timer
        Timer timer = FindFirstObjectByType<Timer>();
        float elapsed = timer != null ? timer.ElapsedTime : 0f;
        timer?.StopTimer();

        // Actualiza el texto de tiempo en el panel
        if (timeText != null)
        {
            // Formatea a minutos:segundos, por ejemplo "02:15"
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            timeText.text = $"Tiempo Total:  {minutes:00}:{seconds:00}";
        }

        // Mostrar cu�ntos recogiste de los totales
        int collected = initialCollectibles - remainingCollectibles;
        if (collectedText != null)
            collectedText.text = $"Objetos Recolectados:  {collected}/{initialCollectibles}";

        // Obtener los movimientos de ItemSlot
        ItemSlot slot = FindFirstObjectByType<ItemSlot>();
        if (slot != null)
        {
            winCorrectText.text = $"Movimientos Correctos:  {slot.CorrectMoves}";
            winIncorrectText.text = $"Movimientos Incorrectos:  {slot.IncorrectMoves}";
            winTotalMovesText.text = $"Movimientos Totales:  {slot.TotalMoves}";
        }
        else
        {
            Debug.LogWarning("No se encontr� ning�n ItemSlot en la escena para leer los movimientos.");
        }

        var scoreComp = FindFirstObjectByType<Score>();
        if (scoreComp != null)
        {
            scoreComp.FinishLevel();

            float thisLevelScore = scoreComp.CurrentScore;

            float totalAccumulated = GameSessionManager.Instance.TotalScore;

   
            if (winThisLevelScoreText != null)
                winThisLevelScoreText.text = $"Puntos del Nivel: {thisLevelScore:0}";

            if (winTotalScoreText != null)
                winTotalScoreText.text = $"Puntos Acumulado:    {totalAccumulated:0}";
        }
        else
        {
            Debug.LogWarning("No se encontro componente Score para leer la puntuaci�n.");
        }

        // 3) **Mostrar el nombre de la sesi�n**:
        string playerName = GameSessionManager.Instance.PlayerName;
        if (winSessionNameText != null)
            winSessionNameText.text = $"{playerName}!";

        // 3) Mostrar el panel y animarlo

        StartCoroutine(PlayVictoryAndShowPanel());

        Debug.Log("�Juego terminado!");
    }

    private IEnumerator FadeWinLevel()
    {

        float duration = 1.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            winPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        winPanelCanvasGroup.alpha = 1f;

        // Reproduce la animaciones si está asignada
        if (animationConfeti != null && animationTrophy != null && animationHappy != null && animationThumb != null)
        {
            // Reproduce la animación desde el principio
            animationConfeti.Play();
            animationTrophy.Play();
            animationHappy.Play();
            animationThumb.Play();
            Debug.Log("¡animaciones activadas!");
        }
    }

    private IEnumerator PlayVictoryAndShowPanel()
    {
        var character = FindFirstObjectByType<MazeGenerator>()?.character;

        if (character != null)
        {
            // Mirar a la c�mara
            if (Camera.main != null)
            {
                Vector3 cameraPosition = Camera.main.transform.position;
                cameraPosition.y = character.transform.position.y;
                character.transform.LookAt(cameraPosition);
            }

            // Ejecutar animaci�n y sonido
            var movement = character.GetComponent<CharacterMovement>();
            var sound = character.GetComponent<CharacterAudio>();

            if (movement != null)
                movement.WinLevelCharacter(2f); // 2 segundos de animaci�n

            if (sound != null)
                sound.PlaySoundHappy();
        }

        // Esperar 3 segundos para dejar tiempo a la animaci�n antes de mostrar el panel
        yield return new WaitForSeconds(6f);

        // 1) Desbloqueamos el siguiente nivel
        GameController gc = FindFirstObjectByType<GameController>();
        if (gc != null)
        {
            gc.UnlockNewLevel();
        }
        else
        {
            Debug.LogWarning("No se encontró GameController para desbloquear el nivel.");
        }

        // Mostrar el panel de victoria y animarlo
        gameWinLevelPanel.SetActive(true);
        StartCoroutine(FadeWinLevel());

        gameWinLevelPanel.transform.SetAsLastSibling();
    }
}
